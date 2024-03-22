using NUnit.Framework;
using RasterizationRenderer;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RasterizationRenderer
{
    public class TestLightSource4D
    {
        LightSource4D lightSource;

        // A Test behaves as an ordinary method
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/RasterizerTests.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; // wait until scene finishes loading

            foreach (var r in Object.FindObjectsOfType<LightSource4D>())
            {
                lightSource = r;
                break;
            }
            Assert.IsNotNull(lightSource);
        }

        [UnityTearDown]
        public void TearDown()
        {

        }

        [Test]
        public void TestUpdateShadowMapDisabledShadows()
        {
            lightSource.UpdateShadowMap(false);
            RenderTexture rt = lightSource.ShadowMap;
            Texture2D tex = RenderUtils.Texture2DFromRenderTexture(rt, rt.width, rt.height);
            for (int i = 0; i < tex.width; i++)
            {
                for (int j = 0; j < tex.height; j++)
                {
                    Color col = tex.GetPixel(i, j, 0);
                    Assert.IsTrue(col == Color.clear);
                }
            }
        }

        [Test]
        public void TestUpdateShadowMapEnabledShadows()
        {
            // call update shadow map twice as the returned shadow map is one frame late
            lightSource.UpdateShadowMap(true);
            lightSource.UpdateShadowMap(true);

            RenderTexture rt = lightSource.ShadowMap;
            Texture2D tex = RenderUtils.Texture2DFromRenderTexture(rt, rt.width, rt.height);

            Color clearColour = Color.clear;
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

            Assert.IsTrue(nonClearPixelCount > 100);
        }
    }
}