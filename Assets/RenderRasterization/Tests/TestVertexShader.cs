using NUnit.Framework;
using NUnit.Framework.Internal;
using RasterizationRenderer;
using System.Collections;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using v2;

public class TestVertexShader
{
    ComputeShader vertexShader;
    ComputeBuffer vertexBuffer;

    TetMesh4D.VertexData[] vertexData = new TetMesh4D.VertexData[]
    {
        new TetMesh4D.VertexData(new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
        new TetMesh4D.VertexData(new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
        new TetMesh4D.VertexData(new Vector4(1.0f, 0.0f, 1.0f, 1.0f)),
        new TetMesh4D.VertexData(new Vector4(0.0f, 1.0f, 1.0f, 1.0f)),
        new TetMesh4D.VertexData(new Vector4(0.0f, 0.0f, 0.0f, 1.0f)),
        new TetMesh4D.VertexData(new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
        new TetMesh4D.VertexData(new Vector4(0.0f, 1.0f, 0.0f, 1.0f)),
    };

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/RasterizerTests.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return null; // wait until scene finishes loading

        vertexShader = TestUtils.LoadShader("VertexShader");

        // make normals, worldPos equal to pos
        TestUtils.CopyPosToOtherFields(vertexData);

        vertexBuffer = RenderUtils.InitComputeBuffer(TetMesh4D.VertexData.SizeBytes, vertexData);
    }

    [UnityTearDown]
    public void TearDown()
    {
        vertexBuffer.Dispose();
        vertexBuffer = null;
    }

    public void PerformIdentityTransformTest(TetMesh4D.VertexData[] vertices)
    {
        VertexShader vertexTransformer = new(vertexShader, vertices);
        float inf = 1e6f;
        TransformMatrixAffine4D transform = new()
        {
            scaleAndRot = Matrix4x4.identity,
            translation = Vector4.zero,
        };
        ComputeBuffer transformedVertexBuffer = vertexTransformer.Render(transform, transform, Matrix4x4.identity, inf, 0.0f);

        float[] transformedVertexData = new float[vertices.Length * TetMesh4D.VertexData.SizeFloats];
        transformedVertexBuffer.GetData(transformedVertexData);

        Debug.Log(string.Join("\n", RenderUtils.ParseRawMeshVertices(transformedVertexData)));

        float[] expectedVertexData = vertices.SelectMany(vert => new float[] {
            vert.position.x,
            vert.position.y,
            vert.position.z,
            vert.position.w,
            vert.normal.x,
            vert.normal.y,
            vert.normal.z,
            vert.normal.w,
            vert.position.x,
            vert.position.y,
            vert.position.z,
            vert.position.w,
        }).ToArray();

        Assert.AreEqual(transformedVertexData.Length, expectedVertexData.Length);
        for (int i = 0; i < expectedVertexData.Length; ++i)
        {
            Assert.True(!double.IsNaN(transformedVertexData[i]));
            TestUtils.AssertAlmostEqual(expectedVertexData[i], transformedVertexData[i]);
        }

        vertexTransformer.OnDisable();
    }

    [Test]
    public void TestIdentityTransform()
    {
        PerformIdentityTransformTest(vertexData);
    }

    [Test]
    public void Test3CubeIdentityTransform()
    {
        TetMesh_raw rawTetMesh = new();
        HypercubeGenerator.Generate3Cube(new(0, 0, 0, 0), Vector3.one,
            new(1, 0, 0, 0), new(0, 1, 0, 0), new(0, 0, 1, 0), rawTetMesh);
        TetMesh4D.VertexData[] vertices = rawTetMesh.vertices.ToArray();

        PerformIdentityTransformTest(vertices);
    }
}
