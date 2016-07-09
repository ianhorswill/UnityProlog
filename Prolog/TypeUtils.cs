#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeUtils.cs" company="Ian Horswill">
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Object = UnityEngine.Object;

namespace Prolog
{
    /// <summary>
    /// 
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils")]
    public static class TypeUtils
    {
        /// <summary>
        /// List of expansions of type names to try when looking for a type.
        /// </summary>
        static readonly List<string> TypeSearchPaths = new List<string> {
                                                           "{0}, Assembly-CSharp",
                                                           "{0}, Assembly-UnityScript",
                                                           "{0}, Assembly-Boo",
                                                           "Prolog.{0}, Prolog",
                                                           "UnityEngine.{0}, UnityEngine",
                                                           "UnityEngine.UI.{0}, UnityEngine.UI",
                                                           "System.{0}"
                                                       };

        /// <summary>
        /// Add a default namespace and assembly to search when looking for types
        /// </summary>
        /// <param name="namespacePrefix">Namespace to search</param>
        /// <param name="assembly">Assembly to search</param>
        /// <returns></returns>
        static public bool AddTypeSearchPath(string namespacePrefix, string assembly)
        {
            if (namespacePrefix == null && assembly == null)
                throw new ArgumentNullException("At least one argument to AddTypeSearchPath must be non-null.");

            if (assembly==null)
                TypeSearchPaths.Add(namespacePrefix+".{0}");
            else if (namespacePrefix == null)
                TypeSearchPaths.Add("{0}, "+assembly);
            else
            {
                TypeSearchPaths.Add(namespacePrefix+".{0}, "+assembly);
            }
            return true;
        }

        /// <summary>
        /// Makes a best effort to find the type object corresponding to the type with specified name.
        /// </summary>
        /// <param name="name">Name of the type</param>
        /// <returns>The type object with the specified name</returns>
        public static Type FindType(string name)
        {
            // Kluge to deal with the fact that the tops of unity's type hierarchy and .NET's have the same $@#$#@ name.
            if (name == "unity_object")
                return typeof(Object);

            if (name.IndexOf(',') >= 0)
                // It's already qualified with an assembly
                return Type.GetType(name, false, true);
            if (name.IndexOf('.') >= 0)
                // It's already qualified with a namespace
                return Type.GetType(name, false, true)
                       ?? Type.GetType(name + ", Assembly-CSharp", false, true)
                       ?? Type.GetType(name + ", Assembly-UnityScript", false, true)
                       ?? Type.GetType(name + ", Assembly-Boo", false, true)
                       ?? Type.GetType(name + ", UnityEngine", false, true)
                       ?? Type.GetType(name + ", Prolog", false, true);
            var t = Type.GetType(name, false, true);
            if (t != null)
                return null;
            foreach (var path in TypeSearchPaths)
            {
                t = Type.GetType(string.Format(path, name), false, true);
                if (t != null)
                    return t;
            }
            return null;
        }

        /// <summary>
        /// If type T is a subclass of some instance type of genericTypeDefinition, return that instance type.
        /// Otherwise return null.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "t")]
        public static Type GenericTypeInstanceOf(this Type t, Type genericTypeDefinition)
        {
            while (t != null && t.GetGenericTypeDefinition() != genericTypeDefinition)
                t = t.BaseType;
            return t;
        }

