#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentCountException.cs" company="Ian Horswill">
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
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Prolog
{
    /// <summary>
    /// Indicates a procedure or predicate was called with the wrong number of arguments.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable"), SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class ArgumentCountException : ArgumentException
    {
        /// <summary>
        /// Indicates a variable that should be bound isn't
        /// </summary>
        public ArgumentCountException(string procName, object[] actualArguments, params object[] expectedArguments)
        {
            procedureName = procName;
            ActualArguments = actualArguments;
            ExpectedArguments = expectedArguments;
        }

        private readonly string procedureName;

        /// <summary>
        /// Arguments provided in the call
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public readonly object[] ActualArguments;
        /// <summary>
        /// Arguments the procedure or predicate expected.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public readonly object[] ExpectedArguments;

        /// <summary>
        /// Message describing the problem.
        /// </summary>
        public override string Message
        {
            get
            {
                var b = new StringBuilder();
                b.AppendFormat("Wrong number of arguments to {0} '{1}'.\nReceived: ", "predicate", procedureName);
                bool firstOne = true;
                foreach (var e in ActualArguments)
                {
                    if (firstOne)
                        firstOne = false;
                    else
                        b.Append(", ");

                    b.Append(Term.ToStringInPrologFormat(e));

                }
                b.Append("\nExpected: ");
                firstOne = true;
                foreach (var e in ExpectedArguments)
                {
                    if (firstOne)
                        firstOne = false;
                    else
                        b.Append(", ");

                    b.Append(e);
                }
                return b.ToString();
            }
        }
    }
}
