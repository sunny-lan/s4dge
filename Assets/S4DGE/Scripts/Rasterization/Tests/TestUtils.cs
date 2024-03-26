using NUnit.Framework;
using RasterizationRenderer;
using UnityEngine;

public class TestUtils
{
    public static void CopyPosToOtherFields(TetMesh4D.VertexData[] vertexData)
    {
        // make normals, worldPos equal to pos
        for (int i = 0; i < vertexData.Length; i++)
        {
            vertexData[i].normal = vertexData[i].position;
            vertexData[i].worldPosition4D = vertexData[i].position;
        }
    }

    public static ComputeShader LoadShader(string shaderName)
    {
        ComputeShader ret = null;
        foreach (var shader in Resources.FindObjectsOfTypeAll<ComputeShader>())
        {
            if (shader.name == shaderName)
            {
                ret = shader;
                break;
            }
        }
        Assert.IsNotNull(ret);

        return ret;
    }

    public static void AssertAlmostEqual(float expected, float actual)
    {
        Assert.Less(Mathf.Abs(expected - actual), 1e-2);
    }
}
