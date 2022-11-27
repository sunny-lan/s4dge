
using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using v2;

[InitializeOnLoad]
[ExecuteAlways]
public class SceneView4DController : MonoBehaviour 
{
    public static SceneView4DController currentlyDrawingSceneView;

    static SceneView4DController()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    public Transform4D t4d;
    public Camera4D camera4D;
    void Awake()
    {
        SceneView.duringSceneGui += InstanceOnSceneGUI;
        t4d = gameObject.AddComponent<Transform4D>();
        camera4D = gameObject.AddComponent<Camera4D>();

    }

    void OnDestroy()
    {
        SceneView.duringSceneGui -= InstanceOnSceneGUI;
    }

    float w;
    void InstanceOnSceneGUI(SceneView sceneview)
    {
        currentlyDrawingSceneView = this;

        t4d.localScale = Vector4.one;
        t4d.localPosition = new Vector4(0, 0, 0, w);
        

        Handles.BeginGUI();
        w = GUILayout.HorizontalSlider(w, -10, 20);
        Handles.EndGUI();

        currentlyDrawingSceneView = null;
    }

    public static void OnSceneGUI(SceneView sceneview)
    {
        var sceneView4D = sceneview.camera.gameObject.GetComponent<SceneView4DController>();
        if (sceneView4D == null)
        {
            sceneview.camera.gameObject.AddComponent<SceneView4DController>();
            sceneview.Repaint();
        }
    }

}

public class Handles4D
{
    public static void DrawLine(Vector4 a, Vector4 b)
    {
        float minW = a.w, maxW = b.w;
        if(minW > maxW)
        {
            var tmp = minW;
            minW=maxW; 
            maxW=tmp;
        }

        float curW = SceneView4DController.currentlyDrawingSceneView.t4d.localPosition.w;
        if(curW >= minW && curW <= maxW)
            Handles.DrawLine(a, b);
    }
}