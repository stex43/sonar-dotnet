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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.CFG.LiveVariableAnalysis
{
    public sealed class RoslynLiveVariableAnalysis : LiveVariableAnalysisBase<ControlFlowGraph, BasicBlock>
    {
        protected override BasicBlock ExitBlock => cfg.ExitBlock;

        public RoslynLiveVariableAnalysis(ControlFlowGraph cfg) : base(cfg) =>
            Analyze();

        protected override IEnumerable<BasicBlock> ReversedBlocks() =>
            cfg.Blocks.Reverse();

        protected override IEnumerable<BasicBlock> Successors(BasicBlock block)
        {
            foreach (var successor in block.Successors)
            {
                if (successor.Destination != null && successor.FinallyRegions.Any())
                {   // When exiting finally region, redirect to finally instead of the normal destination
                    foreach (var finallyRegion in successor.FinallyRegions)
                    {
                        yield return cfg.Blocks[finallyRegion.FirstBlockOrdinal];
                    }
                }
                else if (successor.Destination != null)
                {
                    yield return successor.Destination;
                }
                else if (successor.Source.EnclosingRegion.Kind == ControlFlowRegionKind.Finally)
                {   // Redirect exit from throw and finally to following blocks.
                    foreach (var trySuccessor in TryRegionSuccessors(block.EnclosingRegion))
                    {
                        yield return trySuccessor.Destination;
                    }
                }
            }
        }

        protected override IEnumerable<BasicBlock> Predecessors(BasicBlock block)
        {
            if (block.Predecessors.Any())
            {
                foreach (var predecessor in block.Predecessors)
                {
                    // When exiting finally region, redirect predecessor to the source of StructuredEceptionHandling branches
                    if (predecessor.FinallyRegions.Any())
                    {
                        foreach (var structuredExceptionHandling in StructuredExceptionHandlinBranches(predecessor.FinallyRegions))
                        {
                            yield return structuredExceptionHandling.Source;
                        }
                    }
                    else
                    {
                        yield return predecessor.Source;
                    }
                }
            }
            else if (block.EnclosingRegion.Kind == ControlFlowRegionKind.Finally && block.Ordinal == block.EnclosingRegion.FirstBlockOrdinal)
            {
                // Link first block of FinallyRegion to the source of all branches exiting that FinallyRegion
                foreach (var trySuccessor in TryRegionSuccessors(block.EnclosingRegion))
                {
                    yield return trySuccessor.Source;
                }
            }

            IEnumerable<ControlFlowBranch> StructuredExceptionHandlinBranches(IEnumerable<ControlFlowRegion> finallyRegions) =>
                finallyRegions.Select(x => cfg.Blocks[x.LastBlockOrdinal].Successors.SingleOrDefault(x => x.Semantics == ControlFlowBranchSemantics.StructuredExceptionHandling))
                    .Where(x => x != null);
        }

        internal static bool IsOutArgument(IOperation operation) =>
            new IOperationWrapperSonar(operation) is var wrapped
            && IArgumentOperationWrapper.IsInstance(wrapped.Parent)
            && IArgumentOperationWrapper.FromOperation(wrapped.Parent).Parameter.RefKind == RefKind.Out;

        protected override State ProcessBlock(BasicBlock block)
        {
            var ret = new RoslynState();
            ret.ProcessBlock(cfg, block);
            return ret;
        }

        private IEnumerable<ControlFlowBranch> TryRegionSuccessors(ControlFlowRegion finallyRegion)
        {
            var tryRegion = finallyRegion.EnclosingRegion.NestedRegions.Single(x => x.Kind == ControlFlowRegionKind.Try);
            return tryRegion.Blocks(cfg).SelectMany(x => x.Successors).Where(x => x.FinallyRegions.Contains(finallyRegion));
        }

        private class RoslynState : State
        {
            public void ProcessBlock(ControlFlowGraph cfg, BasicBlock block)
            {
                foreach (var operation in block.OperationsAndBranchValue.ToReversedExecutionOrder())
                {
                    switch (operation.Instance.Kind)
                    {
                        case OperationKindEx.LocalReference:
                            ProcessParameterOrLocalReference(ILocalReferenceOperationWrapper.FromOperation(operation.Instance));
                            break;
                        case OperationKindEx.ParameterReference:
                            ProcessParameterOrLocalReference(IParameterReferenceOperationWrapper.FromOperation(operation.Instance));
                            break;
                        case OperationKindEx.SimpleAssignment:
                            ProcessSimpleAssignment(ISimpleAssignmentOperationWrapper.FromOperation(operation.Instance));
                            break;
                        case OperationKindEx.FlowAnonymousFunction:
                            ProcessFlowAnonymousFunction(cfg, IFlowAnonymousFunctionOperationWrapper.FromOperation(operation.Instance));
                            break;
                    }
                }
            }

            private void ProcessParameterOrLocalReference(IOperationWrapper reference)
            {
                var symbol = ParameterOrLocalSymbol(reference.WrappedOperation);
                Debug.Assert(symbol != null, "ProcessParameterOrLocalReference should resolve parametr or local symbol.");
                if (IsOutArgument(reference.WrappedOperation))
                {
                    Assigned.Add(symbol);
                    UsedBeforeAssigned.Remove(symbol);
                }
                else if (!IsAssignmentTarget())
                {
                    UsedBeforeAssigned.Add(symbol);
                }

                bool IsAssignmentTarget() =>
                    new IOperationWrapperSonar(reference.WrappedOperation).Parent is { } parent
                    && parent.Kind == OperationKindEx.SimpleAssignment
                    && ISimpleAssignmentOperationWrapper.FromOperation(parent).Target == reference.WrappedOperation;
            }

            private void ProcessSimpleAssignment(ISimpleAssignmentOperationWrapper assignment)
            {
                if (ParameterOrLocalSymbol(assignment.Target) is { } localTarget)
                {
                    Assigned.Add(localTarget);
                    UsedBeforeAssigned.Remove(localTarget);
                }
            }

            private void ProcessFlowAnonymousFunction(ControlFlowGraph cfg, IFlowAnonymousFunctionOperationWrapper anonymousFunction)
            {
                var anonymousFunctionCfg = cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunction);
                foreach (var operation in anonymousFunctionCfg.Blocks.SelectMany(x => x.OperationsAndBranchValue).SelectMany(x => x.DescendantsAndSelf()))
                {
                    switch (operation.Kind)
                    {
                        case OperationKindEx.LocalReference:
                            Captured.Add(ILocalReferenceOperationWrapper.FromOperation(operation).Local);
                            break;
                        case OperationKindEx.ParameterReference:
                            Captured.Add(IParameterReferenceOperationWrapper.FromOperation(operation).Parameter);
                            break;
                        case OperationKindEx.FlowAnonymousFunction:
                            ProcessFlowAnonymousFunction(anonymousFunctionCfg, IFlowAnonymousFunctionOperationWrapper.FromOperation(operation));
                            break;
                    }
                }
            }

            private static ISymbol ParameterOrLocalSymbol(IOperation operation) =>
                operation switch
                {
                    var _ when IParameterReferenceOperationWrapper.IsInstance(operation) => IParameterReferenceOperationWrapper.FromOperation(operation).Parameter,
                    var _ when ILocalReferenceOperationWrapper.IsInstance(operation) => ILocalReferenceOperationWrapper.FromOperation(operation).Local,
                    _ => null
                };
        }
    }
}