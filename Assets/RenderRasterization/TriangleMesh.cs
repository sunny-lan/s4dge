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

    // Updates the mesh based on the vertices, tetrahedra
    public void Render(float[] vertexData, int[] triangleData)
    {
        mesh.Clear();

        // Override vertex buffer params so that position, normal take in 4D vectors
        mesh.SetVertexBufferParams(
            vertexData.Length / 4,
            new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, PTS_PER_TET),
                //new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, PTS_PER_TET),
            }
        );

        // Set vertices, normals for the mesh
        mesh.SetVertexBufferData(vertexData, 0, 0, vertexData.Length);

        // Set tetrahedra vertex indices for mesh
        mesh.SetTriangles(triangleData, 0);

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