        /// <summary>
        /// Creates an instance of the specified type
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "t")]
        public static object CreateInstance(this Type type, params object[] constructorArguments)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (constructorArguments == null) throw new ArgumentNullException("constructorArguments");
            
            return type.InvokeMember(null, BindingFlags.CreateInstance, null, null, constructorArguments);
        }

        /// <summary>
        /// Creates an instance of the specified type
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "t"), SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static object CreateInstance(this Type type, List<object> constructorArguments)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (constructorArguments == null) throw new ArgumentNullException("constructorArguments");
            
            return type.CreateInstance(constructorArguments.ToArray());
        }

        /// <summary>
        /// Creates an instance of the specified type, taking as the constructor arguments
        /// all the elements of args up to, but not including, the first keyword, then uses
        /// the rest of the arguments as name/value pairs for initializing fields of the
        /// new object.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "t"), SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static object CreateInstanceWithInitializerKeywords(this Type type, List<object> constructorArguments)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (constructorArguments == null) throw new ArgumentNullException("constructorArguments");
            
            var constructorArgs = new List<object>();
            // Everything up to the first keyword is a constructor arg
            int i;
            for (i = 0; i < constructorArguments.Count; i++)
            {
                var s = constructorArguments[i] as Symbol;
                if (s != null && s.IsKeyword)
                    break;
                constructorArgs.Add(constructorArguments[i]);
            }
            if (((constructorArguments.Count - i) & 1) != 0)
                throw new ArgumentException("Odd number of initializer arguments provided");
            object o = type.CreateInstance(constructorArgs);
            for (; i + 1 < constructorArguments.Count; i += 2)
            {
                var name = constructorArguments[i] as Symbol;
                if (name == null)
                    throw new ArgumentException("Improper initializer name: " + constructorArguments[i]);
                o.SetPropertyOrField(name.KeywordName, constructorArguments[i + 1]);
            }
            return o;
        }

        /// <summary>
        /// Creates an instance of a generic type given its definition, type parameters, and constructor args.
        /// </summary>
        public static object CreateGenericInstance(this Type genericType, Type[] genericTypeParameters, params object[] constructorArguments)
        {
            if (genericType == null) throw new ArgumentNullException("genericType");
            if (genericTypeParameters == null) throw new ArgumentNullException("genericTypeParameters");
            if (constructorArguments == null) throw new ArgumentNullException("constructorArguments");

            return genericType.MakeGenericType(genericTypeParameters).CreateInstance(constructorArguments);
        }

        /// <summary>
        /// Creates an instance of a generic type given its definition, type parameters, and constructor args.
        /// </summary>
        public static object CreateGenericInstance(this Type genericType, Type genericTypeParameter, params object[] constructorArguments)
        {
            if (genericType == null) throw new ArgumentNullException("genericType");
            if (genericTypeParameter == null) throw new ArgumentNullException("genericTypeParameter");
            if (constructorArguments == null) throw new ArgumentNullException("constructorArguments");

            return genericType.MakeGenericType(new[] { genericTypeParameter }).CreateInstance(constructorArguments);
        }

        /// <summary>
        /// Creates an instance of a generic type given its definition, type parameters, and constructor args.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static object CreateGenericInstance(this Type genericType, Type[] genericTypeParameters, List<object> constructorArguments)
        {
            if (genericType == null) throw new ArgumentNullException("genericType");
            if (genericTypeParameters == null) throw new ArgumentNullException("genericTypeParameters");
            if (constructorArguments == null) throw new ArgumentNullException("constructorArguments");

            return genericType.CreateGenericInstance(genericTypeParameters, constructorArguments.ToArray());
        }

        /// <summary>
        /// Calls the specified instance method with the specified arguments.
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Type.InvokeMember(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Object,System.Object[])")]
        public static object InvokeMethod(this object value, string methodName, params object[] args)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (methodName == null) throw new ArgumentNullException("methodName");
            var type = value as Type;
            if (type != null)
                return type.InvokeMember(methodName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static 
                    | BindingFlags.InvokeMethod | BindingFlags.OptionalParamBinding,
                    null, type, args);
            return value.GetType().InvokeMember(methodName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                | BindingFlags.InvokeMethod | BindingFlags.OptionalParamBinding, null, value, args);
        }

        private const BindingFlags FieldOrPropertyBindingFlags =
            BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        /// <summary>
        /// Returns the value of the object's specified field or property.
        /// </summary>
        public static object GetPropertyOrField(this object value, string memberName)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (memberName == null) throw new ArgumentNullException("memberName");
            var t = value as Type;
            if (t != null)
            {
                // value is a type; check for both static fields and type fields.
                // Check for static fields
                FieldInfo f = t.GetField(memberName, FieldOrPropertyBindingFlags);
                if (f != null && f.IsStatic)
                    return f.GetValue(value);
                PropertyInfo p = t.GetProperty(memberName, FieldOrPropertyBindingFlags);
                if (p != null)
                    return p.GetValue(value, new object[0]);
                // Now check for instance fields of the type object itself.
                t = value.GetType();
                f = t.GetField(memberName, FieldOrPropertyBindingFlags);
                if (f != null)
                    return f.GetValue(value);
                p = t.GetProperty(memberName, FieldOrPropertyBindingFlags);
                if (p != null)
                    return p.GetValue(value, new object[0]);
            }
            else
            {
                t = value.GetType();
                FieldInfo f = t.GetField(memberName, FieldOrPropertyBindingFlags);
                if (f != null)
                    return f.GetValue(value);
                PropertyInfo p = t.GetProperty(memberName, FieldOrPropertyBindingFlags);
                if (p != null)
                    return p.GetValue(value, new object[0]);
            }
            throw new MissingMemberException(t.Name, memberName);
        }

        /// <summary>
        /// Sets property or field value of object.
        /// Properly handles inheritance of virtual properties.
        /// Throws exception is member not found.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj"), SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "mname")]
        public static void SetPropertyOrField(this object obj, string mname, object value)
        {
            TrySetPropertyOrField(obj, mname, value, true);
        }

        /// <summary>
        /// Sets property or field value of object.
        /// Properly handles inheritance of virtual properties.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object"), SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj"), SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "mname")]
        public static bool TrySetPropertyOrField(this object objectToModify, string memberName, object value, bool errorOnMissing)
        {
            if (memberName == null) throw new ArgumentNullException("memberName");
            if (objectToModify == null) throw new ArgumentNullException("objectToModify");

            var mtype = objectToModify as Type;
            BindingFlags bflags = BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            if (mtype == null)
            {
                mtype = objectToModify.GetType();
                bflags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            }
            MemberInfo[] mi = mtype.GetMember(memberName, bflags);
            if (mi.Length < 1)
            {
                if (errorOnMissing)
                    throw new MissingMemberException(objectToModify.GetType().Name, memberName);
                return false;
            }
            if (mi.Length == 1)
            {
                var f = mi[0] as FieldInfo;
                if (f != null)
                    f.SetValue(objectToModify, value);
                else
                {
                    var p = mi[0] as PropertyInfo;
                    if (p != null)
                    {
                        MethodInfo setter = null;
                        // If reflection finds an overload for the property that only overloads
                        // the get method, then GetSetAccessor, SetValue, InvokeMethod, etc. all
                        // fail.  So we need to manually crawl up the type hierarchy to find a
                        // set method.
                        // ReSharper disable once PossibleNullReferenceException
                        while (p != null && (setter = p.GetSetMethod()) == null && p.DeclaringType.BaseType != null)
                        {
                            p = p.DeclaringType.BaseType.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                        }

                        if (setter != null)
                            setter.Invoke(objectToModify, new[] { value });
                        else
                            throw new MissingMethodException(mtype.Name, "setter for " + memberName);
                    }
                    else
                        throw new NotSupportedException(memberName + " is an unsupported type of member");
                }
            }
            else
            {
                throw new NotSupportedException("Can't assign methods.");
            }
            return true;
        }
    }
}
