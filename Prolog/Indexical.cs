#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Indexical.cs" company="Ian Horswill">
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

using UnityEngine;

namespace Prolog
{
    /// <summary>
    /// Represents a time-varying value such as this or now.
    /// </summary>
    public class Indexical : AlphaConvertibleTerm
    {
        #region Standard Indexicals
        static Indexical()
        {
            DeclareIndexical("this",
                context =>
                {
                    if (context.This == null)
                        throw new Exception("Indexical $this has not been given a value");
                    return context.This;
                });
            DeclareIndexical("me",
                context =>
                {
                    if (context.KnowledgeBase.GameObject == null)
                        throw new Exception("Current KnowledgeBase has no associated game object");
                    return context.GameObject;
                });
            DeclareIndexical("parent",
                context =>
                {
                    if (context.KnowledgeBase.Parent == null)
                        throw new Exception("Current KnowledgeBase has no parent.");
                    return context.KnowledgeBase.Parent;
                });
            DeclareIndexical("global", context => KnowledgeBase.Global);
            DeclareIndexical("root", context => context.KnowledgeBase.ELRoot);
            DeclareIndexical("global_root", context => KnowledgeBase.Global.ELRoot);
            DeclareIndexical("now", context => Time.time);
        }

        static readonly Dictionary<Symbol,Indexical> IndexicalTable = new Dictionary<Symbol, Indexical>();

        public static Indexical Find(Symbol name)
        {
            Indexical result;
            return IndexicalTable.TryGetValue(name, out result) ? result : null;
        }

        public static void DeclareIndexical(string name, Func<PrologContext, object> valueFunc)
        {
            DeclareIndexical(Symbol.Intern(name), valueFunc);
        }

        public static void DeclareIndexical(Symbol name, Func<PrologContext, object> valueFunc)
        {
            IndexicalTable[name] = new Indexical(name, valueFunc);
        }
        #endregion

        public readonly Symbol Name;
        readonly Func<PrologContext, object> valueFunc;

        private Indexical(Symbol name, Func<PrologContext, object> valueFunc)
        {
            this.Name = name;
            this.valueFunc = valueFunc;
        }

        public object GetValue(PrologContext context)
        {
            return this.valueFunc(context);
        }

        /// <summary>
        /// Replace self with value if evalIndexicals is true, otherwise return self unchanged.
        /// </summary>
        public override object AlphaConvert(List<LogicVariable> oldVars, LogicVariable[] newVars, PrologContext context, bool evalIndexicals)
        {
            // Temporarily ignoring evalIndexicals to try out programming here rule bodies expand indexicals, not just rule heads.
            //return evalIndexicals ? this.GetValue(context) : this;
            return this.GetValue(context);
        }

        public override string ToString()
        {
            return "$"+Name;
        }

        #region Unification
        internal override IEnumerable<bool> UnifyWithTerm(Term value)
        {
            return value.UnifyWithAtomicConstant(this);
        }
        internal override bool UnifyWithTerm(Term value, PrologContext context)
        {
            return value.UnifyWithAtomicConstant(this, context);
        }
        #endregion

        internal static void DeclareUserBindableIndexical(object declaration)
        {
            var s = declaration as Structure;
            if (s==null || !s.IsFunctor(Symbol.EqualSign, 2))
                throw new ArgumentException("Indexical declaration should be of the form Name=DefaultValue.");
            var name = s.Argument(0) as Symbol;
            if (name == null)
                throw new ArgumentException("Indexical declaration should be of the form Name=DefaultValue.");
            MakeUserBindableIndexical(name, s.Argument(1));
        }

        private static void MakeUserBindableIndexical(Symbol name, object defaultValue)
        {
            if (!IsGround(defaultValue))
                throw new ArgumentException("Initial value of an indexical must be a ground term.");
            // We do CopyInstantiation out of paranoia that there might be some variables buried in defaultValue
            // that might get unbound in the future.
            DeclareIndexical(name, MakeLookup(name, CopyInstantiation(defaultValue)));
        }

        private static Func<PrologContext, object> MakeLookup(Symbol name, object defaultValue)
        {
            return context =>
            {
                var bindingStack = context.IndexicalBindingStack;
                for (int i=bindingStack.Count-1; i>=0; i--)
                    if (bindingStack[i].Key == name)
                        return bindingStack[i].Value;
                return defaultValue;
            };
        }

        public static void PushIndexicalBinding(Symbol name, object value, PrologContext context)
        {
            context.IndexicalBindingStack.Add(new KeyValuePair<Symbol, object>(name, value));
        }

        public static void PopIndexicalBinding(PrologContext context)
        {
            context.IndexicalBindingStack.RemoveAt(context.IndexicalBindingStack.Count-1);
        }
    }
}
