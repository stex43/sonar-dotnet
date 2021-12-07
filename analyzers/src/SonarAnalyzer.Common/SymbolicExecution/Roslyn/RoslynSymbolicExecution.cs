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
using System.Linq;
using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    internal class RoslynSymbolicExecution
    {
        private readonly ControlFlowGraph cfg;
        private readonly SymbolicExecutionCheck[] checks;
        private readonly Queue<ExplodedNode> queue = new Queue<ExplodedNode>();
        private readonly SymbolicValueCounter symbolicValueCounter = new SymbolicValueCounter();

        public RoslynSymbolicExecution(ControlFlowGraph cfg, SymbolicExecutionCheck[] checks)
        {
            this.cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            this.checks = checks ?? throw new ArgumentNullException(nameof(checks));
            if (!checks.Any())
            {
                throw new ArgumentException("At least one check is expected", nameof(checks));
            }
        }

        public void Execute()
        {
            // ToDo: Forbid running twice
            foreach (var block in cfg.Blocks)   // ToDo: This is a temporary simplification until we support proper branching
            {
                queue.Enqueue(new ExplodedNode(block, ProgramState.Empty));
                while (queue.Any())
                {
                    var current = queue.Dequeue();
                    var successors = current.Operation == null ? ProcessBranching(current) : ProcessOperation(current);
                    foreach (var node in successors)
                    {
                        queue.Enqueue(node);
                    }
                }
            }
        }

        private IEnumerable<ExplodedNode> ProcessBranching(ExplodedNode node)
        {
            // ToDo: Something is still missing around here - process branching
            yield break;
        }

        private IEnumerable<ExplodedNode> ProcessOperation(ExplodedNode node)
        {
            var state = node.State;
            state = InvokeChecks(state, (check, ps) => check.PreProcess(ps, node.Operation));
            if (state == null)
            {
                yield break;
            }

            // ToDo: Something is still missing around here - process well known instructions
            state = state.SetOperationValue(node.Operation, CreateSymbolicValue());

            state = InvokeChecks(state, (check, ps) => check.PostProcess(ps, node.Operation));
            if (state == null)
            {
                yield break;
            }

            yield return new ExplodedNode(node, state);
        }

        private ProgramState InvokeChecks(ProgramState state, Func<SymbolicExecutionCheck, ProgramState, ProgramState> invoke)
        {
            foreach (var check in checks)
            {
                state = invoke(check, state);
                if (state == null)
                {
                    return null;
                }
            }
            return state;
        }

        private SymbolicValue CreateSymbolicValue() =>
            new SymbolicValue(symbolicValueCounter);
    }
}