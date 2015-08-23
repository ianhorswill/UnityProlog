#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Symbol.cs" company="Ian Horswill">
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
using System.Diagnostics.CodeAnalysis;

namespace Prolog

{
    /// <summary>
    /// A Lisp Symbol
    /// </summary>
    [Documentation("The class of LISP symbols.")]
    public sealed class Symbol : Term
    {
        #region Fields and Properties
        /// <summary>
        /// The printed representation of the symbol
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        [Documentation("Printed representation of the symbol.")]
        public readonly string Name;

        /// <summary>
        /// The keyword name without the trailing colon.
        /// </summary>
        [Documentation("The keyword name without the trailing colon.")]
        public string KeywordName
        {
            get
            {
                return Name.TrimEnd(':');
            }
        }

        /// <summary>
        /// True if this is a keyword symbol.  Keywords are self-evaluating.
        /// </summary>
        [Documentation("True if this is a keyword symbol, i.e. ends with a colon.  Keywords are self-evaluating.")]
        public bool IsKeyword
        {
            get
            {
                return Name.EndsWith(":");
            }
        }

        /// <summary>
        /// True if this is a pattern variable.
        /// </summary>
        [Documentation("True if this is a pattern variable, either in traditional lisp format (starts with a '?') or Prolog format (starts with a captial letter or '_').")]
        public bool IsPatternVariable
        {
            get
            {
                return Name.StartsWith("?") || Name[0] == '_' || char.IsUpper(Name[0]);
            }
        }
        #endregion

        #region Printing
        /// <summary>
        /// Prints the name of the symbol.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Interning and symbol table
        static readonly Dictionary<string, Symbol> SymbolTable = new Dictionary<string, Symbol>();

        /// <summary>
        /// Returns the symbol with the specified name.
        /// Creates a new symbol if necessary, but always returns the same object
        /// when called with the same string.
        /// </summary>
        [Documentation("Returns the unique symbol iwth the specified name.")]
        public static Symbol Intern(string name)
        {
            Symbol s;
            SymbolTable.TryGetValue(name, out s);
            if (s != null)
                return s;
            // Constructor adds to symbol table automatically
            return new Symbol(name);
        }

        /// <summary>
        /// True if the string names a symbol in the symbol table.  Does not intern a new symbol.
        /// </summary>
        /// <param name="name">Name of the symbol to search for</param>
        /// <returns>True if there is a symbol by that name.</returns>
        public static bool IsInterned(string name)
        {
            return SymbolTable.ContainsKey(name);
        }
        #endregion

        #region Constructor - private -
        Symbol(string name)
        {
            //Prolog allows null atoms, alas.
            //System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(name));
            Name = name;
            SymbolTable[name] = this;
        }
        #endregion

        #region Unification
        internal override IEnumerable<bool> UnifyWithTerm(Term value)
        {
            return value.UnifyWithAtomicConstant(this);
        }
        internal override bool UnifyWithTerm(Term value, PrologContext context)
        {
            return value.UnifyWithAtomicConstant(this, context);
        }
        
        #endregion

        #region Symbol constants
        /// <summary>
        /// The symbol 'quote
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Quote = Intern("quote");
        /// <summary>
        /// The symbol 'quasiquote
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Quasiquote")]
        public static readonly Symbol Quasiquote = Intern("quasiquote");
        /// <summary>
        /// The symbol 'unquote
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Unquote = Intern("unquote");
        /// <summary>
        /// The symbol 'unquote-splicing
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol UnquoteSplicing = Intern("unquote-splicing");

        /// <summary>
        /// The symbol 'lambda
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Lambda = Intern("lambda");
        /// <summary>
        /// The symbol 'member
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Member = Intern("member");

        /// <summary>
        /// The symbol 'macroexpand
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Macroexpand")]
        public static readonly Symbol Macroexpand = Intern("macroexpand");

        /// <summary>
        /// The symbol 'else
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Else = Intern("else");
        /// <summary>
        /// The symbol 'if
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol If = Intern("if");
        /// <summary>
        /// The symbol 'begin
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Begin = Intern("begin");
        /// <summary>
        /// The symbol 'set!
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol SetBang = Intern("set!");
        /// <summary>
        /// The symbol 'define
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Define = Intern("define");
        /// <summary>
        /// The symbol 'let
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Let = Intern("let");
        
        /// <summary>
        /// The symbol 'new
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol New = Intern("new");
        /// <summary>
        /// The symbol 'not
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Not = Intern("not");
        /// <summary>
        /// The symbol 'list
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol List = Intern("list");
        /// <summary>
        /// The symbol 'append
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Append = Intern("append");

        /// <summary>
        /// The symbol 'this
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol This = Intern("this");

        /// <summary>
        /// The symbol 'stack
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Stack = Intern("stack");

        /// <summary>
        /// The symbol '...
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Ellipsis = Intern("...");

        /// <summary>
        /// The symbol 'Object
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Object = Intern("Object");

        /// <summary>
        /// The Prolog symbol ','
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Comma = Intern(",");

        /// <summary>
        /// The symbol '='
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol EqualSign = Intern("=");
        /// <summary>
        /// The Prolog symbol '|'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol VerticalBar = Intern("|");
        /// <summary>
        /// The Prolog symbol ':-'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Implication = Intern(":-");
        /// <summary>
        /// The Prolog symbol '.'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol PrologListConstructor = Intern("cons");
        /// <summary>
        /// The Prolog symbol '.'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Dot = Intern(".");
        /// <summary>
        /// The Prolog symbol '!'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Cut = Intern("!");

        /// <summary>
        /// The Prolog symbol 'call'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Call = Intern("call");

        /// <summary>
        /// The Prolog symbol 'fail'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Fail = Intern("fail");

        /// <summary>
        /// The Prolog symbol 'true'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol True = Intern("true");

        /// <summary>
        /// The Prolog symbol '{}'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol CurlyBrackets = Intern("{}");

        /// <summary>
        /// The Prolog symbol '-->'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol GrammarRule = Intern("-->");

        /// <summary>
        /// The Prolog symbol '$'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol DollarSign = Intern("$");

        /// <summary>
        /// The Prolog symbol 'end_of_file'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol EndOfFile = Intern("end_of_file");

        /// <summary>
        /// The division symbol, '/'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Slash = Intern("/");
        /// <summary>
        /// The grammar rule predicate indicator symbol, '//'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol SlashSlash = Intern("//");
        /// <summary>
        /// The symbol 'maplist'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol MapList = Intern("maplist");
        /// <summary>
        /// The Prolog symbol ';'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Semicolon = Intern(";");
        /// <summary>
        /// The Prolog symbol '->'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol IfThenArrow = Intern("->");
        /// <summary>
        /// The Prolog symbol ':'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Colon = Intern(":");
        /// <summary>
        /// The Prolog symbol 'global'
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Symbol Global = Intern("global");

        /// <summary>
        /// The Prolog symbol '-'
        /// </summary>
        public static readonly Symbol Minus = Intern("-");
        /// <summary>
        /// The Prolog symbol '::'
        /// </summary>
        public static readonly Symbol ColonColon = Intern("::");

        #endregion
    }
}
