#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentTypeException.cs" company="Ian Horswill">
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

namespace Prolog
{
    /// <summary>
    /// Indicates a procedure or predicate was called with the wrong number of arguments.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable"), SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class ArgumentTypeException : ArgumentException
    {
        /// <summary>
        /// Indicates a variable that should be bound isn't
        /// </summary>
        public ArgumentTypeException(string procName, string argumentName, object value, Type expectedType)
        {
            procedureName = procName;
            ArgumentName = argumentName;
            Value = value;
            ExpectedType = expectedType;
        }

        private readonly string procedureName;
        /// <summary>
        /// Name of the argument that was passed an incorrect value.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public readonly string ArgumentName;
        /// <summary>
        /// Value passed to the argument
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public readonly object Value;
        /// <summary>
        /// Type of value that should have been passed
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public readonly Type ExpectedType;

        /// <summary>
        /// Message describing the problem.
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
        public override string Message
        {
            get
            {
                return string.Format("The {0} argument to {1} should have been of type {2} but was passed {3}.", ArgumentName, procedureName,
                                     ExpectedType.Name,
                                     Term.ToStringInPrologFormat(Value));
            }
        }
    }
}
