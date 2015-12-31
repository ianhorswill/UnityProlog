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

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Prolog
{
    public class DebugOverlay : MonoBehaviour
    {
        public GUIStyle Style;

        readonly List<object> displayItems = new List<object>();
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
                    if (displayItems.Count > 0)
                    {
                        GUI.depth = -1;

                        var screenRect = new Rect(0, 0, Screen.width, Screen.height);
                        // For some reason, changing the alpha on greyOutTexture has no effect on the greying out
                        // but drawing the box twice does :(
                        GUI.Box(screenRect, greyOutTexture);
                        GUI.Box(screenRect, greyOutTexture);

                        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                        foreach (var item in displayItems)
                        {
                            if (item is string)
                                GUILayout.Label((string) item, Style);
                            if (item is Table)
                                ((Table) item).Render();
                        }

                        GUILayout.EndScrollView();
                    }
                    break;

                case EventType.keyDown:
                    if (Event.current.keyCode == KeyCode.Escape)
                        Hide();
                    break;
            }

        }

        public void Hide()
        {
            displayItems.Clear();
        }

        public void UpdateText(object payload)
        {
            displayItems.Clear();
            textBuilder.Length = 0;
            Render(payload);
            AddDisplayItem();
        }

        private void AddDisplayItem()
        {
            displayItems.Add(textBuilder.ToString());
            textBuilder.Length = 0;
        }

        private void MaybeAddDisplayItem()
        {
            if (textBuilder.Length>0)
                AddDisplayItem();
        }

        private void Render(object renderingOperation)
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
                        Render(op.Argument(0));
                        Render(op.Argument(1));
                        break;

                    case "line":
                        foreach (var arg in op.Arguments)
                            Render(arg);
                        textBuilder.AppendLine();
                        break;

                    case "color":
                        textBuilder.AppendFormat("<color={0}>", op.Argument(0));
                        for (int i = 1; i < op.Arity; i++)
                            Render(op.Argument(i));
                        textBuilder.Append("</color>");
                        break;

                    case "size":
                        textBuilder.AppendFormat("<size={0}>", op.Argument(0));
                        for (int i = 1; i < op.Arity; i++)
                            Render(op.Argument(i));
                        textBuilder.Append("</size>");
                        break;

                    case "bold":
                        textBuilder.AppendFormat("<b>");
                        for (int i = 0; i < op.Arity; i++)
                            Render(op.Argument(i));
                        textBuilder.Append("</b>");
                        break;

                    case "italic":
                        textBuilder.AppendFormat("<i>");
                        for (int i = 0; i < op.Arity; i++)
                            Render(op.Argument(i));
                        textBuilder.Append("</i>");
                        break;

                    case "term":
                        textBuilder.Append(ISOPrologWriter.WriteToString(op.Argument(0)));
                        break;

                    case "table":
                        MakeTable(op.Argument(0));
                        break;

                    default:
                        textBuilder.Append(ISOPrologWriter.WriteToString(op));
                        break;
                }
            }
            else
            {
                var str = renderingOperation as string;
                textBuilder.Append(str ?? ISOPrologWriter.WriteToString(renderingOperation));
            }
        }

        private void MakeTable(object rows)
        {
            MaybeAddDisplayItem();
            displayItems.Add(new Table(rows, Style));
        }
    }

    class Table
    {
        private readonly List<float> widths = new List<float>(); 
        private readonly List<List<string>> rows = new List<List<string>>();
        private GUIStyle style;
        const int Padding = 20;

        public Table(object rows, GUIStyle style)
        {
            this.style = style;
            //Debug.Log("Making table");
            foreach (var row in Prolog.PrologListItems(rows))
                this.rows.Add(MakeRow(row));
            //Debug.Log("Made table");
        }

        private List<string> MakeRow(object row)
        {
            //Debug.Log("Making row "+ISOPrologWriter.WriteToString(row));
            var rowData = new List<string>();
            int column = 0;
            foreach (var columnData in Prolog.PrologListItems(row))
            {
                //Debug.Log("Making column " + ISOPrologWriter.WriteToString(columnData));
                var data = MakeColumn(columnData);
                rowData.Add(data);
                ReserveColumnWidth(column, data);
                column++;
            }
            return rowData;
        }

        private readonly StringBuilder textBuilder = new StringBuilder();

        private string MakeColumn(object columnData)
        {
            textBuilder.Length = 0;
            AddItem(columnData);
            return textBuilder.ToString();
        }

        // This ought to get refactored so that it's shared with the render code in the main class.
        private void AddItem(object data)
        {
            var op = data as Structure;
            if (op != null)
            {
                switch (op.Functor.Name)
                {
                    case "cons":
                        AddItem(op.Argument(0));
                        AddItem(op.Argument(1));
                        break;

                    case "line":
                        foreach (var arg in op.Arguments)
                            AddItem(arg);
                        textBuilder.AppendLine();
                        break;

                    case "color":
                        textBuilder.AppendFormat("<color={0}>", op.Argument(0));
                        for (int i = 1; i < op.Arity; i++)
                            AddItem(op.Argument(i));
                        textBuilder.Append("</color>");
                        break;

                    case "size":
                        textBuilder.AppendFormat("<size={0}>", op.Argument(0));
                        for (int i = 1; i < op.Arity; i++)
                            AddItem(op.Argument(i));
                        textBuilder.Append("</size>");
                        break;

                    case "bold":
                        textBuilder.AppendFormat("<b>");
                        for (int i = 0; i < op.Arity; i++)
                            AddItem(op.Argument(i));
                        textBuilder.Append("</b>");
                        break;

                    case "italic":
                        textBuilder.AppendFormat("<i>");
                        for (int i = 0; i < op.Arity; i++)
                            AddItem(op.Argument(i));
                        textBuilder.Append("</i>");
                        break;

                    case "term":
                        textBuilder.Append(ISOPrologWriter.WriteToString(op.Argument(0)));
                        break;

                    default:
                        textBuilder.Append(ISOPrologWriter.WriteToString(op));
                        break;
                }
            }
            else
            {
                var str = data as string;
                textBuilder.Append(str ?? ISOPrologWriter.WriteToString(data));
            }
        }

        private void ReserveColumnWidth(int column, string data)
        {
            var width = style.CalcSize(new GUIContent(data)).x;
            if (column == widths.Count)
                widths.Add(width);
            else
                widths[column] = Math.Max(width, widths[column]);
        }

        public void Render()
        {
            foreach (var row in rows)
            {
                GUILayout.BeginHorizontal();
                for (int i = 0; i < row.Count; i++)
                    GUILayout.Label(row[i], style, GUILayout.Width(widths[i]+Padding));
                GUILayout.EndHorizontal();
            }
        }
    }
}
