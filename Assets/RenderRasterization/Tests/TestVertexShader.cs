using NUnit.Framework;
using NUnit.Framework.Internal;
using RasterizationRenderer;
using System.Collections;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestVertexShader
{
    ComputeShader vertexShader;
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

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/RasterizationTestScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return null; // wait until scene finishes loading

        foreach (var shader in Resources.FindObjectsOfTypeAll<ComputeShader>())
        {
            if (shader.name == "VertexShader")
            {
                vertexShader = shader;
                break;
            }
        }
        Assert.IsNotNull(vertexShader);

        vertexBuffer = RenderUtils.InitComputeBuffer(TetMesh4D.VertexData.SizeBytes, vertexData);
    }

    [UnityTearDown]
    public void TearDown()
    {
        vertexBuffer.Dispose();
        vertexBuffer = null;
    }

    void AssertAlmostEqual(float expected, float actual)
    {
        Assert.Less(Mathf.Abs(expected - actual), 1e-3);
    }

    [Test]
    public void TestIdentityTransform()
    {
        VertexShader vertexTransformer = new(vertexShader, vertexData);
        float inf = 1e6f;
        ComputeBuffer transformedVertexBuffer = vertexTransformer.Render(Matrix4x4.identity, Vector4.zero, 0, inf, 0.0f);

        float[] transformedVertexData = new float[vertexData.Length * 8];
        transformedVertexBuffer.GetData(transformedVertexData);

        float[] expectedVertexData = vertexData.SelectMany(vert => new float[] {
            vert.position.x,
            vert.position.y,
            vert.position.z,
            vert.position.w,
            vert.normal.x,
            vert.normal.y,
            vert.normal.z,
            vert.normal.w,
        }).ToArray();

        Assert.AreEqual(transformedVertexData.Length, expectedVertexData.Length);
        for (int i = 0; i < expectedVertexData.Length; ++i)
        {
            Assert.True(!double.IsNaN(transformedVertexData[i]));
            //Assert.AreEqual(expectedVertexData[i], transformedVertexData[i]);
            AssertAlmostEqual(expectedVertexData[i], transformedVertexData[i]);
        }

        vertexTransformer.OnDisable();
    }
}
