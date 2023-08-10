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
        new TetMesh4D.VertexData(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
    };

    static TetMesh4D.Tet4D tetForwardFacing = new(new int[] { 0, 1, 2, 3 });
    static TetMesh4D.Tet4D tetBackwardFacing = new(new int[] { 0, 3, 2, 1 });
    static TetMesh4D.Tet4D tetDegenerate = new(new int[] { 0, 4, 5, 6 });
    TetMesh4D.Tet4D[] tets = new TetMesh4D.Tet4D[] { tetForwardFacing, tetDegenerate, tetBackwardFacing, tetForwardFacing };

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/RasterizerTests.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return null; // wait until scene finishes loading

        cullShader = TestUtils.LoadShader("Culler4D");

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
        var tetrahedraUnpacked = tets.SelectMany(tet => tet.tetPoints).ToArray();
        Culler4D culler = new(cullShader, tets);
        var culledTets = culler.Render(vertexBuffer);
        culledTets.UpdateBufferLengths();

        int[] culledTetBuffer = new int[4 * tets.Length];
        culledTets.Buffers[0].Buffer.GetData(culledTetBuffer);

        int[] expectedTetVertices = { 0, 1, 2, 3, 0, 1, 2, 3 };
        for (int i = 0; i < expectedTetVertices.Length; ++i)
        {
            Assert.AreEqual(culledTetBuffer[i], expectedTetVertices[i]);
        }

        Assert.AreEqual(culledTets.Buffers[0].Count, 2);

        culler.OnDisable();
    }
}
