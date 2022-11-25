
using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
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
            Handles.BeginGUI();
            w = GUILayout.HorizontalSlider(w, -10, 20);
            Handles.EndGUI();
            sceneView4DCam.t4d.position = sceneview.camera.transform.position.withW(w);
            //TODO handle rotation
        }
        
    }

}
