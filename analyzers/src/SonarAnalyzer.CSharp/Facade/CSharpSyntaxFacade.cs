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

namespace SonarAnalyzer.Helpers.Facade;

internal sealed class CSharpSyntaxFacade : SyntaxFacade<SyntaxKind>
{
    public override SyntaxKind Kind(SyntaxNode node) => node.Kind();

    public override ComparisonKind ComparisonKind(SyntaxNode node) =>
        node.Kind() switch
        {
            SyntaxKind.EqualsExpression => Helpers.ComparisonKind.Equals,
            SyntaxKind.NotEqualsExpression => Helpers.ComparisonKind.NotEquals,
            SyntaxKind.LessThanExpression => Helpers.ComparisonKind.LessThan,
            SyntaxKind.LessThanOrEqualExpression => Helpers.ComparisonKind.LessThanOrEqual,
            SyntaxKind.GreaterThanExpression => Helpers.ComparisonKind.GreaterThan,
            SyntaxKind.GreaterThanOrEqualExpression => Helpers.ComparisonKind.GreaterThanOrEqual,
            _ => Helpers.ComparisonKind.None,
        };

    public override bool IsKind(SyntaxNode node, SyntaxKind kind) => node.IsKind(kind);

    public override bool IsKind(SyntaxToken token, SyntaxKind kind) => token.IsKind(kind);

    public override bool IsKind(SyntaxTrivia trivia, SyntaxKind kind) => trivia.IsKind(kind);

    public override bool IsAnyKind(SyntaxNode node, ISet<SyntaxKind> syntaxKinds) => node.IsAnyKind(syntaxKinds);

    public override bool IsAnyKind(SyntaxNode node, params SyntaxKind[] syntaxKinds) => node.IsAnyKind(syntaxKinds);

    public override bool IsAnyKind(SyntaxTrivia trivia, params SyntaxKind[] syntaxKinds) => trivia.IsAnyKind(syntaxKinds);

    public override bool IsNullLiteral(SyntaxNode node) => node.IsNullLiteral();

    public override bool IsKnownAttributeType(SemanticModel model, SyntaxNode attribute, KnownType knownType) =>
        AttributeSyntaxExtensions.IsKnownType(Cast<AttributeSyntax>(attribute), knownType, model);

    public override IEnumerable<SyntaxNode> ArgumentExpressions(SyntaxNode node) =>
        node switch
        {
            ObjectCreationExpressionSyntax creation => creation.ArgumentList?.Arguments.Select(x => x.Expression) ?? Enumerable.Empty<SyntaxNode>(),
            null => Enumerable.Empty<SyntaxNode>(),
            var _ when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(node)
                => ((ImplicitObjectCreationExpressionSyntaxWrapper)node).ArgumentList?.Arguments.Select(x => x.Expression) ?? Enumerable.Empty<SyntaxNode>(),
            _ => throw InvalidOperation(node, nameof(ArgumentExpressions))
        };

    public override ImmutableArray<SyntaxNode> AssignmentTargets(SyntaxNode assignment) =>
        Cast<AssignmentExpressionSyntax>(assignment).AssignmentTargets();

    public override SyntaxNode AssignmentLeft(SyntaxNode assignment) =>
        Cast<AssignmentExpressionSyntax>(assignment).Left;

    public override SyntaxNode AssignmentRight(SyntaxNode assignment) =>
        Cast<AssignmentExpressionSyntax>(assignment).Right;

    public override SyntaxNode BinaryExpressionLeft(SyntaxNode binary) =>
        Cast<BinaryExpressionSyntax>(binary).Left;

    public override SyntaxNode BinaryExpressionRight(SyntaxNode binary) =>
        Cast<BinaryExpressionSyntax>(binary).Right;

    public override SyntaxNode CastType(SyntaxNode cast) =>
        Cast<CastExpressionSyntax>(cast).Type;

    public override SyntaxNode CastExpression(SyntaxNode cast) =>
        Cast<CastExpressionSyntax>(cast).Expression;

    public override IEnumerable<SyntaxNode> EnumMembers(SyntaxNode @enum) =>
        @enum == null ? Enumerable.Empty<SyntaxNode>() : Cast<EnumDeclarationSyntax>(@enum).Members;

    public override SyntaxToken? InvocationIdentifier(SyntaxNode invocation) =>
        invocation == null ? null : Cast<InvocationExpressionSyntax>(invocation).GetMethodCallIdentifier();

    public override ImmutableArray<SyntaxToken> LocalDeclarationIdentifiers(SyntaxNode node) =>
        Cast<LocalDeclarationStatementSyntax>(node).Declaration.Variables.Select(x => x.Identifier).ToImmutableArray();

    public override ImmutableArray<SyntaxToken> FieldDeclarationIdentifiers(SyntaxNode node) =>
        Cast<FieldDeclarationSyntax>(node).Declaration.Variables.Select(x => x.Identifier).ToImmutableArray();

    public override SyntaxKind[] ModifierKinds(SyntaxNode node) =>
        node is TypeDeclarationSyntax typeDeclaration
            ? typeDeclaration.Modifiers.Select(x => x.Kind()).ToArray()
            : Array.Empty<SyntaxKind>();

    public override SyntaxNode NodeExpression(SyntaxNode node) =>
        node switch
        {
            ArrowExpressionClauseSyntax x => x.Expression,
            AttributeArgumentSyntax x => x.Expression,
            InterpolationSyntax x => x.Expression,
            InvocationExpressionSyntax x => x.Expression,
            LockStatementSyntax x => x.Expression,
            ReturnStatementSyntax x => x.Expression,
            MemberAccessExpressionSyntax x => x.Expression,
            null => null,
            _ => throw InvalidOperation(node, nameof(NodeExpression))
        };

    public override SyntaxToken? NodeIdentifier(SyntaxNode node) =>
        node.NodeIdentifier();

    public override SyntaxNode RemoveConditionalAccess(SyntaxNode node) =>
        node is ExpressionSyntax expression
            ? expression.RemoveConditionalAccess()
            : node;

    public override SyntaxNode RemoveParentheses(SyntaxNode node) =>
        node.RemoveParentheses();

    public override string StringValue(SyntaxNode node, SemanticModel semanticModel) =>
        CSharpSyntaxHelper.StringValue(node, semanticModel);

    public override bool TryGetInterpolatedTextValue(SyntaxNode node, SemanticModel semanticModel, out string interpolatedValue) =>
        Cast<InterpolatedStringExpressionSyntax>(node).TryGetInterpolatedTextValue(semanticModel, out interpolatedValue);

    public override bool IsStatic(SyntaxNode node) =>
        Cast<BaseMethodDeclarationSyntax>(node).IsStatic();

    public override bool TryGetOperands(SyntaxNode invocation, out SyntaxNode left, out SyntaxNode right) =>
        Cast<InvocationExpressionSyntax>(invocation).TryGetOperands(out left, out right);

    public override bool HasExactlyNArguments(SyntaxNode invocation, int count) =>
        Cast<InvocationExpressionSyntax>(invocation).HasExactlyNArguments(count);

    public override bool IsMemberAccessOnKnownType(SyntaxNode memberAccess, string name, KnownType knownType, SemanticModel semanticModel) =>
        Cast<MemberAccessExpressionSyntax>(memberAccess).IsMemberAccessOnKnownType(name, knownType, semanticModel);

    public override bool IsWrittenTo(SyntaxNode expression, SemanticModel semanticModel, CancellationToken cancellationToken) =>
        expression is ExpressionSyntax ex && ex.IsWrittenTo(semanticModel, cancellationToken);
}
