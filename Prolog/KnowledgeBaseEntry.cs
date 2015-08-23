#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeBaseEntry.cs" company="Ian Horswill">
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Prolog
{
    /// <summary>
    /// Base class of all entries within a given predicate in the knowledge base.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "KnowledgeBase")]
    public abstract class KnowledgeBaseEntry
    {
        /// <summary>
        /// Attempts to prove the given goal using the KB entry.
        /// </summary>
        internal abstract IEnumerable<CutState> Prove(object[] args, PrologContext context, ushort parentFrame);

        /// <summary>
        /// Head of this assertion.
        /// </summary>
        public abstract object Head { get; }

        /// <summary>
        /// Returns the head's functor, or the head itself if it's a symbol.
        /// </summary>
        public Symbol HeadFunctor
        {
            get
            {
                object h = Head;
                var structure = h as Structure;
                if (structure != null)
                    return structure.Functor;
                var symbol = h as Symbol;
                if (symbol != null)
                    return symbol;
                Debug.Assert(false, "Head of rule is not a symbol or structure." );
                return null;
            }
        }

        /// <summary>
        /// Returns the head's arity, or 0 if the head is a symbol.
        /// </summary>
        public int HeadArity
        {
            get
            {
                object h = Head;
                var structure = h as Structure;
                if (structure != null)
                    return structure.Arity;
                if (h is Symbol)
                    return 0;
                Debug.Assert(false, "Head of rule is not a symbol or structure.");
                return 0;
            }
        }

        /// <summary>
        /// Body of this assertion (true/0, if it's a fact).
        /// </summary>
        public abstract object Body { get; }

        /// <summary>
        /// Source file from which this entry (e.g. rule) was loaded, or null if it wasn't consulted from a file (e.g. it was created using assert or reading from a string).
        /// </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// SourceFile without Application.dataPath at the beginning
        /// </summary>
        public string SourceFileTrimmed
        {
            get
            {
                return Prolog.TrimPath(SourceFile);
            }
        }

        /// <summary>
        /// Location in source file from which this entry (e.g. rule) was loaded, or 0 if it wasn't consulted from a file (e.g. it was created using assert or reading from a string).
        /// </summary>
        public int SourceLineNumber { get; set; }

        public virtual bool Prematch(PredicateArgumentIndexer[] argIndexers)
        {
            return true;
        }
    }
}
