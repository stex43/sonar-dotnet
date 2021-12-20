﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using Google.Protobuf;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules;
using SonarAnalyzer.UnitTest.Helpers;
using SonarAnalyzer.UnitTest.MetadataReferences;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    public enum CompilationErrorBehavior
    {
        FailTest,
        Ignore,
        Default = FailTest
    }

    public static class OldVerifier
    {
        public static void VerifyNoExceptionThrown(string path, IEnumerable<DiagnosticAnalyzer> diagnosticAnalyzers, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default)
        {
            var compilation = SolutionBuilder
                .Create()
                .AddProject(AnalyzerLanguage.FromPath(path))
                .AddDocument(path)
                .GetCompilation();

            var diagnostics = DiagnosticVerifier.GetAnalyzerDiagnostics(compilation, diagnosticAnalyzers, checkMode);
            DiagnosticVerifier.VerifyNoExceptionThrown(diagnostics);
        }

        /// <summary>
        /// Verify analyzer from C# on a snippet in non-concurrent execution mode.
        /// </summary>
        public static void VerifyCSharpAnalyzer(string snippet,
                                        SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                        IEnumerable<MetadataReference> additionalReferences) =>
            VerifyCSharpAnalyzer(snippet, diagnosticAnalyzer, null, CompilationErrorBehavior.Default, additionalReferences);

        /// <summary>
        /// Verify analyzer from C# on a snippet in non-concurrent execution mode.
        /// </summary>
        public static void VerifyCSharpAnalyzer(string snippet,
                                        SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                        CompilationErrorBehavior checkMode,
                                        IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyCSharpAnalyzer(snippet, diagnosticAnalyzer, null, checkMode, additionalReferences);

        /// <summary>
        /// Verify analyzer from C# on a snippet in non-concurrent execution mode.
        /// </summary>
        public static void VerifyCSharpAnalyzer(string snippet,
                                                SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                IEnumerable<ParseOptions> options = null,
                                                CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                IEnumerable<MetadataReference> additionalReferences = null,
                                                OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                                DiagnosticDescriptor[] onlyDiagnostics = null,
                                                string sonarProjectConfigPath = null)
        {
            var solution = SolutionBuilder.Create()
                                          .AddProject(AnalyzerLanguage.CSharp, outputKind: outputKind)
                                          .AddSnippet(snippet)
                                          .AddReferences(additionalReferences)
                                          .GetSolution();
            CompileAndVerifyAnalyzer(solution, new DiagnosticAnalyzer[] { diagnosticAnalyzer }, options, checkMode, sonarProjectConfigPath, onlyDiagnostics);
        }

        /// <summary>
        /// Verify analyzer from VB.NET on a snippet in non-concurrent execution mode.
        /// </summary>
        public static void VerifyVisualBasicAnalyzer(string snippet,
                                                     DiagnosticAnalyzer diagnosticAnalyzer,
                                                     CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                     IEnumerable<MetadataReference> additionalReferences = null)
        {
            var solution = SolutionBuilder.Create().AddProject(AnalyzerLanguage.VisualBasic).AddSnippet(snippet).AddReferences(additionalReferences).GetSolution();
            CompileAndVerifyAnalyzer(solution, new[] { diagnosticAnalyzer }, null, checkMode);
        }

        /// <summary>
        /// Verify analyzer from C# 9 with top level statements in non-concurrent execution mode.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9Console(string path,
                                                            DiagnosticAnalyzer diagnosticAnalyzer,
                                                            IEnumerable<MetadataReference> additionalReferences = null,
                                                            DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path },
                                        new[] { diagnosticAnalyzer },
                                        ParseOptionsHelper.FromCSharp9,
                                        CompilationErrorBehavior.Default,
                                        OutputKind.ConsoleApplication,
                                        additionalReferences,
                                        null,
                                        onlyDiagnostics);

        public static void VerifyAnalyzerFromCSharp10Console(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path },
                                        new[] { diagnosticAnalyzer },
                                        ParseOptionsHelper.FromCSharp10,
                                        CompilationErrorBehavior.Default,
                                        OutputKind.ConsoleApplication,
                                        additionalReferences);

        /// <summary>
        /// Verify analyzer from C# 9 with top level statements in non-concurrent execution mode.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9Console(string[] paths, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(paths, new[] { diagnosticAnalyzer }, ParseOptionsHelper.FromCSharp9, CompilationErrorBehavior.Default, OutputKind.ConsoleApplication, additionalReferences);

        public static void VerifyAnalyzerFromCSharp10Console(string[] paths, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(paths, new[] { diagnosticAnalyzer }, ParseOptionsHelper.FromCSharp10, CompilationErrorBehavior.Default, OutputKind.ConsoleApplication, additionalReferences);

        /// <summary>
        /// Verify analyzer from C# 9 with top level statements in non-concurrent execution mode.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9Console(string path,
                                                            DiagnosticAnalyzer[] diagnosticAnalyzers,
                                                            IEnumerable<MetadataReference> additionalReferences = null,
                                                            DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyNonConcurrentAnalyzer(
                new[] { path },
                diagnosticAnalyzers,
                ParseOptionsHelper.FromCSharp9,
                CompilationErrorBehavior.Default,
                OutputKind.ConsoleApplication,
                additionalReferences,
                null,
                onlyDiagnostics);

        /// <summary>
        /// Verify analyzer from C# 9 on a test library project in non-concurrent execution mode.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9LibraryInTest(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzerFromCSharp9InTest(path, diagnosticAnalyzer, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        /// <summary>
        /// Verify analyzer from C# 10 on a test library project in non-concurrent execution mode.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp10LibraryInTest(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzerFromCSharp10InTest(path, diagnosticAnalyzer, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        /// <summary>
        /// Verify analyzer from C# 9 on a test console project in non-concurrent execution mode.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9ConsoleInTest(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzerFromCSharp9InTest(path, diagnosticAnalyzer, OutputKind.ConsoleApplication, additionalReferences);

        /// <summary>
        /// Verify analyzer from C# 10 on a test console project in non-concurrent execution mode.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp10ConsoleInTest(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzerFromCSharp10InTest(path, diagnosticAnalyzer, OutputKind.ConsoleApplication, additionalReferences);

        /// <summary>
        /// Verify analyzer from C# 9 without top level statements in non-concurrent execution mode.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9Library(string path,
                                                            DiagnosticAnalyzer diagnosticAnalyzer,
                                                            IEnumerable<MetadataReference> additionalReferences = null,
                                                            DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyAnalyzerFromCSharp9Library(path, diagnosticAnalyzer, CompilationErrorBehavior.Default, additionalReferences, onlyDiagnostics);

        /// <summary>
        /// Verify analyzer from C# 9 without top level statements in non-concurrent execution mode.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9Library(string path,
                                                            DiagnosticAnalyzer diagnosticAnalyzer,
                                                            CompilationErrorBehavior behaviour,
                                                            IEnumerable<MetadataReference> additionalReferences = null,
                                                            DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path },
                                        new[] { diagnosticAnalyzer },
                                        ParseOptionsHelper.FromCSharp9,
                                        behaviour,
                                        OutputKind.DynamicallyLinkedLibrary,
                                        additionalReferences,
                                        null,
                                        onlyDiagnostics);

        public static void VerifyAnalyzerFromCSharp10Library(string path,
                                                             DiagnosticAnalyzer diagnosticAnalyzer,
                                                             IEnumerable<MetadataReference> additionalReferences = null,
                                                             DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyAnalyzerFromCSharp10Library(path, diagnosticAnalyzer, CompilationErrorBehavior.Default, additionalReferences, onlyDiagnostics);

        public static void VerifyAnalyzerFromCSharp10Library(string path,
                                                             DiagnosticAnalyzer[] diagnosticAnalyzers,
                                                             IEnumerable<MetadataReference> additionalReferences = null,
                                                             DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path },
                diagnosticAnalyzers,
                ParseOptionsHelper.FromCSharp10,
                CompilationErrorBehavior.Default,
                OutputKind.DynamicallyLinkedLibrary,
                additionalReferences,
                null,
                onlyDiagnostics);

        public static void VerifyAnalyzerFromCSharp10Library(string path,
                                                             DiagnosticAnalyzer diagnosticAnalyzer,
                                                             CompilationErrorBehavior behavior,
                                                             IEnumerable<MetadataReference> additionalReferences = null,
                                                             DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path },
                        new[] { diagnosticAnalyzer },
                        ParseOptionsHelper.FromCSharp10,
                        behavior,
                        OutputKind.DynamicallyLinkedLibrary,
                        additionalReferences,
                        null,
                        onlyDiagnostics);

        public static void VerifyAnalyzerCSharpPreviewLibrary(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path },
                new[] { diagnosticAnalyzer },
                ParseOptionsHelper.CSharpPreview,
                CompilationErrorBehavior.Default,
                OutputKind.DynamicallyLinkedLibrary,
                additionalReferences);

        /// <summary>
        /// Verify analyzer from C# 9 without top level statements in non-concurrent execution mode.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9Library(string[] paths, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(paths,
                                        new[] { diagnosticAnalyzer },
                                        ParseOptionsHelper.FromCSharp9,
                                        CompilationErrorBehavior.Default,
                                        OutputKind.DynamicallyLinkedLibrary,
                                        additionalReferences);

        public static void VerifyAnalyzerFromCSharp10Library(string[] paths, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(paths,
                                new[] { diagnosticAnalyzer },
                                ParseOptionsHelper.FromCSharp10,
                                CompilationErrorBehavior.Default,
                                OutputKind.DynamicallyLinkedLibrary,
                                additionalReferences);

        public static void VerifyNonConcurrentAnalyzer(string path,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<MetadataReference> additionalReferences) =>
            VerifyNonConcurrentAnalyzer(new[] { path }, new[] { diagnosticAnalyzer }, null, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyNonConcurrentAnalyzer(string path,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<ParseOptions> options = null,
                                                       CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                       OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                                       IEnumerable<MetadataReference> additionalReferences = null,
                                                       DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path }, new[] { diagnosticAnalyzer }, options, checkMode, outputKind, additionalReferences, null, onlyDiagnostics);

        public static void VerifyNonConcurrentAnalyzer(IEnumerable<string> paths,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<MetadataReference> additionalReferences) =>
            VerifyNonConcurrentAnalyzer(paths, new[] { diagnosticAnalyzer }, null, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyNonConcurrentAnalyzer(IEnumerable<string> paths,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<ParseOptions> options = null,
                                                       IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(paths, new[] { diagnosticAnalyzer }, options, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyAnalyzer(IEnumerable<string> paths,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<MetadataReference> additionalReferences) =>
            VerifyAnalyzer(paths, new[] { diagnosticAnalyzer }, null, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer[] diagnosticAnalyzers,
                                          IEnumerable<ParseOptions> options = null,
                                          CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                          OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                          IEnumerable<MetadataReference> additionalReferences = null,
                                          DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyAnalyzer(new[] { path }, diagnosticAnalyzers, options, checkMode, outputKind, additionalReferences, null, onlyDiagnostics);

        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<MetadataReference> additionalReferences,
                                          string sonarProjectConfigPath) =>
            VerifyAnalyzer(new[] { path },
                           new[] { diagnosticAnalyzer },
                           null, CompilationErrorBehavior.Default,
                           OutputKind.DynamicallyLinkedLibrary,
                           additionalReferences,
                           sonarProjectConfigPath);

        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<ParseOptions> options,
                                          IEnumerable<MetadataReference> additionalReferences,
                                          string sonarProjectConfigPath = null,
                                          DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyAnalyzer(
                new[] { path },
                new[] { diagnosticAnalyzer },
                options,
                CompilationErrorBehavior.Default,
                OutputKind.DynamicallyLinkedLibrary,
                additionalReferences,
                sonarProjectConfigPath,
                onlyDiagnostics);

        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<MetadataReference> additionalReferences,
                                          DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyAnalyzer(new[] { path }, new[] { diagnosticAnalyzer }, null, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences, null, onlyDiagnostics);

        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<ParseOptions> options = null,
                                          CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                          OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                          IEnumerable<MetadataReference> additionalReferences = null,
                                          DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyAnalyzer(new[] { path }, new[] { diagnosticAnalyzer }, options, checkMode, outputKind, additionalReferences, null, onlyDiagnostics);

        public static void VerifyAnalyzer(IEnumerable<string> paths,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<ParseOptions> options = null,
                                          IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyAnalyzer(paths, new[] { diagnosticAnalyzer }, options, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          CompilationErrorBehavior checkMode,
                                          IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyAnalyzer(new[] { path }, new[] { diagnosticAnalyzer }, null, checkMode, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyNonConcurrentUtilityAnalyzer<TMessage>(IEnumerable<string> paths,
                                                                        UtilityAnalyzerBase diagnosticAnalyzer,
                                                                        string protobufPath,
                                                                        string sonarProjectConfigPath,
                                                                        Action<IReadOnlyList<TMessage>> verifyProtobuf,
                                                                        IEnumerable<ParseOptions> options = null)
            where TMessage : IMessage<TMessage>, new()
        {
            var solutionBuilder = SolutionBuilder.CreateSolutionFromPaths(paths);
            foreach (var compilation in solutionBuilder.Compile(options?.ToArray()))
            {
                DiagnosticVerifier.Verify(compilation, diagnosticAnalyzer, CompilationErrorBehavior.Default, sonarProjectConfigPath);
                verifyProtobuf(ReadProtobuf(protobufPath).ToList());
            }

            static IEnumerable<TMessage> ReadProtobuf(string path)
            {
                using var input = File.OpenRead(path);
                var parser = new MessageParser<TMessage>(() => new TMessage());
                while (input.Position < input.Length)
                {
                    yield return parser.ParseDelimitedFrom(input);
                }
            }
        }

        /// <summary>
        /// Verify utility analyzer is not run in non-concurrent execution mode.
        /// </summary>
        public static void VerifyUtilityAnalyzerIsNotRun(IEnumerable<string> paths,
                                                         UtilityAnalyzerBase diagnosticAnalyzer,
                                                         string protobufPath,
                                                         IEnumerable<ParseOptions> options = null)
        {
            var solutionBuilder = SolutionBuilder.CreateSolutionFromPaths(paths);
            foreach (var compilation in solutionBuilder.Compile(options?.ToArray()))
            {
                DiagnosticVerifier.Verify(compilation, diagnosticAnalyzer, CompilationErrorBehavior.Default);
                new FileInfo(protobufPath).Length.Should().Be(0);
            }
        }

        public static void VerifyNoIssueReportedInTest(string path,
                                                       SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<MetadataReference> additionalReferences = null,
                                                       DiagnosticDescriptor[] onlyDiagnostics = null) =>
            VerifyNoIssueReportedInTest(path, diagnosticAnalyzer, null, additionalReferences, onlyDiagnostics);

        public static void VerifyNoIssueReportedInTest(string path,
                                                       SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<ParseOptions> options,
                                                       IEnumerable<MetadataReference> additionalReferences = null,
                                                       DiagnosticDescriptor[] onlyDiagnostics = null)
        {
            var builder = SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.FromPath(path))
                .AddTestReferences()
                .AddReferences(additionalReferences)
                .AddDocument(path);

            VerifyNoIssueReported(builder, diagnosticAnalyzer, options, CompilationErrorBehavior.Default, null, onlyDiagnostics);
        }

        public static void VerifyNoIssueReported(string path,
                                                 SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                 IEnumerable<MetadataReference> additionalReferences) =>
            VerifyNoIssueReported(path, diagnosticAnalyzer, null, checkMode: CompilationErrorBehavior.Default, additionalReferences: additionalReferences);

        public static void VerifyNoIssueReported(string path,
                                                 SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                 IEnumerable<ParseOptions> options,
                                                 IEnumerable<MetadataReference> additionalReferences,
                                                 string sonarProjectConfigPath = null) =>
            VerifyNoIssueReported(path, diagnosticAnalyzer, options, CompilationErrorBehavior.Default, additionalReferences: additionalReferences, sonarProjectConfigPath: sonarProjectConfigPath);

        public static void VerifyNoIssueReported(string path,
                                                 SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                 IEnumerable<ParseOptions> options = null,
                                                 CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                 OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                                 IEnumerable<MetadataReference> additionalReferences = null,
                                                 string sonarProjectConfigPath = null)
        {
            var builder = SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.FromPath(path), outputKind: outputKind)
                .AddReferences(additionalReferences)
                .AddDocument(path);

            VerifyNoIssueReported(builder, diagnosticAnalyzer, options, checkMode, sonarProjectConfigPath);
        }

        public static void VerifyCodeFix(string path,
                                         string pathToExpected,
                                         SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                         SonarCodeFixProvider codeFixProvider,
                                         IEnumerable<ParseOptions> options = null,
                                         OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                         IEnumerable<MetadataReference> additionalReferences = null) =>
            CodeFixVerifier.VerifyCodeFix(path, pathToExpected, pathToExpected, diagnosticAnalyzer, codeFixProvider, null, options, outputKind, additionalReferences);

        /// <summary>
        /// Verifies batch code fix.
        /// </summary>
        public static void VerifyCodeFix(string path,
                                         string pathToExpected,
                                         string pathToBatchExpected,
                                         SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                         SonarCodeFixProvider codeFixProvider,
                                         IEnumerable<ParseOptions> options = null,
                                         IEnumerable<MetadataReference> additionalReferences = null) =>
            CodeFixVerifier.VerifyCodeFix(path, pathToExpected, pathToBatchExpected, diagnosticAnalyzer, codeFixProvider, null, options, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        /// <summary>
        /// Verifies code fix with title.
        /// </summary>
        public static void VerifyCodeFix(string path,
                                         string pathToExpected,
                                         SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                         SonarCodeFixProvider codeFixProvider,
                                         string codeFixTitle,
                                         IEnumerable<ParseOptions> options = null,
                                         IEnumerable<MetadataReference> additionalReferences = null) =>
            CodeFixVerifier.VerifyCodeFix(path, pathToExpected, pathToExpected, diagnosticAnalyzer, codeFixProvider, codeFixTitle, options, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        private static void VerifyNonConcurrentAnalyzerFromCSharp9InTest(string path,
                                                                         DiagnosticAnalyzer diagnosticAnalyzer,
                                                                         OutputKind outputKind,
                                                                         IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path },
                                        new[] { diagnosticAnalyzer },
                                        ParseOptionsHelper.FromCSharp9,
                                        CompilationErrorBehavior.Default,
                                        outputKind,
                                        AddTestReference(additionalReferences));

        private static void VerifyNonConcurrentAnalyzerFromCSharp10InTest(string path,
                                                                 DiagnosticAnalyzer diagnosticAnalyzer,
                                                                 OutputKind outputKind,
                                                                 IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path },
                                        new[] { diagnosticAnalyzer },
                                        ParseOptionsHelper.FromCSharp10,
                                        CompilationErrorBehavior.Default,
                                        outputKind,
                                        AddTestReference(additionalReferences));

        private static void VerifyAnalyzer(IEnumerable<string> paths,
                                           DiagnosticAnalyzer[] diagnosticAnalyzers,
                                           IEnumerable<ParseOptions> options,
                                           CompilationErrorBehavior checkMode,
                                           OutputKind outputKind,
                                           IEnumerable<MetadataReference> additionalReferences,
                                           string sonarProjectConfigPath = null,
                                           DiagnosticDescriptor[] onlyDiagnostics = null)
        {
            using var scope = new EnvironmentVariableScope { EnableConcurrentAnalysis = true};
            var pathsWithConcurrencyTests = paths.Count() == 1 ? CreateConcurrencyTest(paths) : paths;
            var solution = SolutionBuilder.CreateSolutionFromPaths(pathsWithConcurrencyTests, outputKind, additionalReferences);
            CompileAndVerifyAnalyzer(solution, diagnosticAnalyzers, options, checkMode, sonarProjectConfigPath, onlyDiagnostics);
        }

        private static List<string> CreateConcurrencyTest(IEnumerable<string> paths)
        {
            var ret = new List<string>(paths);
            var language = AnalyzerLanguage.FromPath(paths.First());
            foreach (var path in paths)
            {
                var sourcePath = Path.GetFullPath(path);
                var newPath = Path.Combine(Path.GetDirectoryName(sourcePath), Path.GetFileNameWithoutExtension(path) + ".Concurrent" + Path.GetExtension(path));
                var content = File.ReadAllText(sourcePath, Encoding.UTF8);
                File.WriteAllText(newPath, language == AnalyzerLanguage.CSharp ? $"namespace AppendedNamespaceForConcurrencyTest {{ {content} }}" : InsertNamespaceForVB(content));
                ret.Add(newPath);
            }
            return ret;
        }

        private static string InsertNamespaceForVB(string content)
        {
            var match = Regex.Match(content, @"^\s*Imports\s+.+$", RegexOptions.Multiline | RegexOptions.RightToLeft);
            var idx = match.Success ? match.Index + match.Length + 1 : 0;
            return content.Insert(idx, "Namespace AppendedNamespaceForConcurrencyTest : ") + " : End Namespace";
        }

        private static void VerifyNonConcurrentAnalyzer(IEnumerable<string> paths,
                                                        DiagnosticAnalyzer[] diagnosticAnalyzers,
                                                        IEnumerable<ParseOptions> options,
                                                        CompilationErrorBehavior checkMode,
                                                        OutputKind outputKind,
                                                        IEnumerable<MetadataReference> additionalReferences,
                                                        string sonarProjectConfigPath = null,
                                                        DiagnosticDescriptor[] onlyDiagnostics = null)
        {
            var solution = SolutionBuilder.CreateSolutionFromPaths(paths, outputKind, additionalReferences);
            CompileAndVerifyAnalyzer(solution, diagnosticAnalyzers, options, checkMode, sonarProjectConfigPath, onlyDiagnostics);
        }

        private static void CompileAndVerifyAnalyzer(SolutionBuilder solution,
                                                     DiagnosticAnalyzer[] diagnosticAnalyzers,
                                                     IEnumerable<ParseOptions> options,
                                                     CompilationErrorBehavior checkMode,
                                                     string sonarProjectConfigPath = null,
                                                     DiagnosticDescriptor[] onlyDiagnostics = null)
        {
            foreach (var compilation in solution.Compile(options?.ToArray()))
            {
                DiagnosticVerifier.Verify(compilation, diagnosticAnalyzers, checkMode, sonarProjectConfigPath, onlyDiagnostics?.Select(x => x.Id).ToArray());
            }
        }

        private static void VerifyNoIssueReported(ProjectBuilder builder,
                                                  SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                  IEnumerable<ParseOptions> options,
                                                  CompilationErrorBehavior checkMode,
                                                  string sonarProjectConfigPath,
                                                  DiagnosticDescriptor[] onlyDiagnostics = null)
        {
            foreach (var option in options ?? new ParseOptions[] { null })
            {
                DiagnosticVerifier.VerifyNoIssueReported(builder.GetCompilation(option), diagnosticAnalyzer, checkMode, sonarProjectConfigPath, onlyDiagnostics?.Select(x => x.Id).ToArray());
            }
        }

        private static IEnumerable<MetadataReference> AddTestReference(IEnumerable<MetadataReference> additionalReferences) =>
            NuGetMetadataReference.MSTestTestFrameworkV1.Concat(additionalReferences ?? Enumerable.Empty<MetadataReference>());
    }
}