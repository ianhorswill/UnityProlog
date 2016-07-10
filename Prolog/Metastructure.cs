#region Copyright
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
    /// <summary>
    /// Used to extend unification mechanism for constraint processing.
    /// Currently, only subclass is Suspension, which is used by dif/2 and freeze/2.
    /// </summary>
    abstract class Metastructure
    {
        #region Iterator-based unification
        /// <summary>
        /// Merge the information from two Metastructures into one new metastructure.
        /// </summary>
        /// <param name="value">The Metastructure to merge with.</param>
        /// <param name="filter">Test for whether the merging succeeded.  Generally a woken goal.</param>
        /// <returns>The merged Metastructure.</returns>
        public abstract Metastructure MetaMetaUnify(Metastructure value, out IEnumerable<CutState> filter);

        /// <summary>
        /// Called after the variable bound to this Metastructure is unified with an unbound variable
        /// that is not (itself) bound to a Metastructure.
        /// </summary>
        /// <param name="them">The logic variable with which to unify</param>
        /// <param name="filter">Test for whether the merging succeeded.  Generally a woken goal.</param>
        /// <returns>The Metastructure to bind to the newly aliased variables.</returns>
        public abstract Metastructure MetaVarUnify(LogicVariable them, out IEnumerable<CutState> filter);

        /// <summary>
        /// Called after the variable bound to this Metastructure is unified with a non-variable term.
        /// </summary>
        /// <param name="value">The term to which to unify.</param>
        /// <returns>Iterator for any suspended goals.</returns>
        public abstract IEnumerable<CutState> MetaTermUnify(object value);
        #endregion

        #region Trail-based unification
        /// <summary>
        /// Merge the information from two Metastructures into one new metastructure.
        /// </summary>
        /// <param name="theirMetaStructure">The Metastructure to merge with.</param>
        /// <param name="context">Context in which to execute suspended goals.</param>
        /// <returns>The merged Metastructure.</returns>
        public abstract Metastructure MetaMetaUnify(Metastructure theirMetaStructure, PrologContext context);

        /// <summary>
        /// Called after the variable bound to this Metastructure is unified with an unbound variable
        /// that is not (itself) bound to a Metastructure.
        /// </summary>
        /// <param name="l">The logic variable with which to Unify.</param>
        /// <param name="context">Context in which to execute suspended goals.</param>
        /// <returns>The Metastructure to bind to the newly aliased variables.</returns>
        public abstract Metastructure MetaVarUnify(LogicVariable l, PrologContext context);

        /// <summary>
        /// Called after the variable bound to this Metastructure is unified with a non-variable term.
        /// </summary>
        /// <param name="value">The term to which to unify</param>
        /// <param name="context">Context in which to execute suspended goals.</param>
        public abstract void MetaTermUnify(object value, PrologContext context);
        #endregion
    }

    /// <summary>
    /// Used to hold suspended goals for dif/2 and freeze/2.
    /// Suspended goals are run when the logic variable to which this Suspension is bound is unified.
    /// Delayed goals run on any unification, frozen only on unification with non-variable terms.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    sealed class Suspension : Metastructure
    {
        /// <summary>
        /// Create a new set of suspended goals.
        /// </summary>
        /// <param name="delayedGoal">Goal to run upon unification with any value.</param>
        /// <param name="frozenGoal">Goal to run upon unification with a non-variable term.</param>
        /// <param name="prologContext">Context in which to run goals.</param>
        public Suspension(Structure delayedGoal, Structure frozenGoal, PrologContext prologContext)
        {
            DelayedGoal = delayedGoal;
            FrozenGoal = frozenGoal;
            Context = prologContext;
        }

        /// <summary>
        /// Frozen goal - only runs when variable is bound to a value, but not when aliased to another
        /// variable.  Used in implementation of freeze/2.
        /// </summary>
        public Structure FrozenGoal { get; }
        /// <summary>
        /// Delayed goal - runs when variable bound to anything (including another variable).
        /// Used in implementation of dif/2.
        /// </summary>
        public Structure DelayedGoal { get; }

        #region Iterator-based unification
        /// <summary>
        /// Prolog context in which the goals were suspended.  Logic variables (and hence meta structures)
        /// should never be shared across contexts without term copying.
        /// </summary>
        public readonly PrologContext Context;

        /// <summary>
        /// Merge the information from two Metastructures into one new metastructure.
        /// </summary>
        /// <param name="value">The Metastructure to merge with.</param>
        /// <param name="filter">Test for whether the merging succeeded.  Generally a woken goal.</param>
        /// <returns>The merged Metastructure.</returns>
        public override Metastructure MetaMetaUnify(Metastructure value, out IEnumerable<CutState> filter)
        {
            var s = value as Suspension;
            if (s == null) throw new ArgumentTypeException("MetaMetaUnify", "value", value, typeof(Suspension));
            if (Context != s.Context) throw new ArgumentException("Can't unify suspended goals across PrologContexts.");
            
            filter = Prover(CombineGoals(DelayedGoal, s.DelayedGoal));
            return MakeSuspension(null, CombineGoals(FrozenGoal, s.FrozenGoal));
        }

        /// <summary>
        /// Called after the variable bound to this Metastructure is unified with an unbound variable
        /// that is not (itself) bound to a Metastructure.
        /// </summary>
        /// <param name="them">The logic variable with which to unify</param>
        /// <param name="filter">Test for whether the merging succeeded.  Generally a woken goal.</param>
        /// <returns>The Metastructure to bind to the newly aliased variables.</returns>
        public override Metastructure MetaVarUnify(LogicVariable them, out IEnumerable<CutState> filter)
        {
            filter = Prover(DelayedGoal);
            return MakeSuspension(null, FrozenGoal);
        }

        /// <summary>
        /// Called after the variable bound to this Metastructure is unified with a non-variable term.
        /// </summary>
        /// <param name="value">The term to which to unify.</param>
        /// <returns>Iterator for any suspended goals.</returns>
        public override IEnumerable<CutState> MetaTermUnify(object value)
        {
            return Prover(CombineGoals(DelayedGoal, FrozenGoal));
        }
        #endregion

        #region Trail-based unification
        /// <summary>
        /// Merge the information from two Metastructures into one new metastructure.
        /// </summary>
        /// <param name="theirMetaStructure">The Metastructure to merge with.</param>
        /// <param name="context">Context in which to execute suspended goals.</param>
        /// <returns>The merged Metastructure.</returns>
        public override Metastructure MetaMetaUnify(Metastructure theirMetaStructure, PrologContext context)
        {
            var s = theirMetaStructure as Suspension;
            if (s == null) throw new ArgumentTypeException("MetaMetaUnify", "theirMetaStructure", theirMetaStructure, typeof(Suspension));
            if (context != s.Context) throw new ArgumentException("Can't unify suspended goals across PrologContexts.");

            context.WakeUpGoal(CombineGoals(DelayedGoal, s.DelayedGoal));
            return MakeSuspension(null, CombineGoals(FrozenGoal, s.FrozenGoal));
        }

        /// <summary>
        /// Called after the variable bound to this Metastructure is unified with an unbound variable
        /// that is not (itself) bound to a Metastructure.
        /// </summary>
        /// <param name="l">The logic variable with which to Unify.</param>
        /// <param name="context">Context in which to execute suspended goals.</param>
        /// <returns>The Metastructure to bind to the newly aliased variables.</returns>
        public override Metastructure MetaVarUnify(LogicVariable l, PrologContext context)
        {
            if (DelayedGoal != null)
                context.WakeUpGoal(DelayedGoal);
            return MakeSuspension(null, FrozenGoal);
        }

        /// <summary>
        /// Called after the variable bound to this Metastructure is unified with a non-variable term.
        /// </summary>
        /// <param name="value">The term to which to unify</param>
        /// <param name="contextOfBinding">Context in which to execute suspended goals.</param>
        public override void MetaTermUnify(object value, PrologContext contextOfBinding)
        {
            Debug.Assert(contextOfBinding == Context, "Delayed goal woken in a different context than it was created in.");
            contextOfBinding.WakeUpGoal(CombineGoals(DelayedGoal, FrozenGoal));
        }
        #endregion

        #region Utilities
        IEnumerable<CutState> Prover(Structure goal)
        {
            if (goal == null)
                return CutStateSequencer.Succeed();
            return Context.Prove(goal);
        }

        Suspension MakeSuspension(Structure delayed, Structure frozen)
        {
            if (delayed == null && frozen == null)
                return null;
            return new Suspension(delayed, frozen, Context);
        }

        static Structure CombineGoals(Structure goal1, Structure goal2)
        {
            if (goal1 == null)
                return goal2;
            if (goal2 == null)
                return goal1;
            return new Structure(Symbol.Comma, goal1, goal2);
        }

        internal string DebuggerDisplay =>
            $"Suspension(delayed={Term.ToStringInPrologFormat(DelayedGoal)}, frozen={Term.ToStringInPrologFormat(FrozenGoal)})"
            ;

        #endregion
    }
}
