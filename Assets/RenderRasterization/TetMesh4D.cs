using UnityEngine;

namespace RasterizationRenderer
{

    public class TetMesh4D : MonoBehaviour
    {
        public Transform4D modelWorldTransform4D = new();
        public Matrix4x4 modelViewProjection3D = Matrix4x4.identity;
        public static readonly int PTS_PER_TET = 4;

        VertexShader vertexShader;
        VertexData[] vertices;

        Culler4D culler;
        Tet4D[] tets;

        TetSlicer tetSlicer;

        public TriangleMesh triangleMesh;

        // Make sure struct in passed in correct layout to the mesh vertex buffer
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct VertexData
        {
            public Vector4 position;
            public Vector4 normal;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct Tet4D
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
            //mesh.Clear();

            //// Override vertex buffer params so that position, normal take in 4D vectors
            //mesh.SetVertexBufferParams(
            //    vertices.Length,
            //    new[]
            //    {
            //    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, PTS_PER_TET),
            //    new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, PTS_PER_TET),
            //    }
            //);

            //// Set vertices, normals for the mesh
            //mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

            //// Set tetrahedra vertex indices for mesh
            //mesh.SetIndices(tets.SelectMany(tet => tet.tetPoints).ToArray(), MeshTopology.Quads, 0);

            this.vertices = vertices;
            vertexShader = new(this.vertices);

            this.tets = tets;
            culler = new(this.tets);
        }

        // Generate triangle mesh
        public void Render()
        {
            ComputeBuffer vertexBuffer = vertexShader.Render(modelWorldTransform4D.rotation, modelWorldTransform4D.translation, 0.0f, 1.0f, 0.0f);
            VariableLengthComputeBuffer tetrahedraToDraw = culler.Render(vertexBuffer);
            VariableLengthComputeBuffer.BufferList trianglesToDraw = new TetSlicer(tetrahedraToDraw.Buffer, tetrahedraToDraw.Length).Render(vertexBuffer);

            VariableLengthComputeBuffer triangleBuffer = trianglesToDraw.Buffers[0];
            VariableLengthComputeBuffer triangleVertexBuffer = trianglesToDraw.Buffers[1];

            TriangleMesh.Triangle[] triangles = new TriangleMesh.Triangle[triangleBuffer.Length];
            Vector4[] triangleVertices = new Vector4[triangleVertexBuffer.Length];
            triangleBuffer.Buffer.GetData(triangleVertices);
            triangleVertexBuffer.Buffer.GetData(triangles);

            triangleMesh.Render(triangleVertices, triangles);
        }

        private void OnEnable()
        {
            vertexShader.OnEnable();
            culler.OnEnable();
        }

        private void OnDisable()
        {
            vertexShader.OnDisable();
            culler.OnDisable();
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

}

