#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PauseManager.cs" company="Ian Horswill">
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

using UnityEngine;

public class PauseManager : MonoBehaviour
{
    /// <summary>
    /// True if the game is paused
    /// </summary>
    public static bool Paused { get; set; }

    public static bool SingleStep { get; set; }

    static bool ReallyPaused
    {
        get
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return Time.timeScale == 0;
        }

        set
        {
            Time.timeScale = value ? 0 : 1;
        }
    }

    internal void LateUpdate()
    {
        bool shouldBePaused = Paused && !SingleStep;
        // ReSharper disable once RedundantCheckBeforeAssignment
        if (ReallyPaused != shouldBePaused)
        {
            ReallyPaused = shouldBePaused;
        }
    }

    internal void OnGUI()
    {
        switch (Event.current.type)
        {
            case EventType.KeyDown:
                HandleKeypress();
                break;


            case EventType.Repaint:
                RepaintGUI();
                break;
        }
    }

    void HandleKeypress()
    {
        switch (Event.current.keyCode)
        {
            case KeyCode.F5:
                Paused = !Paused;
                break;

            case KeyCode.F10:
                SingleStep = true;
                break;
        }
    }

    public const int PauseGreyoutDepth = 100;

    void RepaintGUI()
    {
        GUI.depth = PauseGreyoutDepth;
        if (ReallyPaused)
        {
            //GUI.Label(new Rect(0, 0, 100, 100), "PAUSED");
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);
        }
    }
}

