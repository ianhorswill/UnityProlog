#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeBaseVariable.cs" company="Ian Horswill">
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
using System.Diagnostics.CodeAnalysis;

namespace Prolog
{
    /// <summary>
    /// A KBE whose only purpose is to act like a global variable, e.g. for state machines, discourse state, etc.
    /// </summary>
    sealed class KnowledgeBaseVariable : KnowledgeBaseEntry
    {
        /// <summary>
        /// Current value of the "variable".
        /// </summary>
        public object CurrentValue { get; set; }

        KnowledgeBaseVariable(object initialValue)
        {
            CurrentValue = initialValue;
        }

        public override object Head
        {
            get { throw new NotImplementedException(); }
        }

        public override object Body
        {
            get { throw new NotImplementedException(); }
        }

        internal override IEnumerable<CutState> Prove(object[] args, PrologContext context, ushort parentFrame)
        {
            if (args.Length != 1) throw new ArgumentCountException("variable", args, new object[] { "Value" });
            return Term.UnifyAndReturnCutState(CurrentValue, args[0]);
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        internal static IEnumerable<CutState> SetImplementation(object[] args, PrologContext context)
        {
            if (args.Length != 2) throw new ArgumentCountException("set", args, new object[] { "Variable", "NewValue"});

            object value = Term.CopyInstantiation(args[1]);
            if (value is LogicVariable) throw new UninstantiatedVariableException((LogicVariable)args[1], "Value argument should be a data object, not an uninstantiated (unbound) variable.");
            var functor = Term.Deref(args[0]) as Symbol;
            if (functor == null) throw new ArgumentTypeException("set", "functor", args[0], typeof (Symbol));

            List<KnowledgeBaseEntry> entries = context.KnowledgeBase.EntryListForStoring(new PredicateIndicator(functor, 1));

            switch (entries.Count)
            {
                case 0:
                    entries.Add(new KnowledgeBaseVariable(value));
                    return CutStateSequencer.Succeed();

                case 1:
                    var v = entries[0] as KnowledgeBaseVariable;
                    if (v==null)
                        throw new ArgumentException("Functor is not a variable; it has another entry defined for it.");
                    v.CurrentValue = value;
                    return CutStateSequencer.Succeed();

                default:
                    throw new ArgumentException("Functor is not a variable; it has multiple entries defined for it.");
            }
        }
    }
}
