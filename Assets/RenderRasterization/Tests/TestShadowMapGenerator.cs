using NUnit.Framework;
using RasterizationRenderer;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestShadowMapGenerator
{
    ShadowMapGenerator mapGenerator;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/RasterizerTests.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return null; // wait until scene finishes loading

        foreach (var r in Object.FindObjectsOfType<ShadowMapGenerator>())
        {
            mapGenerator = r;
            break;
        }
        Assert.IsNotNull(mapGenerator);
    }

    [UnityTearDown]
    public void TearDown()
    {

    }

    [Test]
    public void TestGetClearedShadowMap()
    {
        RenderTexture rt = mapGenerator.GetClearedShadowMap(Color.red);
        Texture2D tex = RenderUtils.Texture2DFromRenderTexture(rt, rt.width, rt.height);
        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                Color col = tex.GetPixel(i, j, 0);
                Assert.IsTrue(col == Color.red);
            }
        }
    }

}
