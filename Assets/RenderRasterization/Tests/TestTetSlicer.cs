using NUnit.Framework;
using NUnit.Framework.Internal;
using RasterizationRenderer;
using RenderingCommon;
using System;
using System.Collections;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestTetSlicer
{
    ComputeShader sliceShader;
    ComputeBuffer vertexBuffer;
    ComputeBuffer tetBuffer;

    TetMesh4D.VertexData[] vertexDataNoIntersection = new TetMesh4D.VertexData[]
    {
        new TetMesh4D.VertexData(new Vector4(0.0f, 0.0f, 1.0f, 0.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
    };

    TetMesh4D.VertexData[] vertexDataTriIntersection = new TetMesh4D.VertexData[]
    {
        new TetMesh4D.VertexData(new Vector4(0.0f, 0.0f, 0.0f, 0.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(1.0f, 0.0f, -1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
    };

    TetMesh4D.VertexData[] vertexDataQuadIntersection = new TetMesh4D.VertexData[]
    {
        new TetMesh4D.VertexData(new Vector4(0.0f, 0.0f, -1.0f, 0.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(0.0f, 1.0f, -1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
    };

    static TetMesh4D.Tet4D tetForwardFacing = new(new int[] { 0, 1, 2, 3 });
    TetMesh4D.Tet4D[] tetsForwardFacing = new TetMesh4D.Tet4D[] { tetForwardFacing };

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/RasterizationTestScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return null; // wait until scene finishes loading

        foreach (var shader in Resources.FindObjectsOfTypeAll<ComputeShader>())
        {
            if (shader.name == "TetrahedronSlicer")
            {
                sliceShader = shader;
                break;
            }
        }
        Assert.IsNotNull(sliceShader);
    }

    [UnityTearDown]
    public void TearDown()
    {

    }

    public void RunTetTest(TetMesh4D.VertexData[] vertices, TetMesh4D.Tet4D[] tets, float[] expectedVertices, int[] expectedTris)
    {
        Debug.Log("tet vertices: " + string.Join(",", vertices));
        Debug.Log("tets: " + string.Join(",", tets));

        vertexBuffer = RenderUtils.InitComputeBuffer(TetMesh4D.VertexData.SizeBytes, vertices);
        var tetrahedraUnpacked = tets.SelectMany(tet => tet.tetPoints).ToArray();
        tetBuffer = RenderUtils.InitComputeBuffer(sizeof(int) * TetMesh4D.PTS_PER_TET, tetrahedraUnpacked);
        TetSlicer slicer = new(sliceShader, tetBuffer, tets.Length);
        var slicedTriangles = slicer.Render(vertexBuffer, 0);

        int[] slicedTriBuffer = new int[expectedTris.Length];
        float[] slicedTriVertexBuffer = new float[expectedVertices.Length];
        slicedTriangles.Buffers[0].Buffer.GetData(slicedTriBuffer);
        slicedTriangles.Buffers[1].Buffer.GetData(slicedTriVertexBuffer);

        Debug.Log("num tris: " + slicedTriangles.Buffers[0].Count);
        Debug.Log("triangles: " + string.Join(",", slicedTriBuffer));
        Debug.Log("num vertices: " + slicedTriangles.Buffers[1].Count);
        Debug.Log("vertices: " + string.Join(",", slicedTriVertexBuffer));

        for (int i = 0; i < expectedVertices.Length; ++i)
        {
            Assert.AreEqual(slicedTriVertexBuffer[i], expectedVertices[i]);
        }

        for (int i = 0; i < expectedTris.Length; ++i)
        {
            Assert.AreEqual(slicedTriBuffer[i], expectedTris[i]);
        }

        Assert.AreEqual(slicedTriangles.Buffers[0].Count, expectedTris.Length / 3);
        Assert.AreEqual(slicedTriangles.Buffers[1].Count, expectedVertices.Length / 8);

        slicer.Dispose();

        vertexBuffer.Dispose();
        vertexBuffer = null;
    }

    [Test]
    public void TestSingleTetNoIntersection()
    {
        RunTetTest(vertexDataNoIntersection, tetsForwardFacing, Array.Empty<float>(), Array.Empty<int>());
    }

    [Test]
    public void TestSingleTetTriangleIntersection()
    {
        float[] expectedTriVertices = {
            0, 0, 0, 1, 0, 0, 0, 0,
            1, 1, 1, 1, 0, 0, 0, 0,
            1, 0, 1, 1, 0, 0, 0, 0,
        };

        int[] expectedTris = { 0, 1, 2 };

        RunTetTest(vertexDataTriIntersection, tetsForwardFacing, expectedTriVertices, expectedTris);
    }

    [Test]
    public void TestSingleTetQuadIntersection()
    {
        float[] expectedTriVertices = {
            0, 0.5f, 0.5f, 1f, 0, 0, 0, 0,
            0.5f, 0.5f, 0.5f, 1f, 0, 0, 0, 0,
            0.5f, 1, 1, 1, 0, 0, 0, 0,
            0, 1, 1, 1, 0, 0, 0, 0,
        };

        int[] expectedTris = { 0, 1, 2, 0, 2, 3 };

        RunTetTest(vertexDataQuadIntersection, tetsForwardFacing, expectedTriVertices, expectedTris);
    }

    [Test]
    public void TestThreeTetsTwoIntersections()
    {
        TetMesh4D.VertexData[] vertexData = vertexDataNoIntersection.Concat(vertexDataTriIntersection).Concat(vertexDataQuadIntersection).ToArray();
        TetMesh4D.Tet4D[] tets = new TetMesh4D.Tet4D[] {
            new (new int[]{ 0, 1, 2, 3}),
            new (new int[]{ 4, 5, 6, 7}),
            new (new int[]{ 8, 9, 10, 11}),
        };
        float[] expectedTriVertices = {
            // Triangle vertices
            0, 0, 0, 1, 0, 0, 0, 0,
            1, 1, 1, 1, 0, 0, 0, 0,
            1, 0, 1, 1, 0, 0, 0, 0,
            // Quadrilateral vertices
            0, 0.5f, 0.5f, 1f, 0, 0, 0, 0,
            0.5f, 0.5f, 0.5f, 1f, 0, 0, 0, 0,
            0.5f, 1, 1, 1, 0, 0, 0, 0,
            0, 1, 1, 1, 0, 0, 0, 0,
        };

        int[] expectedTris = {
            0, 1, 2, // triangle
            3, 4, 5, 3, 5, 6  // quadrilateral
        };

        RunTetTest(vertexData, tets, expectedTriVertices, expectedTris);
    }

    [Test]
    public void TestSlice3Cube()
    {
        TetMesh_raw rawTetMesh = new();
        HypercubeGenerator.Generate3Cube(new(0, 0, 0, 0), Vector3.one,
            new(1, 0, 0, 0), new(0, 1, 0, 0), new(0, 0, 1, 0), rawTetMesh);

        Debug.Log("num tets: " + rawTetMesh.tets.Count());

        float[] expectedTriVertices = {
            0,0,0,1,0,0,0,0,
            1,0,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            0,0,0,1,0,0,0,0,
            0,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            0,1,0,1,0,0,0,0,
            0,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            1,0,0,1,0,0,0,0,
            1,0,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0
        };

        int[] expectedTris = {
            0,1,2,
            3,4,5,
            6,7,8,
            6,8,9,
            10,11,12,
            10,12,13,
            14,15,16,
            17,18,19
        };

        RunTetTest(rawTetMesh.vertices.ToArray(), rawTetMesh.tets.ToArray(), expectedTriVertices, expectedTris);
    }
}
