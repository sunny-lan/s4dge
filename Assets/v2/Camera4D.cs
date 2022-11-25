using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(Transform4D))]
[RequireComponent(typeof(Camera))] //DO NOT MOVE THE 3D CAMERA
public class Camera4D : MonoBehaviour
{
    /// <summary>
    /// the current active camera
    /// </summary>
    public static Camera4D main
    {
        get
        {
            if(_main == null)
            {
                _main= FindObjectOfType<Camera4D>();
            }
            return _main;
        }

        set
        {
            _main = value;
        }
    }
    static Camera4D _main = null;

    /// <summary>
    /// The 4D transform of this camera.
    /// </summary>
    public Transform4D t4d { get; private set; }

    public Camera camera3D { get; private set; }
    
    private void Awake()
    {
        t4d = GetComponent<Transform4D>();
        camera3D = GetComponent<Camera>();
        cameraMapping[camera3D] = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (main == null)
            main = this;
    }

    //
    // Here, we wire Unity camera rendering events to our own Camera4D events
    // So that we can use them in our own renderers
    //

    private static Dictionary<Camera, Camera4D> cameraMapping = new();
    public static event Action<ScriptableRenderContext, Camera4D> onBeginCameraRendering;


    private void OnDestroy()
    {
        cameraMapping.Remove(camera3D);
    }

    static Camera4D()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    private static void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (cameraMapping.TryGetValue(cam, out var cam4D))
            if (cam4D.enabled)
                    onBeginCameraRendering?.Invoke(ctx, cam4D);
    }

}
