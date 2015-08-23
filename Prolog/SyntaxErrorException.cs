#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyntaxErrorException.cs" company="Ian Horswill">
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
using System.IO;
using System.Text;

namespace Prolog
{
    /// <summary>
    /// Signals that a Lisp expression had incorrect syntax.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")
    ,
     SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"
         )]
    public class SyntaxErrorException : Exception
    {
        /// <summary>
        /// Expression producing the error
        /// </summary>
        [SuppressMessage("Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "sexp")]
        public object SExp { get; private set; }

        /// <summary>
        /// The TextReader from which this was read.
        /// </summary>
        public object TextReader { get; private set; }

        /// <summary>
        /// Represents an instance of a syntax error
        /// </summary>
        /// <param name="exp">The probelmatic expression</param>
        /// <param name="message">Descriptive message for user</param>
        public SyntaxErrorException(object exp, string message)
            : base(message)
        {
            SExp = exp;
            TextReader = null;
        }

        /// <summary>
        /// Represents an instance of a syntax error
        /// </summary>
        public SyntaxErrorException(object exp, string message, TextReader reader)
            : base(AnnotateMessage(message, reader))
        {
            SExp = exp;
            TextReader = reader;
        }

        private static string AnnotateMessage(string mes, TextReader reader)
        {
            var tracker = reader as PositionTrackingTextReader;
            if (tracker != null)
            {
                var sb = new StringBuilder();
                sb.Append(mes);
                sb.Append(" in line:        (at ");
                sb.Append(tracker.File);
                sb.Append(":");
                sb.Append(tracker.Line);
                sb.Append(")\n");

                sb.Append(tracker.ErrorLine);
                sb.Append('\n');
                for (int i = 0; i < tracker.Column-2; i++)
                    sb.Append(' ');
                sb.Append("^");
                return sb.ToString();
            }
            return mes;
        }
    }
}
