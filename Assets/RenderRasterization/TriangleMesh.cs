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

    float[] vertexDataArr = new float[0];
    int[] triangleDataArr = new int[0];

    // Appends the vertex data to the triangle mesh
    public void UpdateData(float[] newVertexData, int[] newTriangleData)
    {
        int floatsPerVertex = 4;
        int curVertexCount = vertexDataArr.Length / floatsPerVertex;
        triangleDataArr = triangleDataArr.Concat(newTriangleData.Select(idx => idx + curVertexCount)).ToArray();

        vertexDataArr = vertexDataArr.Concat(newVertexData).ToArray();
    }

    public void Render()
    {
        mesh.Clear();

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
        vertexDataArr = new float[0];
        triangleDataArr = new int[0];
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
