using System.Linq;
using UnityEngine;
using v2;

namespace RasterizationRenderer
{
    [RequireComponent(typeof(Transform4D))]
    [RequireComponent(typeof(TriangleMesh))]
    public class TetMeshRenderer4D : MonoBehaviour
    {
        v2.Transform4D modelWorldTransform4D;
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

        TetSlicer tetSlicer;

        Camera4D camera4D;

        TriangleMesh triangleMesh;
        private TetMesh4D tetMesh;

        LightSource4DManager lightSourceManager;

        public void SetTetMesh(TetMesh4D tetMesh)
        {
            if (tetMesh.vertices.Length > 0 && tetMesh.tets.Length > 0)
            {
                this.tetMesh = tetMesh;
                vertexShader = new(vertexShaderProgram, tetMesh.vertices);
                culler = new(cullShaderProgram, tetMesh.tets);
                tetSlicer = new(sliceShaderProgram, tetMesh.tets.Length);
            }
        }


        public (int[] triangleData, float[] vertexData) GenerateTriangleMesh(float zSlice, ComputeBuffer vertexBuffer, ComputeBuffer tetDrawBuffer, ComputeBuffer numTetsBuffer)
        {
            

            VariableLengthComputeBuffer.BufferList trianglesToDraw = tetSlicer.Render(vertexBuffer, tetDrawBuffer, numTetsBuffer, zSlice);
            trianglesToDraw.UpdateBufferLengths();

            VariableLengthComputeBuffer triangleBuffer = trianglesToDraw.Buffers[0];
            VariableLengthComputeBuffer triangleVertexBuffer = trianglesToDraw.Buffers[1];

            int[] triangleData = new int[triangleBuffer.Count * TetSlicer.PTS_PER_TRIANGLE];
            float[] triangleVertexData = new float[triangleVertexBuffer.Count * TetMesh4D.VertexData.SizeFloats];

            triangleBuffer.Buffer.GetData(triangleData);
            triangleVertexBuffer.Buffer.GetData(triangleVertexData);

            return (triangleData, triangleVertexData);
        }

        public (ComputeBuffer, ComputeBuffer, ComputeBuffer) TransformAndCullVertices(TransformMatrixAffine4D worldToCameraTransform)
        {
            Camera camera3D = camera4D.camera3D;

            ComputeBuffer vertexBuffer = vertexShader.Render(
                worldToCameraTransform * modelWorldTransform4D.localToWorldMatrix,
                modelWorldTransform4D.localToWorldMatrix,
                Matrix4x4.identity,
                camera3D.farClipPlane, camera3D.nearClipPlane
            );

            ComputeBuffer tetDrawBuffer;
            ComputeBuffer numTetsBuffer;

            if (useCuller)
            {
                VariableLengthComputeBuffer.BufferList tetrahedraToDraw = culler.Render(vertexBuffer);
                tetDrawBuffer = tetrahedraToDraw.Buffers[0].Buffer;
                numTetsBuffer = tetrahedraToDraw.GetBufferLengths();
            }
            else
            {
                var tetrahedraUnpacked = tetMesh.tets.SelectMany(tet => tet.tetPoints).ToArray();
                tetDrawBuffer = RenderUtils.InitComputeBuffer<int>(sizeof(int), tetrahedraUnpacked);
                numTetsBuffer = RenderUtils.InitComputeBuffer<int>(sizeof(int), new int[1] { tetrahedraUnpacked.Length });
            }

            return (vertexBuffer, tetDrawBuffer, numTetsBuffer);
        }

        // Generate triangle mesh
        public void Render(float zSliceStart, float zSliceLength, float zSliceInterval)
        {
            // don't draw unless zSliceInterval is large enough so that unity doesn't freeze when accidentally set to 0
            if (vertexShader != null && culler != null && zSliceInterval > 0.05)
            {
                triangleMesh.Reset();

                var (vertexBuffer, tetDrawBuffer, numTetsBuffer) = TransformAndCullVertices(camera4D.WorldToCameraTransform);

                for (float zSlice = zSliceStart; zSlice <= zSliceStart + zSliceLength; zSlice += zSliceInterval)
                {
                    (int[] triangleData, float[] vertexData) = GenerateTriangleMesh(zSlice, vertexBuffer, tetDrawBuffer, numTetsBuffer);
                    triangleMesh.UpdateData(vertexData, triangleData);
                }

                triangleMesh.Render(lightSourceManager, camera4D.WorldToCameraTransform);
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
            if (tetSlicer != null)
            {
                tetSlicer.OnEnable(tetMesh.tets.Length);
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
            if (tetSlicer != null)
            {
                tetSlicer.OnDisable();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            camera4D = Camera4D.main;

            lightSourceManager = new(new());
            foreach (LightSource4D lightSource in FindObjectsOfType<LightSource4D>())
            {
                lightSourceManager.Add(lightSource);
            }
            lightSourceManager.UpdateComputeBuffer();

            modelWorldTransform4D = GetComponent<Transform4D>();
            triangleMesh = GetComponent<TriangleMesh>();
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

