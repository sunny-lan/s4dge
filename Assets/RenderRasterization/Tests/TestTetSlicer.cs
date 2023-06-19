using NUnit.Framework;
using RasterizationRenderer;
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

    static TetMesh4D.Tet4D tetForwardFacing = new(new int[] { 0, 1, 2, 3 });
    TetMesh4D.Tet4D[] tets = new TetMesh4D.Tet4D[] { tetForwardFacing };

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
        vertexBuffer.Dispose();
        vertexBuffer = null;
    }

    [Test]
    public void TestSingleTetNoIntersection()
    {
        vertexBuffer = RenderUtils.InitComputeBuffer(TetMesh4D.VertexData.SizeBytes, vertexDataNoIntersection);
        var tetrahedraUnpacked = tets.SelectMany(tet => tet.tetPoints).ToArray();
        tetBuffer = RenderUtils.InitComputeBuffer(sizeof(int) * TetMesh4D.PTS_PER_TET, tetrahedraUnpacked);
        TetSlicer slicer = new(sliceShader, tetBuffer, tets.Length);
        var slicedTriangles = slicer.Render(vertexBuffer);

        int[] slicedTriBuffer = new int[3 * tets.Length];
        int[] slicedTriVertexBuffer = new int[4 * tets.Length];
        slicedTriangles.Buffers[0].Buffer.GetData(slicedTriBuffer);
        slicedTriangles.Buffers[1].Buffer.GetData(slicedTriVertexBuffer);

        Assert.AreEqual(slicedTriangles.Buffers[0].Count, 0);
        Assert.AreEqual(slicedTriangles.Buffers[1].Count, 0);

        slicer.Dispose();
    }

    [Test]
    public void TestSingleTetTriangleIntersection()
    {
        vertexBuffer = RenderUtils.InitComputeBuffer(TetMesh4D.VertexData.SizeBytes, vertexDataTriIntersection);
        var tetrahedraUnpacked = tets.SelectMany(tet => tet.tetPoints).ToArray();
        tetBuffer = RenderUtils.InitComputeBuffer(sizeof(int) * TetMesh4D.PTS_PER_TET, tetrahedraUnpacked);
        TetSlicer slicer = new(sliceShader, tetBuffer, tets.Length);
        var slicedTriangles = slicer.Render(vertexBuffer);

        int[] slicedTriBuffer = new int[3 * tets.Length];
        float[] slicedTriVertexBuffer = new float[4 * 4 * tets.Length];
        slicedTriangles.Buffers[0].Buffer.GetData(slicedTriBuffer);
        slicedTriangles.Buffers[1].Buffer.GetData(slicedTriVertexBuffer);

        Debug.Log("triangles: " + string.Join(" ", slicedTriBuffer));
        Debug.Log("triangle count: " + slicedTriangles.Buffers[0].Count);
        Debug.Log("vertices: " + string.Join(" ", slicedTriVertexBuffer));
        Debug.Log("vertex count: " + slicedTriangles.Buffers[1].Count);

        //int[] expectedTriVertices = { 0, 1, 2, 3, 0, 1, 2, 3 };
        //for (int i = 0; i < expectedTetVertices.Length; ++i)
        //{
        //    Assert.AreEqual(culledTetBuffer[i], expectedTetVertices[i]);
        //}

        //Assert.AreEqual(slicedTriangles.Buffers[0].Count, 0);
        //Assert.AreEqual(slicedTriangles.Buffers[1].Count, 0);

        slicer.Dispose();
    }
}
