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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules;

[TestClass]
public class UseFindSystemTimeZoneByIdTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UseFindSystemTimeZoneById>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UseFindSystemTimeZoneById>();

#if NET

    [TestMethod]
    public void UseFindSystemTimeZoneById_Net_CS() =>
        builderCS.AddReferences(NuGetMetadataReference.TimeZoneConverter())
            .AddPaths("UseFindSystemTimeZoneById.Net.cs")
            .Verify();

    [TestMethod]
    public void UseFindSystemTimeZoneById_Net_VB() =>
        builderVB.AddReferences(NuGetMetadataReference.TimeZoneConverter())
            .AddPaths("UseFindSystemTimeZoneById.Net.vb")
            .Verify();

    [TestMethod]
    public void UseFindSystemTimeZoneById_Net_WithoutReference_DoesNotRaise_CS() =>
        builderCS.AddPaths("UseFindSystemTimeZoneById.Net.cs")
            .WithErrorBehavior(CompilationErrorBehavior.Ignore)
            .VerifyNoIssueReported();

    [TestMethod]
    public void UseFindSystemTimeZoneById_Net_WithoutReference_DoesNotRaise_VB() =>
        builderVB.AddPaths("UseFindSystemTimeZoneById.Net.vb")
            .WithErrorBehavior(CompilationErrorBehavior.Ignore)
            .VerifyNoIssueReported();

#else

    [TestMethod]
    public void UseFindSystemTimeZoneById_CS() =>
        builderCS.AddReferences(NuGetMetadataReference.TimeZoneConverter())
            .AddPaths("UseFindSystemTimeZoneById.cs").Verify();

    [TestMethod]
    public void UseFindSystemTimeZoneById_VB() =>
        builderVB.AddReferences(NuGetMetadataReference.TimeZoneConverter())
            .AddPaths("UseFindSystemTimeZoneById.vb").Verify();

#endif

}
