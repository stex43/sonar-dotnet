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

using SonarAnalyzer.Helpers.Facade;

namespace SonarAnalyzer.Helpers;

internal sealed class CSharpFacade : ILanguageFacade<SyntaxKind>
{
    private static readonly Lazy<CSharpFacade> Singleton = new(() => new CSharpFacade());
    private static readonly Lazy<AssignmentFinder> AssignmentFinderLazy = new(() => new CSharpAssignmentFinder());
    private static readonly Lazy<IExpressionNumericConverter> ExpressionNumericConverterLazy = new(() => new CSharpExpressionNumericConverter());
    private static readonly Lazy<SyntaxFacade<SyntaxKind>> SyntaxLazy = new(() => new CSharpSyntaxFacade());
    private static readonly Lazy<ISyntaxKindFacade<SyntaxKind>> SyntaxKindLazy = new(() => new CSharpSyntaxKindFacade());
    private static readonly Lazy<ITrackerFacade<SyntaxKind>> TrackerLazy = new(() => new CSharpTrackerFacade());

    public AssignmentFinder AssignmentFinder => AssignmentFinderLazy.Value;
    public StringComparison NameComparison => StringComparison.Ordinal;
    public StringComparer NameComparer => StringComparer.Ordinal;
    public GeneratedCodeRecognizer GeneratedCodeRecognizer => CSharpGeneratedCodeRecognizer.Instance;
    public IExpressionNumericConverter ExpressionNumericConverter => ExpressionNumericConverterLazy.Value;
    public SyntaxFacade<SyntaxKind> Syntax => SyntaxLazy.Value;
    public ISyntaxKindFacade<SyntaxKind> SyntaxKind => SyntaxKindLazy.Value;
    public ITrackerFacade<SyntaxKind> Tracker => TrackerLazy.Value;

    public static CSharpFacade Instance => Singleton.Value;

    private CSharpFacade() { }

    public DiagnosticDescriptor CreateDescriptor(string id, string messageFormat, bool? isEnabledByDefault = null, bool fadeOutCode = false) =>
        DescriptorFactory.Create(id, messageFormat, isEnabledByDefault, fadeOutCode);

    public object FindConstantValue(SemanticModel model, SyntaxNode node) =>
        node.FindConstantValue(model);

    public IMethodParameterLookup MethodParameterLookup(SyntaxNode invocation, IMethodSymbol methodSymbol) =>
        invocation switch
        {
            null => null,
            AttributeSyntax x => new CSharpAttributeParameterLookup(x, methodSymbol),
            _ => new CSharpMethodParameterLookup(GetArgumentList(invocation), methodSymbol),
        };

    public IMethodParameterLookup MethodParameterLookup(SyntaxNode invocation, SemanticModel semanticModel) =>
        invocation != null ? new CSharpMethodParameterLookup(GetArgumentList(invocation), semanticModel) : null;

    private static BaseArgumentListSyntax GetArgumentList(SyntaxNode invocation) =>
        invocation switch
        {
            ArgumentListSyntax x => x,
            ObjectCreationExpressionSyntax x => x.ArgumentList,
            InvocationExpressionSyntax x => x.ArgumentList,
            _ when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(invocation) =>
                ((ImplicitObjectCreationExpressionSyntaxWrapper)invocation).ArgumentList,
            ConstructorInitializerSyntax x => x.ArgumentList,
            ElementAccessExpressionSyntax x => x.ArgumentList,
            _ => throw new ArgumentException($"{invocation.GetType()} does not contain an ArgumentList.", nameof(invocation)),
        };

    public string GetName(SyntaxNode expression) =>
        expression.GetName();
}
