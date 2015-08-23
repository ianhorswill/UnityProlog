﻿#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Metastructure.cs" company="Ian Horswill">
// Copyright (C) 2015 Ian Horswill
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
//  
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Prolog
{
    abstract class Metastructure
    {
        #region Iterator-based unification
        public abstract Metastructure MetaMetaUnify(Metastructure value, out IEnumerable<CutState> filter);
        public abstract Metastructure MetaVarUnify(LogicVariable them, out IEnumerable<CutState> filter);
        public abstract IEnumerable<CutState> MetaTermUnify(object value);
        public abstract void MetaTermUnify(object value, PrologContext context);
        #endregion

        #region Trail-based unification
        internal abstract Metastructure MetaMetaUnify(Metastructure theirMetaStructure, PrologContext context);
        internal abstract Metastructure MetaVarUnify(LogicVariable l, PrologContext context);
        #endregion
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    sealed class Suspension : Metastructure
    {
        public Suspension(Structure delayedGoal, Structure frozenGoal, PrologContext prologContext)
        {
            DelayedGoal = delayedGoal;
            FrozenGoal = frozenGoal;
            context = prologContext;
        }

        /// <summary>
        /// Frozen goal - only runs when variable is bound to a value, but not when aliased to another variable.
        /// </summary>
        public Structure FrozenGoal { get; private set; }
        /// <summary>
        /// Delayed goal - runs when variable bound to anything (including another variable)
        /// </summary>
        public Structure DelayedGoal { get; private set; }
        private readonly PrologContext context;

        public override Metastructure MetaMetaUnify(Metastructure value, out IEnumerable<CutState> filter)
        {
            var s = value as Suspension;
            if (s == null) throw new ArgumentTypeException("MetaMetaUnify", "value", value, typeof(Suspension));
            if (context != s.context) throw new ArgumentException("Can't unify suspended goals across PrologContexts.");
            
            filter = Prover(CombineGoals(DelayedGoal, s.DelayedGoal));
            return MakeSuspension(null, CombineGoals(FrozenGoal, s.FrozenGoal));
        }

        public override Metastructure MetaVarUnify(LogicVariable them, out IEnumerable<CutState> filter)
        {
            filter = Prover(DelayedGoal);
            return MakeSuspension(null, FrozenGoal);
        }

        internal override Metastructure MetaMetaUnify(Metastructure theirMetaStructure, PrologContext context)
        {
            var s = theirMetaStructure as Suspension;
            if (s == null) throw new ArgumentTypeException("MetaMetaUnify", "theirMetaStructure", theirMetaStructure, typeof(Suspension));
            if (context != s.context) throw new ArgumentException("Can't unify suspended goals across PrologContexts.");

            context.WakeUpGoal(CombineGoals(DelayedGoal, s.DelayedGoal));
            return MakeSuspension(null, CombineGoals(FrozenGoal, s.FrozenGoal));
        }

        internal override Metastructure MetaVarUnify(LogicVariable l, PrologContext context)
        {
            if (DelayedGoal != null)
                context.WakeUpGoal(DelayedGoal);
            return MakeSuspension(null, FrozenGoal);
        }

        IEnumerable<CutState> Prover(Structure goal)
        {
            if (goal == null)
                return CutStateSequencer.Succeed();
            return context.Prove(goal);
        }

        Suspension MakeSuspension(Structure delayed, Structure frozen)
        {
            if (delayed == null && frozen == null)
                return null;
            return new Suspension(delayed, frozen, context);
        }

        static Structure CombineGoals(Structure goal1, Structure goal2)
        {
            if (goal1 == null)
                return goal2;
            if (goal2 == null)
                return goal1;
            return new Structure(Symbol.Comma, goal1, goal2);
        }

        public override IEnumerable<CutState> MetaTermUnify(object value)
        {
            return Prover(CombineGoals(DelayedGoal, FrozenGoal));
        }

        public override void MetaTermUnify(object value, PrologContext contextOfBinding)
        {
            Debug.Assert(contextOfBinding==context, "Delayed goal woken in a different context than it was created in.");
            contextOfBinding.WakeUpGoal(CombineGoals(DelayedGoal, FrozenGoal));
        }


        internal string DebuggerDisplay
        {
            get
            {
                return string.Format(
                    "Suspension(delayed={0}, frozen={1})",
                    Term.ToStringInPrologFormat(DelayedGoal),
                    Term.ToStringInPrologFormat(FrozenGoal));
            }
        }
    }
}
