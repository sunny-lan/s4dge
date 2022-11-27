
using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using v2;

[InitializeOnLoad]

public class SceneView4DController :MonoBehaviour 
{
    
    static SceneView4DController()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    static float w;
    public static void OnSceneGUI(SceneView sceneview)
    {
        Camera4D sceneView4DCam = sceneview.camera.gameObject.GetComponent<Camera4D>();
        if (sceneView4DCam == null)
        {
            sceneview.camera.gameObject.AddComponent<Transform4D>();
            sceneview.camera.gameObject.AddComponent<Camera4D>();
            sceneview.Repaint();
        }
        else
        {
            sceneView4DCam.t4d.localScale = Vector4.one;
            sceneView4DCam.t4d.localPosition = new Vector4(0, 0, 0, w);
        }

        Handles.BeginGUI();
        w = GUILayout.HorizontalSlider(w, -10, 20);
        Handles.EndGUI();

    }

}
