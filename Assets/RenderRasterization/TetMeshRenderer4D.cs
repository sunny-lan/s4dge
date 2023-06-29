using System.Linq;
using UnityEngine;

namespace RasterizationRenderer
{

    public class TetMeshRenderer4D : MonoBehaviour
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

        Culler4D culler;

        public TriangleMesh triangleMesh;
        private TetMesh4D tetMesh;

        public void SetTetMesh(TetMesh4D tetMesh)
        {
            if (tetMesh.vertices.Length > 0 && tetMesh.tets.Length > 0)
            {
                this.tetMesh = tetMesh;
                vertexShader = new(vertexShaderProgram, tetMesh.vertices);
                culler = new(cullShaderProgram, tetMesh.tets);
            }
        }

        public (int[] triangleData, float[] vertexData) GenerateTriangleMesh(float zSlice, float vanishingW, float nearW)
        {
            ComputeBuffer vertexBuffer = vertexShader.Render(modelWorldTransform4D.rotation, modelWorldTransform4D.translation, zSlice, vanishingW, nearW);
            //VariableLengthComputeBuffer tetrahedraToDraw = culler.Render(vertexBuffer);
            var tetrahedraUnpacked = tetMesh.tets.SelectMany(tet => tet.tetPoints).ToArray();
            ComputeBuffer tetsToDraw = RenderUtils.InitComputeBuffer<int>(sizeof(int), tetrahedraUnpacked);
            //if (tetrahedraToDraw.Count > 0)
            if (tetsToDraw.count > 0)
            {
                //var tetSlicer = new TetSlicer(sliceShaderProgram, tetrahedraToDraw.Buffer, tetrahedraToDraw.Count);
                var tetSlicer = new TetSlicer(sliceShaderProgram, tetsToDraw, tetsToDraw.count / 4);
                VariableLengthComputeBuffer.BufferList trianglesToDraw = tetSlicer.Render(vertexBuffer);

                VariableLengthComputeBuffer triangleBuffer = trianglesToDraw.Buffers[0];
                VariableLengthComputeBuffer triangleVertexBuffer = trianglesToDraw.Buffers[1];

                int[] triangleData = new int[triangleBuffer.Count * TetSlicer.PTS_PER_TRIANGLE];
                float[] triangleVertexData = new float[triangleVertexBuffer.Count * 4];
                triangleBuffer.Buffer.GetData(triangleData);
                triangleVertexBuffer.Buffer.GetData(triangleVertexData);

                tetSlicer.Dispose();

                return (triangleData, triangleVertexData);
            }

            return (null, null);
        }

        // Generate triangle mesh
        public void Render(float zSliceStart, float zSliceLength, float zSliceInterval, float vanishingW, float nearW)
        {
            if (vertexShader != null && culler != null)
            {
                for (float zSlice = zSliceStart; zSlice <= zSliceStart + zSliceLength; zSlice += zSliceInterval)
                {
                    (int[] triangleData, float[] vertexData) = GenerateTriangleMesh(zSlice, vanishingW, nearW);
                    triangleMesh.UpdateData(vertexData, triangleData);
                }

                triangleMesh.Render();
                triangleMesh.Reset();
            }
        }

        private void OnEnable()
        {
            if (vertexShader != null)
            {
                vertexShader.OnEnable(tetMesh.vertices);
            }
            if (culler != null)
            {
                culler.OnEnable(tetMesh.tets);
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

