using UnityEngine;
using UnityEngine.Rendering;
using v2;
using static RasterizationRenderer.TetMesh4D;

public class TriangleMesh : MonoBehaviour
{
    // Stores 3D triangle points as well as depth data in w-coordinate
    public struct Triangle
    {
        public int[] points;

        public Triangle(int[] points) { this.points = points; }
    }

    Mesh mesh;
    public Material material;
    int curVertexCount = 0;
    public void UpdateData(float[] newVertexData, int[] newTriangleData)
    {
        int numNewVertices = newVertexData.Length / VertexData.SizeFloats;
        int curSubmesh = mesh.subMeshCount;
        ++mesh.subMeshCount;

        // Override vertex buffer params so that position, normal take in 4D vectors
        mesh.SetVertexBufferParams(
            curVertexCount + numNewVertices,
            new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 4),
            }
        );

        // Set vertices, normals for the mesh
        mesh.SetVertexBufferData(newVertexData, 0, curVertexCount * VertexData.SizeFloats, newVertexData.Length);

        // Set tetrahedra vertex indices for mesh
        mesh.SetTriangles(newTriangleData, curSubmesh, false, curVertexCount);

        curVertexCount += numNewVertices;
    }

    public void Render(LightSource4DManager lightSources, TransformMatrixAffine4D worldToCameraTransform, float farClipPlane, float nearClipPlane)
    {
        PassLightDataToMaterial(lightSources, farClipPlane, nearClipPlane);
        material.SetMatrix("worldToCameraScaleAndRot", worldToCameraTransform.scaleAndRot);
        material.SetVector("worldToCameraTranslation", worldToCameraTransform.translation);

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        for (int i = 0; i < mesh.subMeshCount; ++i)
        {
            Graphics.DrawMesh(
                mesh: mesh,
                matrix: Matrix4x4.identity,
                material: material,
                layer: gameObject.layer,
                camera: Camera4D.main.camera3D,
                submeshIndex: i,
                properties: null
            );
        }
    }

    public void RenderToRenderTexture(RenderTexture rt, Color clearColour, Material overrideMaterial = null, bool clearTexture = true)
    {
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        Matrix4x4 projectionMatrix = Camera4D.main.camera3D.projectionMatrix;
        RenderTexture prevRT = RenderTexture.active;
        RenderTexture.active = rt;

        Material renderMaterial = material;
        if (overrideMaterial != null)
        {
            renderMaterial = overrideMaterial;
        }
        renderMaterial.SetPass(0);

        GL.PushMatrix();
        GL.LoadProjectionMatrix(projectionMatrix);
        if (clearTexture)
        {
            GL.Clear(true, true, clearColour);
        }
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            Graphics.DrawMeshNow(mesh, Matrix4x4.identity, i);
        }
        GL.PopMatrix();
        RenderTexture.active = prevRT;
    }

    public void RenderToRenderTextureCmdBuf(RenderTexture rt, Color clearColour)
    {
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        Matrix4x4 projectionMatrix = Camera.main.projectionMatrix;
        CommandBuffer cmdBuf = new();
        cmdBuf.Clear();
        cmdBuf.SetRenderTarget(rt);
        cmdBuf.SetProjectionMatrix(projectionMatrix);
        cmdBuf.ClearRenderTarget(true, true, clearColour);
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            cmdBuf.DrawMesh(mesh, Matrix4x4.identity, material, i);
        }
        Graphics.ExecuteCommandBuffer(cmdBuf);
    }

    public void PassLightDataToMaterial(LightSource4DManager lightSources, float farClipPlane, float nearClipPlane)
    {
        lightSources.UpdateComputeBuffer();
        material.SetBuffer("lightSources", lightSources.LightSourceBuffer);
        material.SetInt("numLights", lightSources.Count);
        material.SetFloat("vanishingW", farClipPlane);
        material.SetFloat("nearW", nearClipPlane);
        material.SetMatrix("modelViewProjection3D", Matrix4x4.identity);

        RenderTexture shadowMap = lightSources.LightSources[0].ShadowMap;
        material.SetTexture("_ShadowMap", shadowMap, RenderTextureSubElement.Color);
    }

    public void Reset()
    {
        mesh.Clear();
        curVertexCount = 0;
        mesh.subMeshCount = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh = new();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
