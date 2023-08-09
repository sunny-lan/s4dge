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
    [SerializeField] bool useRayTracedLighting = false;
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
    ComputeBuffer hyperCubeBuffer;
    ComputeBuffer vertexBuffer;
    ComputeBuffer tetBuffer;
    ComputeBuffer tetMeshBuffer;

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
		SetShaderParams();

	}

	void SetShaderParams()
	{
		rayTracingMaterial.SetInt("MaxBounceCount", maxBounceCount);
		rayTracingMaterial.SetInt("NumRaysPerPixel", numRaysPerPixel);
		rayTracingMaterial.SetFloat("DefocusStrength", defocusStrength);
		rayTracingMaterial.SetFloat("DivergeStrength", divergeStrength);
        rayTracingMaterial.SetInt("UseRayTracedLighting", useRayTracedLighting ? 1 : 0);
	}

    List<Vector4> vertices = new();
    List<int4> tets = new();
    List<TetMesh_shaderdata> tetMeshes = new();

    List<Sphere> spheres = new();
    List<HyperSphere> hyperSpheres = new();
    List<HyperCube> hyperCubes = new();

    void UpdateShapes()
    {
        List<RayTracedShape> shapes = Scene4D.Instance.rayTracedShapes;

        vertices.Clear();
        tets.Clear();
        tetMeshes.Clear();
        spheres.Clear();
        hyperSpheres.Clear();
        hyperCubes.Clear();

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
                            inverseTransform = sphere.transform4D.worldToLocalMatrix,
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
                            inverseTransform = hyperSphere.transform4D.worldToLocalMatrix,
                            radius = hyperSphere.radius,
                            material = hyperSphere.material
                        }
                    );
                    break;
                }
                case ShapeClass.HyperCube:
                {
                    RayTracedHyperCube hyperCube = (RayTracedHyperCube)shape;
                    hyperCubes.Add(
                        new HyperCube()
                        {
                            inverseTransform = hyperCube.transform4D.worldToLocalMatrix,
                            p1 = hyperCube.p1,
                            p2 = hyperCube.p2,
                            material = hyperCube.material
                        }
                    );
                    break;
                }
                case ShapeClass.Tet: 
                    {
                        TetMeshRenderer meshRenderer = (TetMeshRenderer)shape;
                        if (meshRenderer.mesh?.mesh_Raw == null) continue;

                        int idxStart = tets.Count;
                        vertices.AddRange<Vector4>(meshRenderer.mesh.mesh_Raw.vertices.Select(vertex=>vertex.position));
                        tets.AddRange<int4>(meshRenderer.mesh.mesh_Raw.tets.Select(x=>new int4(
                            x.tetPoints[0],
                            x.tetPoints[1],
                            x.tetPoints[2],
                            x.tetPoints[3]
                        )));
                        tetMeshes.Add(new()
                        {
                            inverseTransform = meshRenderer.transform4D.worldToLocalMatrix,
                            idxStart = idxStart,
                            idxEnd = tets.Count
                        });
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

        ShaderHelper.CreateStructuredBuffer(ref hyperCubeBuffer, hyperCubes);
		rayTracingMaterial.SetBuffer("HyperCubes", hyperCubeBuffer);
		rayTracingMaterial.SetInt("NumHyperCubes", hyperCubes.Count);

        ShaderHelper.CreateStructuredBuffer(ref vertexBuffer, vertices);
        rayTracingMaterial.SetBuffer("Vertices", vertexBuffer);
        rayTracingMaterial.SetInt("NumVertices", vertices.Count);

        ShaderHelper.CreateStructuredBuffer(ref tetBuffer, tets);
        rayTracingMaterial.SetBuffer("Tets", tetBuffer);
        rayTracingMaterial.SetInt("NumTets", tets.Count);

        ShaderHelper.CreateStructuredBuffer(ref tetMeshBuffer, tetMeshes);
        rayTracingMaterial.SetBuffer("TetMeshes", tetMeshBuffer);
        rayTracingMaterial.SetInt("NumTetMeshes", tetMeshes.Count);
    }


    void OnDisable()
	{
		ShaderHelper.Release(sphereBuffer);
        ShaderHelper.Release(hyperSphereBuffer);
        ShaderHelper.Release(hyperCubeBuffer);
        ShaderHelper.Release(tetMeshBuffer);
    }

    public struct Sphere
    {
        public TransformMatrixAffine4D inverseTransform;
        public float radius;
        public RayTracingMaterial material;
    }

    public struct HyperSphere
    {
        public TransformMatrixAffine4D inverseTransform;
        public float radius;
        public RayTracingMaterial material;
    };

    public struct HyperCube
    {
        public TransformMatrixAffine4D inverseTransform;
        public Vector4 p1;
        public Vector4 p2;
        public RayTracingMaterial material;
    };

}