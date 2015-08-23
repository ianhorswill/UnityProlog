#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AlphaConvertibleTerm.cs" company="Ian Horswill">
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

using System.Collections.Generic;

namespace Prolog
{
    /// <summary>
    /// A Term that requires processing during alpha conversion.
    /// </summary>
    public abstract class AlphaConvertibleTerm : Term
    {
        /// <summary>
        /// Recopy the term to replace variables.  If term contains no variables, no recopying is done.
        /// </summary>
        /// <param name="oldVars">Variables to be replaced</param>
        /// <param name="newVars">The corresponding variables that are replacing the oldVars</param>
        /// <param name="context">PrologContext to evaluating indexicals</param>
        /// <param name="evalIndexicals">If true, any indexicals will be replaced with their values.</param>
        /// <returns>Converted term or original term if not conversion necessary</returns>
        public abstract object AlphaConvert(
            List<LogicVariable> oldVars,
            LogicVariable[] newVars,
            PrologContext context,
            bool evalIndexicals);
    }
}
