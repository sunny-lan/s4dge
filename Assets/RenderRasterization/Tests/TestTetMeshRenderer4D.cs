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

    [Test]
    public void TestRenderHypercube()
    {
        renderer.SetTetMesh(HypercubeGenerator.GenerateHypercube());
        float zSlice = 0;
        float vanishingW = 1e6f;
        float nearW = 1;
        (int[] triangleData, float[] vertexData) = renderer.GenerateTriangleMesh(zSlice, vanishingW, nearW);

        int[] expectedTris = new int[6 * 3];
        float[] expectedVertices = new float[6 * 4];

        //for (int i = 0; i < expectedVertices.Length; ++i)
        //{
        //    Assert.AreEqual(expectedVertices[i], expectedVertices[i]);
        //}

        //for (int i = 0; i < expectedTris.Length; ++i)
        //{
        //    Assert.AreEqual(triangleData[i], expectedTris[i]);
        //}

        Assert.AreEqual(triangleData.Length, expectedTris.Length);
        Assert.AreEqual(vertexData.Length, expectedVertices.Length);
    }
}
