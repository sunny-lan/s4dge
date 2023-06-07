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

    [Header("Ray Tracing Settings")]
	[SerializeField, Range(0, 32)] int maxBounceCount = 4;
	[SerializeField, Range(0, 64)] int numRaysPerPixel = 2;
	[SerializeField, Min(0)] float defocusStrength = 0;
	[SerializeField, Min(0)] float divergeStrength = 0.3f;
	[SerializeField, Min(0)] float focusDistance = 1;

	[Header("View Settings")]
	[SerializeField] bool useShaderInSceneView;
	[SerializeField] Shader rayTracingShader;
	
    Material rayTracingMaterial;
    public RayTracingMaterial defaultMat;

    // Buffers
    ComputeBuffer sphereBuffer;

    void OnRenderImage(RenderTexture src, RenderTexture target)
    {
        InitFrame();
        Debug.Log("onRenderImage");
        if (Camera.current.name != "SceneCamera" || useShaderInSceneView) {
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
    // Transform4D t4d;
    // Start is called before the first frame update
    void Start()
    {
        cam4D = GetComponent<Camera4D>();
        // t4d = GetComponent<Transform4D>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("boon testing");
    }

    void InitFrame()
	{
		// Create materials used in blits
		ShaderHelper.InitMaterial(rayTracingShader, ref rayTracingMaterial);
		
		// Update data
		UpdateCameraParams(Camera.current);
		CreateSpheres();
		SetShaderParams();

	}

	void SetShaderParams()
	{
		rayTracingMaterial.SetInt("MaxBounceCount", maxBounceCount);
		rayTracingMaterial.SetInt("NumRaysPerPixel", numRaysPerPixel);
		rayTracingMaterial.SetFloat("DefocusStrength", defocusStrength);
		rayTracingMaterial.SetFloat("DivergeStrength", divergeStrength);
	}

    void CreateSpheres()
	{
        Debug.Log("creating sphere");
		Sphere[] spheres = new Sphere[1];

        spheres[0] = new Sphere()
        {
            position = new Vector3(0,0,25f),
            radius = 15f,
            material = defaultMat
        };

		// Create buffer containing all sphere data, and send it to the shader
		ShaderHelper.CreateStructuredBuffer(ref sphereBuffer, spheres);
		rayTracingMaterial.SetBuffer("Spheres", sphereBuffer);
		rayTracingMaterial.SetInt("NumSpheres", spheres.Length);
	}

    void OnDisable()
	{
		ShaderHelper.Release(sphereBuffer);
	}

    public struct Sphere
    {
        public Vector3 position;
        public float radius;
        public RayTracingMaterial material;
    }
}