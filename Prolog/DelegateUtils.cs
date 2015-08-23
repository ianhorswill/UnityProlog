#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateUtils.cs" company="Ian Horswill">
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
    /// Extension methods and other utilities for working with delegates
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils")]
    public static class DelegateUtils
    {
        /// <summary>
        /// Assigns d the printed name s
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s"), SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "d")]
        public static void NameProcedure(Delegate d, string s)
        {
            NamedProcedureTable[d] = s;
        }

        /// <summary>
        /// Name of procedure for debugging purposes
        /// </summary>
        /// <param name="procedure"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily"), SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "d")]
        public static string DelegateName(Delegate procedure)
        {
            if (NamedProcedureTable.ContainsKey(procedure))
                return NamedProcedureTable[procedure];
            return procedure.Method.Name;
        }

        /// <summary>
        /// The arguments of the specified procedure.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "d"), SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Arglist")]
        public static IList<object> Arglist(this Delegate d)
        {
            return Arglists[d];
        }

        /// <summary>
        /// The documentation for the specified procedure.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "d")]
        public static string Documentation(this Delegate d)
        {
            return Docstrings[d];
        }

        internal static Dictionary<Delegate, IList<object>> Arglists = new Dictionary<Delegate, IList<object>>();
        internal static Dictionary<Delegate, string> Docstrings = new Dictionary<Delegate, string>();
        internal static Dictionary<Delegate, string> NamedProcedureTable = new Dictionary<Delegate, string>();
    }
}
