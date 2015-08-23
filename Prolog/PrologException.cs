#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrologException.cs" company="Ian Horswill">
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
    /// General class for prolog-specific exceptions mandated by the ISO standard.
    /// </summary>
    public class PrologException : Exception
    {
        /// <summary>
        /// The ISO-standard exception term describing the exception
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public object ISOException { get; private set; }
        /// <summary>
        /// Creates a .NET Exception object to represent an ISO-Prolog exception
        /// </summary>
        public PrologException(object isoException)
        {
            ISOException = isoException;
        }

        /// <summary>
        /// Human-readable message describing the exception
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Prolog exception: {0}", ISOException);
            }
        }
    }
}
