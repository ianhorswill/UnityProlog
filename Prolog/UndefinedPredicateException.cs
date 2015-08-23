#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UndefinedPredicateException.cs" company="Ian Horswill">
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
    /// <summary>
    /// Thrown to signal that a predicate used as a goal does not appear in the database.
    /// </summary>
    public class UndefinedPredicateException : Exception
    {
        /// <summary>
        /// The offending predicate in question
        /// </summary>
        public PredicateIndicator Predicate { get; private set; }
        /// <summary>
        /// Thrown to signal that a predicate used as a goal does not appear in the database.
        /// </summary>
        public UndefinedPredicateException(PredicateIndicator p)
        {
            Predicate = p;
        }

        /// <summary>
        /// Thrown to signal that a predicate used as a goal does not appear in the database.
        /// </summary>
        public UndefinedPredicateException(Symbol functor, int arity) 
            : this(new PredicateIndicator(functor, arity))
        { }

        /// <summary>
        /// Human-readable message describing the exception
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Undefined predicate: {0}", Predicate);
            }
        }
    }
}
