using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using v2;


// Video that this shader is based on and hopefully we can adopt
// https://www.youtube.com/watch?v=Qz0KTGYJtUk

[ExecuteAlways, ImageEffectAllowedInSceneView]
public class Raycast4D : MonoBehaviour {

	[Header("View Settings")]
	[SerializeField] bool useShaderInSceneView;
	[SerializeField] Shader rayTracingShader;
	Material rayTracingMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture target)
    {
        Debug.Log("Hello world");
        if (Camera.current.name != "SceneCamera" || useShaderInSceneView) {
            ShaderHelper.InitMaterial(rayTracingShader, ref rayTracingMaterial);
            UpdateCameraParams(Camera.current);
            Graphics.Blit(null, target, rayTracingMaterial);
        }
        else {
            Graphics.Blit(src, target);
        }
    }

    void UpdateCameraParams(Camera cam) {
        float planeHeight = cam.nearClipPlane * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2;
        float planeWidth = planeHeight * cam.aspect;

        rayTracingMaterial.SetVector("ViewParams", new Vector3(planeWidth, planeHeight, cam.nearClipPlane));
        rayTracingMaterial.SetMatrix("CamLocalToWorldMatrix", cam.transform.localToWorldMatrix);
    }


    Camera4D cam4D;
    Transform4D t4d;
    // Start is called before the first frame update
    void Start()
    {
        cam4D = GetComponentInChildren<Camera4D>();
        t4d = GetComponent<Transform4D>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}