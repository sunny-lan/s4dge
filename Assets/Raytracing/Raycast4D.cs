using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using v2;
using Unity.Mathematics;


// Video that this shader is based on and hopefully we can adopt
// https://www.youtube.com/watch?v=Qz0KTGYJtUk

[ExecuteAlways, ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Transform4D))]
[RequireComponent(typeof(Scene4D))]
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
    public Transform4D t4d { get; private set; }
    public RayTracingMaterial defaultMat;

    // Buffers
    ComputeBuffer sphereBuffer;
    ComputeBuffer hyperSphereBuffer;
    ComputeBuffer tetBuffer;

    private void Awake()
    {
        t4d = GetComponent<Transform4D>();
    }

    void OnRenderImage(RenderTexture src, RenderTexture target)
    {
        InitFrame();

        if (Camera.current.name != "SceneCamera" || useShaderInSceneView) {
            if (Camera.current.name == "SceneCamera") {
                // apply scene view camera position/rotation to the t4d when rendering in scene view
                t4d.ApplyTransform3D(Camera.current.cameraToWorldMatrix);
            }

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
        rayTracingMaterial.SetMatrix("CamLocalToWorldMatrix", t4d.localToWorldMatrix.scaleAndRot); // Matrix4x4 no longer affine
        rayTracingMaterial.SetVector("CamTranslation", t4d.localToWorldMatrix.translation); // Extra vector to represent 4d translation
    }


    Camera4D cam4D;
    // Start is called before the first frame update
    void Start()
    {
        cam4D = GetComponent<Camera4D>();
    }

    void InitFrame()
	{
		// Create materials used in blits
		ShaderHelper.InitMaterial(rayTracingShader, ref rayTracingMaterial);
		
		// Update data
		UpdateCameraParams(Camera.current);
        UpdateShapes();
        CreateTets();
		SetShaderParams();

	}

	void SetShaderParams()
	{
		rayTracingMaterial.SetInt("MaxBounceCount", maxBounceCount);
		rayTracingMaterial.SetInt("NumRaysPerPixel", numRaysPerPixel);
		rayTracingMaterial.SetFloat("DefocusStrength", defocusStrength);
		rayTracingMaterial.SetFloat("DivergeStrength", divergeStrength);
	}

    void UpdateShapes()
    {
        List<RayTracedShape> shapes = Scene4D.Instance.rayTracedShapes;
        List<Sphere> spheres = new List<Sphere>();
        List<HyperSphere> hyperSpheres = new List<HyperSphere>();

        foreach (RayTracedShape shape in shapes)
        {
            switch (shape.shapeClass)
            {
                case ShapeClass.Sphere:
                {
                    RayTracedSphere sphere = (RayTracedSphere)shape;
                    spheres.Add(
                        new Sphere()
                        {
                            scaleAndRot = sphere.transform4D.worldToLocalMatrix.scaleAndRot,
                            position = sphere.transform4D.worldToLocalMatrix.translation,
                            radius = sphere.radius,
                            material = sphere.material
                        }
                    );
                    break;
                }
                case ShapeClass.HyperSphere:
                {
                    RayTracedHyperSphere hyperSphere = (RayTracedHyperSphere)shape;
                    hyperSpheres.Add(
                        new HyperSphere()
                        {
                            scaleAndRot = hyperSphere.transform4D.worldToLocalMatrix.scaleAndRot,
                            position = hyperSphere.transform4D.worldToLocalMatrix.translation,
                            radius = hyperSphere.radius,
                            material = hyperSphere.material
                        }
                    );
                    break;
                }
                default:
                {
                    Debug.LogWarning($"Found shape with unhandled shape class {shape.shapeClass}");
                    break;
                }
            }
        }
        
        ShaderHelper.CreateStructuredBuffer(ref sphereBuffer, spheres);
		rayTracingMaterial.SetBuffer("Spheres", sphereBuffer);
		rayTracingMaterial.SetInt("NumSpheres", spheres.Count);

        ShaderHelper.CreateStructuredBuffer(ref hyperSphereBuffer, hyperSpheres);
		rayTracingMaterial.SetBuffer("HyperSpheres", hyperSphereBuffer);
		rayTracingMaterial.SetInt("NumHyperSpheres", hyperSpheres.Count);
    }

    void CreateTets()
    {
        RasterizationRenderer.TetMesh4D_tmp mesh = new();
        mesh.Append(new Vector4[]
        {
             new(1, -1, 0, 0),
             new(-1, -1, 0, 0),
             new(0, 1, 1, 0),
             new(0, 1, -1, 0),
        });
        RasterizationRenderer.HypercubeGenerator.GenerateHypercube(mesh);
        var tets = mesh.tets.Select(t =>
        {
            var points = t.tetPoints.Select(p => mesh.vertices[p].position).ToArray();
            return new Matrix4x4(
            
                points[0],
                points[1],
                points[2],
                points[3]
            ).transpose;
        }).ToArray();

        ShaderHelper.CreateStructuredBuffer(ref tetBuffer, tets);
        rayTracingMaterial.SetBuffer("Tets", tetBuffer);
        rayTracingMaterial.SetInt("NumTets", tets.Length);
    }

    void OnDisable()
	{
		ShaderHelper.Release(sphereBuffer);
        ShaderHelper.Release(hyperSphereBuffer);
        ShaderHelper.Release(tetBuffer);
    }

    public struct Sphere
    {
        public Matrix4x4 scaleAndRot;
        public Vector4 position;
        public float radius;
        public RayTracingMaterial material;
    }

    public struct HyperSphere
    {
        public Matrix4x4 scaleAndRot;
        public Vector4 position;
        public float radius;
        public RayTracingMaterial material;
    };


}