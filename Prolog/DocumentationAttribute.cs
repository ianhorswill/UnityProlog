#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationAttribute.cs" company="Ian Horswill">
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
using System.Reflection;

namespace Prolog
{
    /// <summary>
    /// Describes in which versions of the manual this item should be included.
    /// </summary>
    [Flags]
    public enum DocumentationTypes
    {
        /// <summary>
        /// LISP version of the manual
        /// </summary>
        Lisp = 1,
        /// <summary>
        /// C# version of the manual
        /// </summary>
        // ReSharper disable once InconsistentNaming
        CS = 2,
        /// <summary>
        /// Both versions of the manual
        /// </summary>
        All = 3
    };

    /// <summary>
    /// Tags a class or member with a documentation string for use in
    /// online help and automatic generation of the manual.
    /// </summary>
    [AttributeUsage(AttributeTargets.All), Documentation("Attaches a textual description to the type or member, for incorporation into the Twig manual and use in the online documentation.")]
    public sealed class DocumentationAttribute : Attribute
    {
        /// <summary>
        /// English text describing the attached Type or member.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Specifies which version(s) the manual this item should be included in.
        /// </summary>
        public DocumentationTypes ManualVersion { get; private set; }

        /// <summary>
        /// Tag a class or member with a documentation string for use in
        /// online help and automatic generation of the manual.
        /// </summary>
        public DocumentationAttribute(string description, DocumentationTypes manualVersion)  // url is a positional parameter
        {
            Description = description;
            ManualVersion = manualVersion;
        }

        /// <summary>
        /// Tag a class or member with a documentation string for use in
        /// online help and automatic generation of the manual.
        /// </summary>
        public DocumentationAttribute(string description)
            : this(description, DocumentationTypes.All)
        { }
    }

    /// <summary>
    /// Adds extension methods for reading documentation annotations of Types and MemberInfo objects.
    /// </summary>
    public static class DocumentationExtensions
    {
        /// <summary>
        /// Get documentation string of member, if any.
        /// </summary>
        public static string GetDocumentation(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
                throw new ArgumentNullException("memberInfo");

            object[] a = memberInfo.GetCustomAttributes(typeof(DocumentationAttribute), false);
            if (a.Length == 0)
                return null;
            return ((DocumentationAttribute)a[0]).Description;
        }
    }
}