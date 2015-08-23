#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugOverlay.cs" company="Ian Horswill">
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

using System.Text;

using UnityEngine;

namespace Prolog
{
    public class DebugOverlay : MonoBehaviour
    {
        public GUIStyle Style;

        private string text;
        private readonly StringBuilder textBuilder = new StringBuilder();

        private static Texture2D greyOutTexture;
        internal void Start()
        {
            if (greyOutTexture == null)
            {
                greyOutTexture = new Texture2D(1, 1);
                // For some reason, changing the alpha value here has no effect on the amount of greying out
                greyOutTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
            }
        }

        private Vector2 scrollPosition;
        internal void OnGUI()
        {
            switch (Event.current.type)
            {
                case EventType.repaint:
                case EventType.Layout:
                    if (!string.IsNullOrEmpty(text))
                    {
                        GUI.depth = -1;

                        var screenRect = new Rect(0, 0, Screen.width, Screen.height);
                        // For some reason, changing the alpha on greyOutTexture has no effect on the greying out
                        // but drawing the box twice does :(
                        GUI.Box(screenRect, greyOutTexture);
                        GUI.Box(screenRect, greyOutTexture);

                        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                        GUILayout.Label(text, Style);
                        GUILayout.EndScrollView();
                    }
                    break;

                case EventType.keyDown:
                    if (Event.current.keyCode == KeyCode.Escape)
                        Hide();
                    break;
            }

        }

	public void Hide() {
	    text=null;
	}

        public void UpdateText(object payload)
        {
            textBuilder.Length = 0;
            this.Render(payload);
            text = textBuilder.ToString();
        }

        void Render(object renderingOperation)
        {
            renderingOperation = Term.Deref(renderingOperation);
            if (renderingOperation == null)
                return;

            var op = renderingOperation as Structure;
            if (op != null)
            {
                switch (op.Functor.Name)
                {
                    case "cons":
                        this.Render(op.Argument(0));
                        this.Render(op.Argument(1));
                        break;

                    case "line":
                        foreach (var arg in op.Arguments)
                            this.Render(arg);
                        this.textBuilder.AppendLine();
                        break;

                    case "color":
                        this.textBuilder.AppendFormat("<color={0}>", op.Argument(0));
                        for (int i=1; i<op.Arity; i++)
                            this.Render(op.Argument(i));
                        this.textBuilder.Append("</color>");
                        break;

                    case "size":
                        this.textBuilder.AppendFormat("<size={0}>", op.Argument(0));
                        for (int i = 1; i < op.Arity; i++)
                            this.Render(op.Argument(i));
                        this.textBuilder.Append("</size>");
                        break;

                    case "bold":
                        this.textBuilder.AppendFormat("<b>");
                        for (int i = 0; i < op.Arity; i++)
                            this.Render(op.Argument(i));
                        this.textBuilder.Append("</b>");
                        break;

                    case "italic":
                        this.textBuilder.AppendFormat("<i>");
                        for (int i = 0; i < op.Arity; i++)
                            this.Render(op.Argument(i));
                        this.textBuilder.Append("</i>");
                        break;
                    
                    case "term":
                        this.textBuilder.Append(ISOPrologWriter.WriteToString(op.Argument(0)));
                        break;

                    default:
                        this.textBuilder.Append(ISOPrologWriter.WriteToString(op));
                        break;
                }
            }
            else
            {
                var str = renderingOperation as string;
                this.textBuilder.Append(str ?? ISOPrologWriter.WriteToString(renderingOperation));
            }
        }
    }
}
