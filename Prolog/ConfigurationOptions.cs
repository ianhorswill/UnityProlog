#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationOptions.cs" company="Ian Horswill">
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

using Prolog;

using UnityEngine;

internal class ConfigurationOptions : MonoBehaviour
{
    public string Subtree="configuration";
    public ConfigurationOption[] Options= {};

    internal void Awake()
    {
        var kb = this.GetComponent<KB>();
        foreach (var option in Options)
            if (option.Selected)
            {
                kb.IsTrue(ISOPrologReader.Read(string.Format("assert(/{0}/{1}).",Subtree, option.Name)));
            }
    }

    [Serializable]
    public class ConfigurationOption
    {
        public Boolean Selected=false;
        public string Name="";
    }
}
