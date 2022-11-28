
using UnityEditor;
using UnityEngine;
using v2;

[CustomEditor(typeof(S4DGELoader)), CanEditMultipleObjects]
class S4DGELoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        S4DGELoader loader = (S4DGELoader)target;
        if (GUILayout.Button("Reload"))
            loader.ReloadFile();
    }
}