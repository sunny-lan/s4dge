using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestTetrahedron
{
    ComputeShader testor;

    [UnitySetUp]
    public IEnumerator Setup()
    {

        EditorSceneManager.LoadSceneInPlayMode("Assets/Raytracing/Tests/TetrahedronTestScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return null; // wait until scene finishes loading

        foreach (var shader in Resources.FindObjectsOfTypeAll<ComputeShader>())
        {
            if (shader.name == "TetrahedronTestor")
            {
                testor = shader;
                break;
            }
        }
        Assert.IsNotNull(testor);

        
    }

    struct Hyperplane
    {
        public Vector4 normal;
        public float offset;
    }

    struct Tet
    {
        public Hyperplane edges1, edges2, edges3, edges4;
        public  Hyperplane volume;
    }


    struct RayTracingMaterial
    {
        public Vector4 colour;
        public Vector4 emissionColour;
        public Vector4 specularColour;
        public float emissionStrength;
        public float smoothness;
        public float specularProbability;
        public int flag;
    };

    struct MeshInfo
    {
        public uint firstTriangleIndex;
        public uint numTriangles;
        public RayTracingMaterial material;
        public Vector3 boundsMin;
        public Vector3 boundsMax;
    };

    struct HitInfo
    {
        public byte didHit;
        public float dst;
        public Vector4 hitPoint;
        public Vector4 normal;
        public float numHits;
        public RayTracingMaterial material;
    };

    // A Test behaves as an ordinary method
    [Test]
    public void TestTetrahedronSimplePasses()
    {
        using ComputeBuffer _tet = new(1, UnsafeUtility.SizeOf<Tet>());
        using ComputeBuffer _hit = new(1, UnsafeUtility.SizeOf<HitInfo>());

        var kernel = testor.FindKernel("CSMain");
        testor.SetBuffer(kernel, "ret", _tet);
        testor.SetBuffer(kernel, "hitInfo", _hit);

        testor.Dispatch(kernel, 1, 1, 1);

        Tet[] tet = new Tet[1];
        _tet.GetData(tet);

        HitInfo[] hit = new HitInfo[1];
        _hit.GetData(hit);

        Debug.Log("normal = " + tet[0].volume.normal);
        Debug.Log("offset = " + tet[0].volume.offset);
        Debug.Log("didHit = " + hit[0].didHit);
        Debug.Log("hit dst = " + hit[0].dst);
    }
}
