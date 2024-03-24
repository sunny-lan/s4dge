using System.Linq;
using UnityEngine;
using S4DGE;

namespace RasterizationRenderer
{
    [RequireComponent(typeof(Transform4D))]
    [RequireComponent(typeof(TriangleMesh))]
    public class TetMeshRenderer4D : MonoBehaviour
    {
        S4DGE.Transform4D modelWorldTransform4D;
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

        TriangleMesh triangleMesh;

        TetMesh4D _tetMesh;

        private ComputeBuffer _tetDrawBuffer;
        private ComputeBuffer _numTetsBuffer;

        public TetMesh4D tetMesh
        {
            get { return _tetMesh; }
            internal set { _tetMesh = value; }
        }

        LightSource4DManager lightSourceManager;

        public void SetTetMesh(TetMesh4D tetMesh)
        {
            if (tetMesh.vertices.Length > 0 && tetMesh.tets.Length > 0)
            {
                this.tetMesh = tetMesh;
            }
        }

        public void AppendTetMesh(TetMesh4D tetMesh)
        {
            if (this.tetMesh == null)
            {
                Clear();
            }
            this.tetMesh.AppendTets(tetMesh.vertices, tetMesh.tets);
        }

        public void Clear()
        {
            this.tetMesh = new(new TetMesh4D.VertexData[0], new TetMesh4D.Tet4D[0]);
        }

        public void MeshInit()
        {
            // dispose of allocated resources if reiniting
            vertexShader?.OnDisable();
            culler?.OnDisable();
            tetSlicer?.OnDisable();

            vertexShader = new(vertexShaderProgram, tetMesh.vertices);
            culler = new(cullShaderProgram, tetMesh.tets);
            tetSlicer = new(sliceShaderProgram, tetMesh.tets.Length);
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

        public (ComputeBuffer, ComputeBuffer, ComputeBuffer) TransformAndCullVertices(
            TransformMatrixAffine4D worldToCameraTransform, float farClipPlane, float nearClipPlane)
        {
            Camera camera3D = Camera4D.main.camera3D;

            ComputeBuffer vertexBuffer = vertexShader.Render(
                worldToCameraTransform * modelWorldTransform4D.localToWorldMatrix,
                modelWorldTransform4D.localToWorldMatrix,
                Matrix4x4.identity,
                farClipPlane, nearClipPlane
            );

            if (_tetDrawBuffer != null)
            {
                _tetDrawBuffer.Release();
            }
            if (_numTetsBuffer != null)
            {
                _numTetsBuffer.Release();
            }

            if (useCuller)
            {
                VariableLengthComputeBuffer.BufferList tetrahedraToDraw = culler.Render(vertexBuffer);
                _tetDrawBuffer = tetrahedraToDraw.Buffers[0].Buffer;
                _numTetsBuffer = tetrahedraToDraw.GetBufferLengths();
            }
            else
            {
                var tetrahedraUnpacked = tetMesh.tets.SelectMany(tet => tet.tetPoints).ToArray();
                _tetDrawBuffer = RenderUtils.InitComputeBuffer<int>(sizeof(int), tetrahedraUnpacked);
                _numTetsBuffer = RenderUtils.InitComputeBuffer<int>(sizeof(int), new int[1] { tetrahedraUnpacked.Length / TetMesh4D.PTS_PER_TET });
            }

            return (vertexBuffer, _tetDrawBuffer, _numTetsBuffer);
        }

        // Generate triangle mesh
        public void Render(float zSliceStart, float zSliceLength, float zSliceInterval,
            TransformMatrixAffine4D worldToCameraTransform, float farClipPlane, float nearClipPlane,
            TriangleMesh overrideOutputMesh = null, Material overrideMaterial = null, RenderTexture outputTexture = null, bool clear = true)
        {
            // don't draw unless zSliceInterval is large enough so that unity doesn't freeze when accidentally set to 0
            if (vertexShader != null && culler != null && zSliceInterval > 0.05)
            {
                TriangleMesh renderMesh = triangleMesh;
                if (overrideOutputMesh != null)
                {
                    renderMesh = overrideOutputMesh;
                }

                if (clear)
                {
                    renderMesh.Reset();
                }

                var (vertexBuffer, tetDrawBuffer, numTetsBuffer) = TransformAndCullVertices(worldToCameraTransform, farClipPlane, nearClipPlane);

                for (float zSlice = zSliceStart; zSlice <= zSliceStart + zSliceLength; zSlice += zSliceInterval)
                {
                    (int[] triangleData, float[] vertexData) = GenerateTriangleMesh(zSlice, vertexBuffer, tetDrawBuffer, numTetsBuffer);
                    //RenderUtils.PrintTriMeshData(vertexData, triangleData);
                    renderMesh.UpdateData(vertexData, triangleData);
                }


                if (outputTexture == null)
                {
                    renderMesh.Render(lightSourceManager, worldToCameraTransform, farClipPlane, nearClipPlane);
                }
                else
                {
                    renderMesh.RenderToRenderTexture(outputTexture, Color.clear, overrideMaterial, false);
                }
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
            if (lightSourceManager != null)
            {
                lightSourceManager.OnDisable();
            }
            if (_tetDrawBuffer != null)
            {
                _tetDrawBuffer.Release();
            }
            if (_numTetsBuffer != null)
            {
                _numTetsBuffer.Release();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
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
    }

}

