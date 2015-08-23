﻿#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BadProcedureException.cs" company="Ian Horswill">
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
    class BadProcedureException : Exception
    {
        public object Procedure { get; private set; }

        public BadProcedureException(object proc)
        {
            Procedure = proc;
        }

        public BadProcedureException(Symbol functor, int arity)
        {
            Procedure = new Structure(Symbol.Intern("/"), functor, arity);
        }

        public override string Message
        {
            get
            {
                var s = Procedure as Structure;
                if (s != null)
                    return String.Format("Unknown arithmetic function: {0}/{1}", s.Arguments[0], s.Arguments[1]);
                return String.Format("Bad function or procedure: {0}", this.Procedure);
            }
        }

    }
}
