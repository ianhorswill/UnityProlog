#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceFileTracker.cs" company="Ian Horswill">
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
using System.IO;

namespace Prolog
{
    class SourceFileTracker
    {
        readonly Dictionary<string, DateTime> files = new Dictionary<string, DateTime>();

        public void NoteFile(string path)
        {
            files[path] = File.GetLastWriteTime(path);
        }

        public List<string> OutOfDateFiles
        {
            get
            {
                // Collections can't be iterated over while we're mutating them so we have to precompute the list of out of date files, then return them.
                var modifiedFiles = new List<string>();
                foreach (var pair in files)
                    if (File.GetLastWriteTime(pair.Key) > pair.Value)
                        modifiedFiles.Add(pair.Key);
                return modifiedFiles;
            }
        }

        public bool Contains(string path)
        {
            return files.ContainsKey(path);
        }
    }
}
