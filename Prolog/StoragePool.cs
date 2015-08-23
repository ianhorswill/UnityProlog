﻿#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StoragePool.cs" company="Ian Horswill">
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
using System.Collections.Generic;

namespace Prolog
{
    /// <summary>
    /// A pool of explicitly managed objects.
    /// Prevents GC of frequently created objects if the programmer can reliably
    /// know when to deallocate them.
    /// </summary>
    /// <typeparam name="T">Type of object to be managed.</typeparam>
    public sealed class StoragePool<T> 
        where T : class
    {
        /// <summary>
        /// Creates a new StoragePool given a constructor for new objects.
        /// </summary>
        /// <param name="createFunc">Constructor to use to create objects in the pool.</param>
        public StoragePool(Func<T> createFunc)
        {
            create = createFunc;
        }

        private readonly Stack<T> pool = new Stack<T>();

        private readonly Func<T> create; 

        /// <summary>
        /// Allocates an object frm the pool, or creates a new one if no existing objects are
        /// available.
        /// </summary>
        /// <returns>The allocated object</returns>
        public T Allocate()
        {
            if (pool.Count > 0)
                return pool.Pop();
            return create();
        }

        /// <summary>
        /// Returns object to the storage pool for reuse.
        /// </summary>
        /// <param name="item"></param>
        public void Deallocate(T item)
        {
            pool.Push(item);
        }
    }
}
