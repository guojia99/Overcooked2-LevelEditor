using UnityEngine;
using UnityEditor;


[InitializeOnLoad]
public class SnapInitializer
{
    static SnapInitializer()
    {
        EditorPrefs.SetFloat("MoveSnapX", 1.2f);
        EditorPrefs.SetFloat("MoveSnapY", 1f);
        EditorPrefs.SetFloat("MoveSnapZ", 1.2f);
    }
}