
using UnityEditor;
using UnityEngine;
using v2;

[CustomEditor(typeof(S4DGELoader)), CanEditMultipleObjects]
class S4DGELoaderEditor : Editor
{

    // reloading the file as workaround for broken shape bug
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        //TODO this is so sus
        foreach (var obj in FindObjectsOfType<S4DGELoader>())
        {
            obj.ReloadFile();
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        S4DGELoader loader = (S4DGELoader)target;
        if (GUILayout.Button("Reload"))
            loader.ReloadFile();
    }
}