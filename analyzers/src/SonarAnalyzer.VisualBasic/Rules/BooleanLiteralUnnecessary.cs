﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class BooleanLiteralUnnecessary : BooleanLiteralUnnecessaryBase<BinaryExpressionSyntax, SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override bool IsBooleanLiteral(SyntaxNode node) => IsTrueLiteralKind(node) || IsFalseLiteralKind(node);

        protected override SyntaxNode GetLeftNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Left;

        protected override SyntaxNode GetRightNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Right;

        protected override SyntaxToken GetOperatorToken(BinaryExpressionSyntax binaryExpression) => binaryExpression.OperatorToken;

        protected override bool IsTrueLiteralKind(SyntaxNode syntaxNode) => syntaxNode.IsKind(SyntaxKind.TrueLiteralExpression);

        protected override bool IsFalseLiteralKind(SyntaxNode syntaxNode) => syntaxNode.IsKind(SyntaxKind.FalseLiteralExpression);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(CheckLogicalNot, SyntaxKind.NotExpression);
            context.RegisterNodeAction(CheckAndExpression, SyntaxKind.AndAlsoExpression);
            context.RegisterNodeAction(CheckAndExpression, SyntaxKind.AndExpression);
            context.RegisterNodeAction(CheckOrExpression, SyntaxKind.OrElseExpression);
            context.RegisterNodeAction(CheckOrExpression, SyntaxKind.OrExpression);
            context.RegisterNodeAction(CheckEquals, SyntaxKind.EqualsExpression);
            context.RegisterNodeAction(CheckNotEquals, SyntaxKind.NotEqualsExpression);
            context.RegisterNodeAction(CheckConditional, SyntaxKind.TernaryConditionalExpression);
        }

        private void CheckLogicalNot(SonarSyntaxNodeReportingContext context)
        {
            var logicalNot = (UnaryExpressionSyntax)context.Node;
            var logicalNotOperand = logicalNot.Operand.RemoveParentheses();
            if (IsBooleanLiteral(logicalNotOperand))
            {
                context.ReportIssue(Diagnostic.Create(Rule, logicalNot.Operand.GetLocation()));
            }
        }

        private void CheckConditional(SonarSyntaxNodeReportingContext context)
        {
            var conditional = (TernaryConditionalExpressionSyntax)context.Node;
            var whenTrue = conditional.WhenTrue;
            var whenFalse = conditional.WhenFalse;
            var typeLeft = context.SemanticModel.GetTypeInfo(whenTrue).Type;
            var typeRight = context.SemanticModel.GetTypeInfo(whenFalse).Type;
            if (typeLeft.IsNullableBoolean()
                || typeRight.IsNullableBoolean()
                || typeLeft == null
                || typeRight == null)
            {
                return;
            }
            CheckTernaryExpressionBranches(context, conditional.SyntaxTree, whenTrue, whenFalse);
        }
    }
}
