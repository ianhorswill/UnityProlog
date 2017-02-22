#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KB.cs" company="Ian Horswill">
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

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prolog
{
    /// <summary>
    /// A wrapper for a Prolog KnowledgeBase as a Unity Component.
    /// </summary>
    public class KB : MonoBehaviour
    {
        private const string GlobalKBGameObjectName = "GlobalKB";

        public string[] SourceFiles = new string[0];

        private KnowledgeBase kb;

        private static bool globalKBInitialized;

        public KnowledgeBase KnowledgeBase
        {
            get
            {
                if (kb == null)
                    this.MakeKB();

                return kb; 
            }
        }

        /// <summary>
        /// True if this is the KB object for the global Prolog knowledgebase.
        /// </summary>
        public bool IsGlobal
        {
            get
            {
                return name == GlobalKBGameObjectName;
            }
        }

        public static KB Global
        {
            get
            {
                return GameObject.Find(GlobalKBGameObjectName).GetComponent<KB>();
            }
        }

        internal void Awake()
        {
            if (IsGlobal)
            {
                if (globalKBInitialized)
                    Debug.LogError("There appear to be multiple KB components for the Global Prolog knowledgebase.");
                else
                    globalKBInitialized = true;
                this.MakeKB();
            }
        }

        internal void Start()
        {
            if (kb == null)
                MakeKB();
        }

        private void MakeKB()
        {
            var parent = transform.parent;
            KB parentKB = null;
            if (parent != null)
            {
                parentKB = parent.GetComponent<KB>();
            }

            kb = IsGlobal?
                KnowledgeBase.Global
                : new KnowledgeBase(
                    gameObject.name,
                    gameObject,
                    parentKB == null ? GameObject.Find(GlobalKBGameObjectName).KnowledgeBase()
                                       : parentKB.KnowledgeBase);

            // Add UID counter.
            ELNode.Store(kb.ELRoot/Symbol.Intern("next_uid")%0);

            try
            {
                foreach (var file in SourceFiles)
                    kb.Consult(ExpandFileName(file));
            }
            catch (Exception)
            {
                Debug.Break(); // Pause the game
                throw;
            }
        }

        string ExpandFileName(string path)
        {
            return path.Replace("$NAME", gameObject.name)
                       .Replace("$LEVEL", SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Call prolog query specified by string.
        /// Provided as UI glue.
        /// </summary>
        /// <param name="prologCode">Source code for Prolog query</param>
        public void Run(string prologCode)
        {
            var code = prologCode.EndsWith(".") ? prologCode : prologCode + '.';
            kb.IsTrue(ISOPrologReader.Read(code));
        }
    }
}
