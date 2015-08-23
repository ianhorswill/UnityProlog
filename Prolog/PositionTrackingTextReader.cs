#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PositionTrackingTextReader.cs" company="Ian Horswill">
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
using System.IO;
using System.Text;

namespace Prolog
{
    class PositionTrackingTextReader : TextReader
    {
        public PositionTrackingTextReader(TextReader reader, string filename)
        {
            subreader = reader;
            File = filename;
            Line = 1;
            currentLine = new StringBuilder();
        }

        private readonly TextReader subreader;
        readonly StringBuilder currentLine;
        private string errorLine;

        public int Line { get; private set; }
        public int Column { get; private set; }
        public string File { get; private set; }

        /// <summary>
        /// The current line at the time a syntax error was found;
        /// </summary>
        public string ErrorLine
        {
            get
            {
                if (errorLine == null)
                {
                    // Finish off the current line
                    currentLine.Append(subreader.ReadLine());
                    errorLine = currentLine.ToString();
                }
                return errorLine;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ErrorLine")]
        public override int Read()
        {
            if (errorLine != null)
                throw new InvalidOperationException("Attempt to read after ErrorLine invoked.");
            int c = subreader.Read();
            if (c=='\n')
            {
                Line += 1;
                Column = 0;
                currentLine.Length = 0;
            }
            else if (c>=0)
            {
                currentLine.Append((char) c);
                Column += 1;
            }

            return c;
        }

        public override int Peek()
        {
            return subreader.Peek();
        }
    }
}
