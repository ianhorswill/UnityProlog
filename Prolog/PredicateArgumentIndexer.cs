﻿#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PredicateArgumentIndexer.cs" company="Ian Horswill">
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
    public struct PredicateArgumentIndexer
    {
        public PredicateArgumentIndexer(IndexerType type, object functor, byte arity)
        {
            this.Type = type;
            this.Functor = functor;
            this.Arity = arity;
        }

        public PredicateArgumentIndexer(object argument)
        {
            argument = Term.Deref(argument);
            if (argument == null)
            {
                this.Type = IndexerType.Null;
                this.Functor = null;
                this.Arity = 0;
            }
            else
            {
                if (argument is LogicVariable || argument is Indexical)
                {
                    this.Type = IndexerType.Variable;
                    this.Functor = null;
                    this.Arity = 0;
                }
                else
                {
                    var s = argument as Structure;
                    if (s != null)
                    {
                        this.Type = IndexerType.Structure;
                        this.Functor = s.Functor;
                        this.Arity = (byte)s.Arity;
                    }
                    else
                    {
                        this.Type = IndexerType.Atom;
                        this.Functor = argument;
                        this.Arity = 0;
                    }
                }
            }
        }

        public static PredicateArgumentIndexer[] ArglistIndexers(object[] args)
        {
            var result = new PredicateArgumentIndexer[args.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = new PredicateArgumentIndexer(args[i]);
            return result;
        }

        public static bool PotentiallyMatchable(PredicateArgumentIndexer a, PredicateArgumentIndexer b)
        {
            if (b.Type == IndexerType.Variable)
                return true;
            switch (a.Type)
            {
                case IndexerType.Variable:
                    return true;

                case IndexerType.Null:
                    return b.Type == IndexerType.Null;

                case IndexerType.Atom:
                    return b.Type == IndexerType.Atom && a.Functor.Equals(b.Functor);

                case IndexerType.Structure:
                    return b.Type == IndexerType.Structure && a.Functor == b.Functor && a.Arity == b.Arity;

                default:
                    throw new NotImplementedException("Invalid PredicateArgumentIndexerType");
            }
        }

        public static bool PotentiallyMatchable(object a, PredicateArgumentIndexer b)
        {
            return PotentiallyMatchable(new PredicateArgumentIndexer(a), b);
        }

        public static bool PotentiallyMatchable(PredicateArgumentIndexer[] a, PredicateArgumentIndexer[] b)
        {
            for (var i=0; i<a.Length; i++)
                if (!PotentiallyMatchable(a[i], b[i]))
                    return false;
            return true;
        }

        public enum IndexerType : byte
        {
            Variable,
            Null,
            Atom,
            Structure
        }

        public readonly IndexerType Type;
        public readonly byte Arity;
        public readonly object Functor;

        public static bool operator ==(PredicateArgumentIndexer a, PredicateArgumentIndexer b)
        {
            switch (a.Type)
            {
                case IndexerType.Variable:
                    case IndexerType.Null:
                    return a.Type == b.Type;

                    case IndexerType.Atom:
                    return b.Type == IndexerType.Atom && a.Functor.Equals(b.Functor);

                    case IndexerType.Structure:
                    return b.Type == IndexerType.Structure && a.Functor == b.Functor && a.Arity == b.Arity;

                default:
                    throw new NotImplementedException("Invalid PredicateArgumentIndexer type");
            }
        }

        public static bool operator !=(PredicateArgumentIndexer a, PredicateArgumentIndexer b)
        {
            return !(a==b);
        }

        public override int GetHashCode()
        {
            return (int)Type ^ Functor.GetHashCode() ^ Arity;
        }

        public override bool Equals(object obj)
        {
            if (obj is PredicateArgumentIndexer)
            {
                return this == (PredicateArgumentIndexer)obj;
            }
            return false;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case IndexerType.Structure:
                    return string.Format("{0}/{1}", ((Symbol)Functor).Name, Arity);

                case IndexerType.Atom:
                    return ISOPrologWriter.WriteToString(Functor);

                case IndexerType.Variable:
                    return "Var";

                case IndexerType.Null:
                    return "NullIndexer";
            }
            return "<PredicateArgumentIndexer with invalid type>";
        }
    }
}
