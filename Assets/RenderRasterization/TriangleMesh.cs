using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static RasterizationRenderer.TetMesh4D;

public class TriangleMesh : MonoBehaviour
{
    // Stores 3D triangle points as well as depth data in w-coordinate
    public struct Triangle
    {
        public Vector4[] points;

        public Triangle(Vector4[] points) { this.points = points; }
    }

    public Mesh mesh = new();

    // Updates the mesh based on the vertices, tetrahedra
    public void UpdateMesh(Vector4[] vertices, Triangle[] tris)
    {
        mesh.Clear();

        // Override vertex buffer params so that position, normal take in 4D vectors
        mesh.SetVertexBufferParams(
            vertices.Length,
            new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, PTS_PER_TET),
                //new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, PTS_PER_TET),
            }
        );

        // Set vertices, normals for the mesh
        mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

        // Set tetrahedra vertex indices for mesh
        mesh.SetIndices(tris.SelectMany(tet => tet.points).ToArray(), MeshTopology.Quads, 0);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
