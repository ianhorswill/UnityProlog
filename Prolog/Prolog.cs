#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Prolog.cs" company="Ian Horswill">
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Prolog
{
    /// <summary>
    /// Static class with utilities for interacting with Prolog.
    /// </summary>
    [Documentation("Static class with utilities for interacting with Prolog.")]
    public static class Prolog
    {
        public static string CurrentSourceFile;

        public static string CurrentSourceFileTrimmed
        {
            get
            {
                return TrimPath(CurrentSourceFile);
            }
        }

        public static int CurrentSourceLineNumber;

        public static TextWriter TraceOutput;

        /// <summary>
        /// File name extensions that are recognized as prolog source code.
        /// </summary>
        [Documentation("File name extensions that are recognized as prolog source code.")]
        public static string[] SourceFileExtensions = {".pl", ".p", ".prolog", ".pro"};

        #region Parsing
        /// <summary>
        /// Converts English text string to Prolog-format list of words (symbols).
        /// </summary>
        public static Structure StringToWordList(string text, bool allowUnquote = false, List<LogicVariable> vars = null)
        {
            var b = new StringBuilder();
            var words = new List<object>();
            bool unquoted = false;

            foreach (var c in text)
            {
                if (unquoted)
                {
                    b.Append(c);
                    if (c == ']')
                    {
                        b.Append('.');
                        var s = b.ToString();
                        if (vars == null)
                            vars = new List<LogicVariable>();
                        words.Add(new ISOPrologReader(s, vars).ReadTermWithExistingVars());
                        b.Length = 0;
                        unquoted = false;
                    }
                }
                else
                switch (c)
                {
                    case ' ':
                    case '\t':
                        if (b.Length > 0)
                        {
                            words.Add(GetLexicalItem(b.ToString()));
                            b.Length = 0;
                        }
                        break;

                    case '.':
                    case '!':
                    case ',':
                    case '?':
                    case '(':
                    case ')':
                    case '[':
                    case ']':
                    case '\'':
                    case '"':
                        if (b.Length > 0)
                        {
                            words.Add(GetLexicalItem(b.ToString()));
                            b.Length = 0;
                        }
                        if (c == '[' && allowUnquote)
                        {
                            b.Append(c);
                            unquoted = true;
                        }
                        else
                            words.Add(GetLexicalItem(new string(c, 1)));
                        break;


                    default:
                        b.Append(c);
                        break;
                }
            }

            if (b.Length > 0)
            {
                words.Add(GetLexicalItem(b.ToString()));
            }

            return IListToPrologList(words);
        }

        static readonly Dictionary<string, Symbol> LexicalItems = new Dictionary<string, Symbol>();
        public static Symbol GetLexicalItem(string p)
        {
            var l = p.ToLower();
            Symbol s;
            if (LexicalItems.TryGetValue(l, out s))
                return s;
            return Symbol.Intern(l);
        }

        public static bool IsLexicalItem(string s)
        {
            var l = s.ToLower();
            return LexicalItems.ContainsKey(l); // || Symbol.IsInterned(l);
        }

        /// <summary>
        /// True if string is a prefix of some lexical item other than the string itself.
        /// </summary>
        public static bool IsPrefixOfDistinctLexicalItem(string s)
        {
            foreach (var pair in LexicalItems)
            {
                var item = pair.Key;
                if (item != s && item.StartsWith(s))
                    return true;
            }

            return false;
        }

        public static void RegisterLexicalItem(Symbol s)
        {
            LexicalItems[s.Name.ToLower()] = s;
        }

        /// <summary>
        /// List of enclitics (suffixes attached by a ') in English
        /// </summary>
        public static readonly string[] EnglishEnclitics = { "s", "m", "t", "d", "re", "ll","ve" };

        /// <summary>
        /// Convert Prolog list of words into one text string.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "wordList"), SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WordList"), Documentation("Convert Prolog list of words into one text string.")]
        public static string WordListToString(Structure wordList)
        {
            var s = new StringBuilder();
            string lastToken = null;
            foreach (var word in PrologListToIList(wordList))
            {
                string str = word.ToString();
                if (lastToken != null
                    && char.IsLetterOrDigit(str[0])
                    && !(lastToken == "'" && EnglishEnclitics.Contains(str)))
                    s.Append(' ');
                s.Append(str);

                lastToken = str;
            }
            return s.ToString();
        }

        public static string DefaultSourceFileExtension = ".prolog";

        #endregion

        #region List conversions
        /// <summary>
        /// Convert a scheme-format list (really a .NET IList) to a Prolog list.
        /// </summary>
        [Documentation("Convert a .NET IList to a Prolog list.")]
        // ReSharper disable once InconsistentNaming
        public static Structure IListToPrologList(IList schemeList)
        {
            if (schemeList == null) throw new ArgumentNullException("schemeList");
            Structure listTail = null;
            for (int i = schemeList.Count - 1; i >= 0; i--)
                listTail = new Structure(Symbol.PrologListConstructor, schemeList[i], listTail);
            return listTail;
        }

        /// <summary>
        /// Convert a Prolog-format list into a Scheme list (really a .NET List[object]).
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists"), Documentation("Convert a Prolog-format list into a .NET IList.")]
        public static List<object> PrologListToIList(object prologList)
        {
            object current = Term.Deref(prologList);
            var schemeList = new List<object>(PrologListLength(prologList));
            while (current != null)
            {
                var t = current as Structure;
                if (t == null) throw new ArgumentException("List is not a proper list; it ends with: " + current);
                schemeList.Add(t.Argument(0));
                current = t.Argument(1);
            }
            return schemeList;
        }

        /// <summary>
        /// Iterates over items in a Prolog list.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists"), Documentation("Convert a Prolog-format list into a .NET IList.")]
        public static IEnumerable PrologListItems(object prologList)
        {
            object current = Term.Deref(prologList);
            while (current != null)
            {
                var t = current as Structure;
                if (t == null) throw new ArgumentException("List is not a proper list; it ends with: " + current);
                yield return t.Argument(0);
                current = t.Argument(1);
            }
        }

        /// <summary>
        /// Convert a Prolog-format list into an object[] array.
        /// </summary>
        [Documentation("Convert a Prolog-format list into an object[].")]
        public static object[] PrologListToArray(object prologList)
        {
            object current = Term.Deref(prologList);
            var array = new object[PrologListLength(prologList)];
            int index = 0;
            while (current != null)
            {
                var t = current as Structure;
                if (t == null) throw new ArgumentException("List is not a proper list; it ends with: " + current);
                array[index++] = t.Argument(0);
                current = t.Argument(1);
            }
            return array;
        }

        /// <summary>
        /// Converts an object[] array into a Prolog list.
        /// </summary>
        [Documentation("Converts an object[] array into a Prolog list.")]
        public static Structure ArrayToPrologList<T>(T[] array)
        {
            if (array == null) throw new ArgumentNullException("array");
            Structure result = null;
            for (int i = array.Length - 1; i >= 0; i--)
                result = new Structure(Symbol.PrologListConstructor, array[i], result);
            return result;
        }

        /// <summary>
        /// Computes the length of a prolog list.
        /// </summary>
        [Documentation("Finds the length of a prolog list.")]
        public static int PrologListLength(object prologList)
        {
            object current = Term.Deref(prologList);
            int index = 0;
            while (current != null)
            {
                var t = current as Structure;
                if (t == null)
                {
                    if (current is LogicVariable)
                        throw new ArgumentException("List is incomplete (i.e. it ends in a logic variable).", "prologList");
                    throw new ArgumentException("Argument is not a valid Prolog list", "prologList");
                }
                if (t.IsFunctor(Symbol.PrologListConstructor, 2))
                {
                    index++;
                    current = t.Argument(1);
                }
                else
                    throw new ArgumentException("Argument is not a valid Prolog list", "prologList");
            }
            return index;
        }

        /// <summary>
        /// Computes the length of a prolog list.
        /// </summary>
        [Documentation("Finds the length of a prolog list.")]
        public static object PrologListElement(object prologList, int index)
        {
            object current = Term.Deref(prologList);
            while (current != null)
            {
                var t = current as Structure;
                if (t == null)
                {
                    if (current is LogicVariable)
                        throw new ArgumentException("List is incomplete (i.e. it ends in a logic variable).", "prologList");
                    throw new ArgumentException("Argument is not a valid Prolog list", "prologList");
                }
                if (t.IsFunctor(Symbol.PrologListConstructor, 2))
                {
                    if (index-- == 0)
                        // Done.
                        return t.Argument(0);
                    current = t.Argument(1);
                }
                else
                    throw new ArgumentException("Argument is not a valid Prolog list", "prologList");
            }
            // Hit the end of the list
            throw new IndexOutOfRangeException();
        }
        #endregion

        /// <summary>
        /// Returns the full pathname of the specified file.
        /// </summary>
        /// <param name="path">Path within the application directory</param>
        /// <returns>Full pathname</returns>
        internal static string LoadFilePath(string path)
        {
            if (Path.GetExtension(path) == string.Empty)
                path = path + DefaultSourceFileExtension;
            return Path.Combine(Application.dataPath, path);
        }

        /// <summary>
        /// Returns the full pathname of the specified directory.
        /// </summary>
        /// <param name="path">Path within the application directory</param>
        /// <returns>Full pathname</returns>
        internal static string LoadDirectoryPath(string path)
        {
            return Path.Combine(Application.dataPath, path);
        }

        /// <summary>
        /// Removes Applicaiton.dataPath from the beginning of path.
        /// </summary>
        /// <param name="path">Path to trim</param>
        /// <returns>Trimmed version.</returns>
        public static string TrimPath(string path)
        {
            var prefix = Application.dataPath+Path.DirectorySeparatorChar;
            if (path.StartsWith(prefix))
                return path.Substring(prefix.Length);
            return path;
        }
    }
}
