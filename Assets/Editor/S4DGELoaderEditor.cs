
using System.IO;
using System.Linq;
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

    

    [MenuItem("Assets/Convert to S4DGE")]
    private static void ConvertToS4DGE()
    {
        Mesh mesh = (Mesh)Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(mesh);
        var folder = Path.GetDirectoryName(path);
        var filename = Path.GetFileName(path);

        var convertedShape = S4DGE.MeshToS4DGE(mesh).First(); //TODO submesh support
        var output = Path.Join(folder, $"{filename}.s4dge");
        S4DGE.SaveS4DGE(convertedShape, output);

        Debug.Log($"Saved file to {output}");
    }

    [MenuItem("Assets/Convert to S4DGE", true)]
    private static bool ConvertToS4DGEValidation()
    {
        return Selection.activeObject.GetType() == typeof(Mesh);
    }
}