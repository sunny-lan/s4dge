using System.Linq;
using UnityEngine;

namespace RasterizationRenderer
{

    public class TetMeshRenderer4D : MonoBehaviour
    {
        public v2.Transform4D modelWorldTransform4D;
        public static readonly int PTS_PER_TET = 4;
        public bool useCuller;

        [SerializeField]
        public ComputeShader vertexShaderProgram;
        [SerializeField]
        public ComputeShader cullShaderProgram;
        [SerializeField]
        public ComputeShader sliceShaderProgram;
        VertexShader vertexShader;

        Culler4D culler;

        Camera4D camera4D;

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

        public (int[] triangleData, float[] vertexData) GenerateTriangleMesh(float zSlice)
        {
            Camera camera3D = camera4D.camera3D;

            ComputeBuffer vertexBuffer = vertexShader.Render(
                camera4D.WorldToCameraTransform * modelWorldTransform4D.localToWorldMatrix,
                Matrix4x4.identity,
                zSlice, camera3D.farClipPlane, camera3D.nearClipPlane
            );
            var tetrahedraUnpacked = tetMesh.tets.SelectMany(tet => tet.tetPoints).ToArray();

            int tetDrawCount = 0;
            ComputeBuffer tetDrawBuffer;

            if (useCuller)
            {
                VariableLengthComputeBuffer tetrahedraToDraw = culler.Render(vertexBuffer);
                tetDrawCount = tetrahedraToDraw.Count;
                tetDrawBuffer = tetrahedraToDraw.Buffer;
            }
            else
            {
                tetDrawBuffer = RenderUtils.InitComputeBuffer<int>(sizeof(int), tetrahedraUnpacked);
                tetDrawCount = tetDrawBuffer.count / 4;
            }

            if (tetDrawCount > 0)
            {
                var tetSlicer = new TetSlicer(sliceShaderProgram, tetDrawBuffer, tetDrawCount);
                VariableLengthComputeBuffer.BufferList trianglesToDraw = tetSlicer.Render(vertexBuffer);

                VariableLengthComputeBuffer triangleBuffer = trianglesToDraw.Buffers[0];
                VariableLengthComputeBuffer triangleVertexBuffer = trianglesToDraw.Buffers[1];

                int[] triangleData = new int[triangleBuffer.Count * TetSlicer.PTS_PER_TRIANGLE];
                float[] triangleVertexData = new float[triangleVertexBuffer.Count * TetMesh4D.VertexData.SizeFloats];
                triangleBuffer.Buffer.GetData(triangleData);
                triangleVertexBuffer.Buffer.GetData(triangleVertexData);

                tetSlicer.Dispose();

                return (triangleData, triangleVertexData);
            }

            return (null, null);
        }

        // Generate triangle mesh
        public void Render(float zSliceStart, float zSliceLength, float zSliceInterval)
        {
            if (vertexShader != null && culler != null)
            {
                for (float zSlice = zSliceStart; zSlice <= zSliceStart + zSliceLength; zSlice += zSliceInterval)
                {
                    (int[] triangleData, float[] vertexData) = GenerateTriangleMesh(zSlice);
                    if (triangleData != null && vertexData != null)
                    {
                        triangleMesh.UpdateData(vertexData, triangleData);
                    }
                }

                triangleMesh.Render(camera4D.camera3D);
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
            camera4D = Camera4D.main;
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

