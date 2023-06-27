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

    List<float[]> vertexData;
    List<int[]> triangleData;

    // Appends the vertex data to the triangle mesh
    public void UpdateData(float[] newVertexData, int[] newTriangleData)
    {
        vertexData.Add(newVertexData);
        triangleData.Add(newTriangleData);
    }

    public void Render()
    {
        mesh.Clear();

        float[] vertexDataArr = vertexData.SelectMany(vertexArr => vertexArr).ToArray();
        int[] triangleDataArr = triangleData.SelectMany(triangleArr => triangleArr).ToArray();

        // Override vertex buffer params so that position, normal take in 4D vectors
        mesh.SetVertexBufferParams(
            vertexDataArr.Length / 4,
            new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, PTS_PER_TET),
                //new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, PTS_PER_TET),
            }
        );

        // Set vertices, normals for the mesh
        mesh.SetVertexBufferData(vertexDataArr, 0, 0, vertexDataArr.Length);

        // Set tetrahedra vertex indices for mesh
        mesh.SetTriangles(triangleDataArr, 0);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
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

    public void Reset()
    {
        vertexData = null;
        triangleData = null;
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
