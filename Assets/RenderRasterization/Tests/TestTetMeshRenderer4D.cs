using NUnit.Framework;
using NUnit.Framework.Internal;
using RasterizationRenderer;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestTetMeshRenderer4D
{
    TetMeshRenderer4D renderer;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/RasterizationTestScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return null; // wait until scene finishes loading

        foreach (var r in Resources.FindObjectsOfTypeAll<TetMeshRenderer4D>())
        {
            renderer = r;
            break;
        }
        Assert.IsNotNull(renderer);
    }

    [UnityTearDown]
    public void TearDown()
    {
    }

    void AssertAlmostEqual(float expected, float actual)
    {
        Assert.Less(Mathf.Abs(expected - actual), 1e-3);
    }

    void RunMeshRendererTest(TetMesh4D mesh, float zSlice, float vanishingW, float nearW, int[] expectedTris, float[] expectedVertices)
    {
        renderer.SetTetMesh(mesh);
        var (vertexBuffer, tetDrawBuffer, numTetsBuffer) = renderer.TransformAndCullVertices();
        (int[] triangleData, float[] vertexData) = renderer.GenerateTriangleMesh(zSlice, vertexBuffer, tetDrawBuffer, numTetsBuffer);
        Debug.Log("triangles: " + string.Join(",", triangleData));
        Debug.Log("vertices: " + string.Join(",", vertexData));


        for (int i = 0; i < expectedVertices.Length; ++i)
        {
            Assert.AreEqual(expectedVertices[i], expectedVertices[i]);
        }

        for (int i = 0; i < expectedTris.Length; ++i)
        {
            Assert.AreEqual(triangleData[i], expectedTris[i]);
        }

        Assert.AreEqual(triangleData.Length, expectedTris.Length);
        Assert.AreEqual(vertexData.Length, expectedVertices.Length);
    }

    [Test]
    public void TestRender3CubeWithCuller()
    {
        renderer.useCuller = true;

        TetMesh_raw rawTetMesh = new();
        HypercubeGenerator.Generate3Cube(new(0, 0, 0, 0), Vector3.one,
            new(1, 0, 0, 0), new(0, 1, 0, 0), new(0, 0, 1, 0), rawTetMesh);

        // triangles left after culling
        int[] expectedTris = {
            0,1,2,
            3,4,5,
            6,7,8,
            6,8,9
        };

        float[] expectedVertices =
        {
            0,0,0,1,0,0,0,0,
            1,0,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            0,0,0,1,0,0,0,0,
            0,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            0,1,0,1,0,0,0,0,
            0,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0,
            1,1,0,1,0,0,0,0
        };

        RunMeshRendererTest(
            mesh: rawTetMesh.ToRasterizableTetMesh(),
            zSlice: 0,
            vanishingW: 1e6f,
            nearW: 1,
            expectedTris: expectedTris,
            expectedVertices: expectedVertices
        );
    }

    [Test]
    public void TestRender3CubeWithoutCuller()
    {
        renderer.useCuller = false;

        TetMesh_raw rawTetMesh = new();
        HypercubeGenerator.Generate3Cube(new(0, 0, 0, 0), Vector3.one,
            new(1, 0, 0, 0), new(0, 1, 0, 0), new(0, 0, 1, 0), rawTetMesh);

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

        float[] expectedVertices =
        {
            0,0,0,1,0,0,0,0,
            3,0,0,1,0,0,0,0,
            3,3,0,1,0,0,0,0,
            0,0,0,1,0,0,0,0,
            0,3,0,1,0,0,0,0,
            3,3,0,1,0,0,0,0,
            0,3,0,1,0,0,0,0,
            0,3,0,1,0,0,0,0,
            3,3,0,1,0,0,0,0,
            3,3,0,1,0,0,0,0,
            3,0,0,1,0,0,0,0,
            3,0,0,1,0,0,0,0,
            3,3,0,1,0,0,0,0,
            3,3,0,1,0,0,0,0,
            3,3,0,1,0,0,0,0,
            3,3,0,1,0,0,0,0,
            3,3,0,1,0,0,0,0,
            3,3,0,1,0,0,0,0,
            3,3,0,1,0,0,0,0,
            3,3,0,1,0,0,0,0
        };

        RunMeshRendererTest(
            mesh: rawTetMesh.ToRasterizableTetMesh(),
            zSlice: 0,
            vanishingW: 1e6f,
            nearW: 1,
            expectedTris: expectedTris,
            expectedVertices: expectedVertices
        );
    }
}
