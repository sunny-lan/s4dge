using System;
using UnityEngine;

namespace RasterizationRenderer
{

    public class TetMesh4D : MonoBehaviour
    {
        public Transform4D modelWorldTransform4D = new();
        public Matrix4x4 modelViewProjection3D = Matrix4x4.identity;
        public static readonly int PTS_PER_TET = 4;

        [SerializeField]
        public ComputeShader vertexShaderProgram;
        [SerializeField]
        public ComputeShader cullShaderProgram;
        [SerializeField]
        public ComputeShader sliceShaderProgram;
        VertexShader vertexShader;
        public VertexData[] vertices
        {
            get; private set;
        }

        Culler4D culler;

        public Tet4D[] tets
        {
            get; private set;
        }

        TetSlicer tetSlicer;

        public TriangleMesh triangleMesh;

        // Make sure struct in passed in correct layout to the mesh vertex buffer
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        [Serializable]
        public struct VertexData
        {
            [SerializeField]
            public Vector4 position;

            [SerializeField]
            public Vector4 normal;

            public VertexData(Vector4 position, Vector4 normal)
            {
                this.position = position;
                this.normal = normal;
            }

            public static int SizeBytes
            {
                get => sizeof(float) * 8;
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        [Serializable]
        public struct Tet4D
        {
            [SerializeField]
            public int[] tetPoints; // points to indices in Vector4 points array

            public Tet4D(int[] tetPoints)
            {
                this.tetPoints = tetPoints;
            }

            public override string ToString()
            {
                return "(" + string.Join(",", tetPoints) + ")";
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

            if (tets.Length > 0 && vertices.Length > 0)
            {
                this.vertices = vertices;
                vertexShader = new(vertexShaderProgram, this.vertices);

                this.tets = tets;
                culler = new(cullShaderProgram, this.tets);
            }
        }

        // Generate triangle mesh
        public void Render(float zSlice, float vanishingW, float nearW)
        {
            if (vertexShader != null && culler != null)
            {
                ComputeBuffer vertexBuffer = vertexShader.Render(modelWorldTransform4D.rotation, modelWorldTransform4D.translation, zSlice, vanishingW, nearW);
                VariableLengthComputeBuffer tetrahedraToDraw = culler.Render(vertexBuffer);
                if (tetrahedraToDraw.Count > 0)
                {
                    var tetSlicer = new TetSlicer(sliceShaderProgram, tetrahedraToDraw.Buffer, tetrahedraToDraw.Count);
                    VariableLengthComputeBuffer.BufferList trianglesToDraw = tetSlicer.Render(vertexBuffer);

                    VariableLengthComputeBuffer triangleBuffer = trianglesToDraw.Buffers[0];
                    VariableLengthComputeBuffer triangleVertexBuffer = trianglesToDraw.Buffers[1];

                    int[] triangleData = new int[triangleBuffer.Count * TetSlicer.PTS_PER_TRIANGLE];
                    float[] triangleVertexData = new float[triangleVertexBuffer.Count * 4];
                    triangleBuffer.Buffer.GetData(triangleData);
                    triangleVertexBuffer.Buffer.GetData(triangleVertexData);

                    triangleMesh.Render(triangleVertexData, triangleData);

                    tetSlicer.Dispose();
                }
            }
        }

        private void OnEnable()
        {
            if (vertexShader != null)
            {
                vertexShader.OnEnable();
            }
            if (culler != null)
            {
                culler.OnEnable();
            }
        }

        private void OnDisable()
        {
            if (vertexShader != null)
            {
                vertexShader.OnDisable();
            }
            if (culler != null)
            {
                culler.OnDisable();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            OnDisable();
        }
    }

}

