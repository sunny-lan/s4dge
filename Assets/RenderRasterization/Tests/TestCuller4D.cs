using NUnit.Framework;
using RasterizationRenderer;
using System.Collections;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

[TestFixture]
public class TestCuller4D
{

    ComputeShader cullShader;
    ComputeBuffer vertexBuffer;

    TetMesh4D.VertexData[] vertexData = new TetMesh4D.VertexData[]
    {
        new TetMesh4D.VertexData(new Vector4(0.0f, 0.0f, 0.0f, 0.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
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
            if (shader.name == "Culler4D")
            {
                cullShader = shader;
                break;
            }
        }
        Assert.IsNotNull(cullShader);

        vertexBuffer = RenderUtils.InitComputeBuffer(TetMesh4D.VertexData.SizeBytes, vertexData);
    }

    [UnityTearDown]
    public void TearDown()
    {
        vertexBuffer.Dispose();
        vertexBuffer = null;
    }

    [Test]
    public void TestCuller4DForwardFacing()
    {
        var tetrahedraUnpacked = tetsForwardFacing.SelectMany(tet => tet.tetPoints).ToArray();
        Culler4D culler = new(cullShader, tetsForwardFacing);
        var culledTets = culler.Render(vertexBuffer);

        int[] culledTetBuffer = new int[4];
        culledTets.Buffer.GetData(culledTetBuffer);

        for (int i = 0; i < 4; ++i)
        {
            Assert.AreEqual(culledTetBuffer[i], tetrahedraUnpacked[i]);
        }

        Assert.AreEqual(culledTets.Length, 1);

        culler.OnDisable();
    }
}
