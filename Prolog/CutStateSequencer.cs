#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CutStateSequencer.cs" company="Ian Horswill">
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
using System.Collections;
using System.Collections.Generic;

namespace Prolog
{
    /// <summary>
    /// A placeholder enumerator that primitives can return when they just want to succeed or fail
    /// without the overhead of the state machine craeted by yield return.  These enumerators will
    /// either succeed one or fail, and are pooled so they don't allocate storage in steady state.
    ///
    /// Notes:
    /// - There is one pool shared across PrologContexts.
    /// - This could be further optimized by making a specialized fail enumerator, since there need
    ///   only be one of it.  This might possibly improve cache locality.
    /// </summary>
    internal sealed class CutStateSequencer : IEnumerable<CutState>, IEnumerator<CutState>
    {
        /// <summary>
        /// Returns a sequencer that succeeds once.
        /// </summary>
        public static CutStateSequencer Succeed()
        {
            return FromBoolean(true);
        }


        public static IEnumerable<CutState> SucceedAndRestoreTrail(PrologContext context, int trailMark)
        {
            try
            {
                yield return CutState.Continue;
            }
            finally
            {
                context.RestoreVariables(trailMark);
            }

        }

        /// <summary>
        /// Returns a sequencer that fails.
        /// </summary>
        public static CutStateSequencer Fail()
        {
            return FromBoolean(false);
        }

        public static CutStateSequencer FromBoolean(bool succeed)
        {
            var s = Pool.Allocate();
            s.succeedNextCall = succeed;
            return s;
        }

        public void Dispose()
        {
            Pool.Deallocate(this);
        }

        private CutStateSequencer() 
        { }

        private static readonly StoragePool<CutStateSequencer> Pool = new StoragePool<CutStateSequencer>(() => new CutStateSequencer()); 

        private bool succeedNextCall;

        public CutState Current
        {
            get
            {
                return CutState.Continue;
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public bool MoveNext()
        {
            var r = succeedNextCall;
            succeedNextCall = false;
            return r;
        }

        public IEnumerator<CutState> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}
