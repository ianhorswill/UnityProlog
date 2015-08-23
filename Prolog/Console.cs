#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Console.cs" company="Ian Horswill">
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
using System.IO;
using System.Text;

using UnityEngine;

//
// Console window driver by Lee Fan.
// Modifications by Ian Horswill
//

namespace Northwestern.UnityUtils
{
    public class Console : MonoBehaviour
    {
        public Rect WindowRect = new Rect(0, 0, 640, 480); //Defines console size and dimensions
        public string WindowTitle = "Console";
        public string Header = ""; //First thing shown when console starts

        public KeyCode ActivationKey = KeyCode.F2;	//Key used to show/hide console
        public bool ShowConsole = false;				//Whether or not console is visible


        //Public variables for writing to console stdout and stdin
        public string In;

        public ConsoleWriter Out;

        /// <summary>
        /// CharacterNameStyle in which to display text.
        /// </summary>
        public GUIStyle Style;

        // ReSharper disable once InconsistentNaming
        protected static int IDCount = typeof(Console).GetHashCode();

        // ReSharper disable once InconsistentNaming
        private int ID; //unique generated ID

        // ReSharper disable once InconsistentNaming
        private string consoleID; //generated from ID

        private string consoleBuffer; //Tied to GUI.Label

        private Vector2 scrollPosition;

        // Set when written to output; forces scroll to bottom left.
        private bool forceScrollToEnd;

        private bool firstFocus; //Controls console input focus

        /// <summary>
        /// List of all the things the commands the user has typed
        /// </summary>
        private List<string> history;
        /// <summary>
        /// Position in the history list when recalling previous commands
        /// </summary>
        private int historyPosition;

        /// <summary>
        /// Initializes console properties and sets up environment.
        /// </summary>
        internal virtual void Start()
        {
            Initialize();
            In = string.Empty;
            Out = new ConsoleWriter();
            consoleBuffer = Header;
            if (consoleBuffer != "")
                Out.WriteLine(consoleBuffer);
            scrollPosition = Vector2.zero;
            ID = IDCount++;
            this.consoleID = "window" + ID;
            history = new List<string>();
        }

        /// <summary>
        /// Creates the Console Window.
        /// </summary>
        /// <param name='windowID'>
        /// unused parameter.
        /// </param>
        // ReSharper disable once InconsistentNaming
        private void DoConsoleWindow(int windowID)
        {
            //Console Window
            GUI.DragWindow(new Rect(0, 0, this.WindowRect.width, 20));
            //Scroll Area
            scrollPosition = GUILayout.BeginScrollView(
                scrollPosition,
                GUILayout.MaxHeight(this.WindowRect.height - 48),
                GUILayout.ExpandHeight(false),
                GUILayout.Width(this.WindowRect.width - 15));
            //Console Buffer
            GUILayout.Label(consoleBuffer, Style, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
            if (forceScrollToEnd)
            {
                scrollPosition = new Vector2(0, Mathf.Infinity);
                forceScrollToEnd = false;
            }
            //Input Box
            GUI.SetNextControlName(this.consoleID);
            In = GUI.TextField(new Rect(4, this.WindowRect.height - 24, this.WindowRect.width - 8, 20), In, Style);
            if (firstFocus)
            {
                GUI.FocusControl(this.consoleID);
                firstFocus = false;
            }
        }

        internal void OnGUI()
        {
            if (this.ShowConsole)
            {
                this.WindowRect = GUI.Window(ID, this.WindowRect, this.DoConsoleWindow, WindowTitle);
            }
            if (Event.current.isKey && Event.current.type == EventType.KeyUp)
            {
                var weAreFocus = GUI.GetNameOfFocusedControl() == this.consoleID;
                if (Event.current.keyCode == ActivationKey)
                {
                    this.ShowConsole = !this.ShowConsole;
                    firstFocus = true;
                }
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:
                        if (weAreFocus && In != string.Empty)
                        {
                            scrollPosition = GUI.skin.label.CalcSize(new GUIContent(consoleBuffer));
                            string command = In;
                            In = string.Empty;
                            if (!this.OmitCommandFromHistory(command))
                            {
                                history.Add(command);
                                historyPosition = history.Count;
                            }
                            Run(command);
                        }
                        break;

                    case KeyCode.UpArrow:
                        if (historyPosition > 0)
                        {
                            In = history[--historyPosition];
                        }
                        break;

                    case KeyCode.DownArrow:
                        if (historyPosition < history.Count-1)
                        {
                            In = history[++historyPosition];
                        }
                        break;
                }
                if (weAreFocus)
                    Event.current.Use();
            }
            if (Out != null && Out.IsUpdated())
            {
                consoleBuffer = Out.GetTextUpdate();
                forceScrollToEnd = true;
            }
        }

        /// <summary>
        /// A TextWriter for output buffer
        /// </summary>
        public class ConsoleWriter : TextWriter
        {
            private bool bufferUpdated;

            //tracks when changes are made to StringBuilder to prevent generating new strings every click

            private readonly StringBuilder oBuffer;

            public ConsoleWriter()
            {
                oBuffer = new StringBuilder();
            }

            public override Encoding Encoding
            {
                get
                {
                    return Encoding.Default;
                }
            }

            public override void Write(string value)
            {
                oBuffer.Append(value);
                bufferUpdated = true;
            }

            public override void WriteLine(string value)
            {
                oBuffer.AppendLine(value);
                bufferUpdated = true;
            }

            public override void WriteLine()
            {
                this.WriteLine("");
            }

            public string GetTextUpdate()
            {
                bufferUpdated = false;
                return oBuffer.ToString();
            }

            public bool IsUpdated()
            {
                return bufferUpdated;
            }
        }

        /// <summary>
        /// Run when a newline is entered in the input box.
        /// </summary>
        /// <param name='command'>
        /// The entered text prior to the newline.
        /// </param>
        protected virtual void Run(string command)
        {
            Out.WriteLine(">> " + command);
            //override for functionality
        }

        /// <summary>
        /// Allows for initialization of <code>Width</code>, <code>Height</code>, <code>Header</code>, <code>ActivationKey</code>, and <code>showConsole</code>
        /// </summary>
        protected virtual void Initialize()
        {
            //override to set console properties
        }

        protected virtual bool OmitCommandFromHistory(string command)
        {
            return false;
        }
    }
}
