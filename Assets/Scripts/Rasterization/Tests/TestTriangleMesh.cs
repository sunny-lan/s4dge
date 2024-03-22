using NUnit.Framework;
using RasterizationRenderer;
using System.Collections;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestTriangleMesh
{
    TriangleMesh triMesh;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/RasterizerTests.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return null; // wait until scene finishes loading

        foreach (var r in Object.FindObjectsOfType<TriangleMesh>())
        {
            triMesh = r;
            break;
        }
        Assert.IsNotNull(triMesh);
        triMesh.Reset();
    }

    // triangle mesh should automatically adjust the tet indices for subsequent calls to UpdateData()
    int[] expectedIndices2 = { 1, 1, 1 };

    [UnityTearDown]
    public void TearDown()
    {
    }

    [Test]
    public void TestUpdateData()
    {
        float[] vertexData1 = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        float[] vertexData2 = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        int[] indices1 = { 0, 0, 0 };
        int[] indices2 = { 0, 0, 0 };

        triMesh.UpdateData(vertexData1, indices1);
        Assert.That(Enumerable.SequenceEqual(triMesh.tMesh.triangles, indices1));
        GraphicsBuffer vertexBuf = triMesh.tMesh.GetVertexBuffer(0);
        float[] vertexDataActual1 = new float[vertexBuf.count * TetMesh4D.VertexData.SizeFloats];
        vertexBuf.GetData(vertexDataActual1);
        Assert.That(Enumerable.SequenceEqual(vertexData1, vertexDataActual1));
        Assert.That(Enumerable.SequenceEqual(indices1, triMesh.tMesh.triangles));

        triMesh.UpdateData(vertexData2, indices2);
        GraphicsBuffer vertexBuf2 = triMesh.tMesh.GetVertexBuffer(0);
        float[] vertexDataActual2 = new float[vertexBuf2.count * TetMesh4D.VertexData.SizeFloats];
        vertexBuf2.GetData(vertexDataActual2);
        Debug.Log("Actual vertices: " + string.Join(", ", vertexDataActual2));
        Debug.Log("Actual triangles: " + string.Join(", ", triMesh.tMesh.triangles));
        Assert.That(Enumerable.SequenceEqual(vertexData1.Concat(vertexData2).ToArray(), vertexDataActual2));
        Assert.That(Enumerable.SequenceEqual(indices1.Concat(expectedIndices2).ToArray(), triMesh.tMesh.triangles));
    }

    [Test]
    public void TestRenderToRenderTexture()
    {
        float[] vertexData = {
            0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1,
            1, 1, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1,
            -1, 1, 0, 1, 0, 0, 0, 1, -1, 1, 0, 1
        };
        int[] indices = { 0, 1, 2 };

        triMesh.UpdateData(vertexData, indices);

        // try rendering to the tri mesh and make sure something renders
        Color clearColour = Color.clear;
        RenderTexture rt = new(100, 100, 0);
        triMesh.RenderToRenderTexture(rt, clearColour);

        Texture2D tex = RenderUtils.Texture2DFromRenderTexture(rt, rt.width, rt.height);
        int nonClearPixelCount = 0;
        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                Color col = tex.GetPixel(i, j, 0);
                if (col != clearColour)
                {
                    ++nonClearPixelCount;
                }
            }
        }

        Assert.IsTrue(nonClearPixelCount > 10);
    }
}

