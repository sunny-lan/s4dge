using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class TetMesh4D : MonoBehaviour
{
    public Mesh mesh = new();

    // Make sure struct in passed in correct layout to the mesh vertex buffer
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct VertexData
    {
        public Vector4 position;
        public Vector4 normal;
    }

    public class Tet4D
    {
        public int[] tetPoints; // points to indices in Vector4 points array

        public Tet4D(int[] tetPoints)
        {
            this.tetPoints = tetPoints;
        }
    }

    // Updates the mesh based on the vertices, tetrahedra
    public void UpdateMesh(VertexData[] vertices, Tet4D[] tets)
    {
        mesh.Clear();

        // Override vertex buffer params so that position, normal take in 4D vectors
        mesh.SetVertexBufferParams(
            vertices.Length,
            new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 4),
            }
        );

        // Set vertices, normals for the mesh
        mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

        // Set tetrahedra vertex indices for mesh
        mesh.SetIndices(tets.SelectMany(tet => tet.tetPoints).ToArray(), MeshTopology.Quads, 0);
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
