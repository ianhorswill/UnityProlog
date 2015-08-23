#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PredicateIndicator.cs" company="Ian Horswill">
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

namespace Prolog
{
    public struct PredicateIndicator
    {
        public readonly Symbol Functor;
        public readonly int Arity;

        public PredicateIndicator(Symbol functor, int arity)
        {
            this.Functor = functor;
            this.Arity = arity;
        }

        public static PredicateIndicator FromExpression(object expression)
        {
            var s = Term.Deref(expression) as Structure;
            if (s == null
                || (!s.IsFunctor(Symbol.Slash, 2) && !s.IsFunctor(Symbol.SlashSlash, 2))
                || !(s.Argument(0) is Symbol)
                || !(s.Argument(1) is int))
                throw new ArgumentException("Predicate indicator should be of the form functor/arity, but got "+ISOPrologWriter.WriteToString(expression));
            return new PredicateIndicator((Symbol)s.Argument(0), (int)s.Argument(1));
        }

        public PredicateIndicator(Structure s) : this(s.Functor, s.Arity) { }

        public static bool operator==(PredicateIndicator a, PredicateIndicator b)
        {
            return a.Functor == b.Functor && a.Arity == b.Arity;
        }

        public static bool operator !=(PredicateIndicator a, PredicateIndicator b)
        {
            return a.Functor != b.Functor || a.Arity != b.Arity;
        }

        public override int GetHashCode()
        {
            return Functor.GetHashCode() ^ Arity;
        }

        public override bool Equals(object obj)
        {
            if (obj is PredicateIndicator)
            {
                var o = (PredicateIndicator)obj;
                return this.Functor == o.Functor && this.Arity == o.Arity;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Functor.Name, Arity);
        }
    }
}
