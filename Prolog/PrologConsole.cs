#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrologConsole.cs" company="Ian Horswill">
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

using Northwestern.UnityUtils;

using UnityEngine;

namespace Prolog
{
    class PrologConsole : Console
    {
#pragma warning disable 649
        public GameObject DefaultGameObject;
#pragma warning restore 649

        private Repl repl;

        internal override void Start()
        {
            base.Start();
            repl = new Repl {
                       Output = Out,
                       CurrentGameObject = DefaultGameObject??gameObject,
                       OnChangeKB = KBChanged
                   };
            WindowTitle = "Prolog console: " + repl.CurrentKnowledgeBase.Name;
            Prolog.TraceOutput = Out;
            PrologChecker.Check();
        }

        void KBChanged(KnowledgeBase newKB)
        {
            var eli = FindObjectOfType<ELInspector>();
            if (eli != null)
                eli.SetKB(newKB);
            WindowTitle = "Prolog console: " + newKB.Name;
        }

        protected override void Run(string command)
        {

            if (command != ";")
                Out.Write("?- ");
            Out.WriteLine(command);
            repl.ProcessCommandLine(command);
        }

        protected override bool OmitCommandFromHistory(string command)
        {
            // Ignore requests for subsequent solutions
            return command.Trim() == ";";
        }
    }
}
