
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using v2;
using static UnityEditor.Rendering.CameraUI;

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
        AssetDatabase.Refresh(); //TODO performance

        Debug.Log($"Saved file to {output}");
    }

    [MenuItem("Assets/Convert to S4DGE", true)]
    private static bool ConvertToS4DGEValidation()
    {
        return Selection.activeObject.GetType() == typeof(Mesh);
    }


    [MenuItem("Assets/Merge S4DGEs")]
    private static void MergeS4DGE()
    {
        var objPaths = Selection.assetGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray();
        var objects = objPaths.Select(S4DGE.LoadS4DGE).ToArray();

        InterpolationBasedShape result = S4DGE.MorphMerge(objects[0], objects[1], options: new());
        var folder = Path.GetDirectoryName(objPaths[0]);
        var savePath = Path.Join(folder, 
            string.Join("_", objPaths.Select(Path.GetFileNameWithoutExtension))
            +"_merged.s4dge");

        S4DGE.SaveS4DGE(result, savePath);
        AssetDatabase.Refresh(); //TODO performance
    }

    [MenuItem("Assets/Merge S4DGEs", true)]
    private static bool MergeS4DGEValidation()
    {
        return Selection.assetGUIDs.Length == 2 
            && Selection.assetGUIDs.All(guid => AssetDatabase.GUIDToAssetPath(guid).EndsWith(".s4dge"));
    }


    [MenuItem("Assets/Add S4DGEs")]
    private static void AddS4DGE()
    {
        var objPaths = Selection.assetGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray();
        var objects = objPaths.Select(S4DGE.LoadS4DGE).ToArray();

        InterpolationBasedShape result = S4DGE.AddShapes(objects);
        var folder = Path.GetDirectoryName(objPaths[0]);
        var savePath = Path.Join(folder,
            string.Join("_", objPaths.Select(Path.GetFileNameWithoutExtension))
            + "_added.s4dge");

        S4DGE.SaveS4DGE(result, savePath);
        AssetDatabase.Refresh(); //TODO performance
    }

    [MenuItem("Assets/Add S4DGEs", true)]
    private static bool AddS4DGEValidation()
    {
        return Selection.assetGUIDs.All(guid => AssetDatabase.GUIDToAssetPath(guid).EndsWith(".s4dge"));
    }

    [MenuItem("Assets/Transform S4DGE")]
    private static void TransformS4DGE()
    {
        var objPath = Selection.assetGUIDs.Select(AssetDatabase.GUIDToAssetPath).Single();
        var result = S4DGE.LoadS4DGE(objPath);

        S4DGE.Transform(result, p =>
        {
            p.w++;
            return p;
        });

        var folder = Path.GetDirectoryName(objPath);
        var savePath = Path.Join(folder, $"{Path.GetFileNameWithoutExtension(objPath)}_transformed.s4dge");

        S4DGE.SaveS4DGE(result, savePath);
        AssetDatabase.Refresh(); //TODO performance
    }

    [MenuItem("Assets/Transform S4DGE", true)]
    private static bool TransformS4DGEValidation()
    {
        return Selection.assetGUIDs.Length == 1
            && Selection.assetGUIDs.All(guid => AssetDatabase.GUIDToAssetPath(guid).EndsWith(".s4dge"));
    }
}