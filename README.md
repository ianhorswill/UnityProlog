This is a simple Prolog interpreter that is compatible with Unity3D,
allowing Prolog code to be mixed with the standard Unity-supported
languages.  It is open source (MIT License), but is easiest to use by
grabbing the DLL and placing it in the Assets folder of your Unity
project.  You also need to copy the files in the Editor subdirectory
to the Editor directory of your Unity project; these will ensure that
your .prolog files get copied over to your standalone builds so that
they can be loaded.

See the Documentation folder for more information.

To compile the code on your own, you either need to copy the Prolog
directory into your Unity project, or build the DLL on your own using
Visual Studio.  The latter will require that you put a copy of the
UnityEngine.dll file from the Unity3D distribution in the top-level
directory of the Visual Studio project.  The Unity DLL is not
distributed with the source for copyright reasons.

Please direct any questions to Ian Horswill <ian@northwestern.edu>.
