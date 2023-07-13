using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
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

    List<float> vertexData = new();
    List<int> triangleData = new();

    // Appends the vertex data to the triangle mesh
    public void UpdateData(float[] newVertexData, int[] newTriangleData)
    {
        int floatsPerVertex = VertexData.SizeFloats;
        int curVertexCount = vertexData.Count / floatsPerVertex;
        triangleData = triangleData.Concat(newTriangleData.Select(idx => idx + curVertexCount)).ToList();

        vertexData = vertexData.Concat(newVertexData).ToList();
    }

    public void Render(LightSource4DManager lightSources)
    {
        PassLightDataToMaterial(lightSources);

        mesh.Clear();

        // Override vertex buffer params so that position, normal take in 4D vectors
        mesh.SetVertexBufferParams(
            vertexData.Count / VertexData.SizeFloats,
            new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 4),
            }
        );

        // Set vertices, normals for the mesh
        mesh.SetVertexBufferData(vertexData, 0, 0, vertexData.Count);

        // Set tetrahedra vertex indices for mesh
        mesh.SetTriangles(triangleData, 0, false);

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        Graphics.DrawMesh(
            mesh: mesh,
            matrix: Matrix4x4.identity,
            material: material,
            layer: gameObject.layer,
            //camera: GetComponent<Camera>(),
            camera: null,
            submeshIndex: 0,
            properties: null
        //properties: blk
        );
    }

    public void PassLightDataToMaterial(LightSource4DManager lightSources)
    {
        material.SetBuffer("lightSources", lightSources.LightSourceBuffer);
        material.SetInt("numLights", lightSources.Count);
    }

    public void Reset()
    {
        vertexData = new();
        triangleData = new();
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
