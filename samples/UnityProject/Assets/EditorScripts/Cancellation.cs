#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class CancelDefaultFSharpOnEditorExit
{
    static CancelDefaultFSharpOnEditorExit()
    {
        EditorApplication.playModeStateChanged += CancelFSharpToken;
    }
    
    static void CancelFSharpToken(PlayModeStateChange state) 
    {
        if (state == PlayModeStateChange.ExitingPlayMode) {
            Microsoft.FSharp.Control.FSharpAsync.CancelDefaultToken();
            Debug.Log("Canceled FSharp async token");
        }
    }
}
#endif
