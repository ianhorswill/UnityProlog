#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstantiationException.cs" company="Ian Horswill">
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
    /// Thrown when a variable that should/shouldn't be bound isn't/is bound
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable"), SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class InstantiationException : Exception
    {
        /// <summary>
        /// Indicates a variable that should/shouldn't be bound isn't/is bound
        /// </summary>
        public InstantiationException(LogicVariable offendingVariable, string message)
            : base(message)
        {
            Variable = offendingVariable;
        }

        /// <summary>
        /// The variable that should/shouldn't have been bound.
        /// </summary>
        public LogicVariable Variable { get; private set; }
    }

    /// <summary>
    /// Thrown when a variable that should be bound isn't
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable"), SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class UninstantiatedVariableException : InstantiationException
    {
        /// <summary>
        /// Indicates a variable that should be bound isn't
        /// </summary>
        public UninstantiatedVariableException(LogicVariable offendingVariable, string message)
            : base(offendingVariable, message)
        { }
    }

    /// <summary>
    /// Thrown when a variable that shouldn't be bound is bound
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable"), SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class InstantiatedVariableException : InstantiationException
    {
        /// <summary>
        /// Indicates a variable that shouldn't be bound is bound
        /// </summary>
        public InstantiatedVariableException(LogicVariable offendingVariable, string message)
            : base(offendingVariable, message)
        { }
    }
}
