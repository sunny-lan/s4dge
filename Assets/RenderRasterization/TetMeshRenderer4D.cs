using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
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

        TriangleMesh triangleMesh;

        TetMesh4D _tetMesh;
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

            vertexShader = new(vertexShaderProgram, tetMesh.vertices, lightSourceManager.LightSources);
            culler = new(cullShaderProgram, tetMesh.tets);
            tetSlicer = new(sliceShaderProgram, tetMesh.tets.Length, lightSourceManager.LightSources.Count);
        }

        public float[] PackTriangleVertexData(VariableLengthComputeBuffer triangleVertexBuffer, VariableLengthComputeBuffer lightSpaceTriangleVertexBuffer, int numLights)
        {
            float[] triangleVertexData = new float[triangleVertexBuffer.Count * TetMesh4D.VertexData.SizeFloats];
            float[] lightSpaceVertexData = new float[lightSpaceTriangleVertexBuffer.Count * 4 * numLights];

            triangleVertexBuffer.Buffer.GetData(triangleVertexData);
            lightSpaceTriangleVertexBuffer.Buffer.GetData(lightSpaceVertexData);

            float[] ret = new float[triangleVertexData.Length + lightSpaceVertexData.Length];

            int floatsPerVertex = TetMesh4D.VertexData.SizeFloats + 4;

            Assert.IsTrue(triangleVertexBuffer.Count == lightSpaceTriangleVertexBuffer.Count);
            for (int i = 0; i < triangleVertexBuffer.Count; ++i)
            {
                for (int j = 0; j < TetMesh4D.VertexData.SizeFloats; ++j)
                {
                    ret[i * floatsPerVertex + j] = triangleVertexData[i * TetMesh4D.VertexData.SizeFloats + j];
                }
                for (int j = 0; j < 4; ++j)
                {
                    ret[i * floatsPerVertex + TetMesh4D.VertexData.SizeFloats + j] = lightSpaceVertexData[i * 4 + j];
                }
            }

            //Debug.Log("num vertices: " + ret.Length / 16 + ", " + triangleVertexBuffer.Count);

            return ret;
        }

        public (int[] triangleData, float[] vertexData) GenerateTriangleMesh(float zSlice, ComputeBuffer vertexBuffer, ComputeBuffer lightSpaceVertexBuffer, ComputeBuffer tetDrawBuffer, ComputeBuffer numTetsBuffer)
        {
            VariableLengthComputeBuffer.BufferList trianglesToDraw = tetSlicer.Render(vertexBuffer, lightSpaceVertexBuffer, tetDrawBuffer, numTetsBuffer, zSlice);
            trianglesToDraw.UpdateBufferLengths();

            VariableLengthComputeBuffer triangleBuffer = trianglesToDraw.Buffers[0];
            VariableLengthComputeBuffer triangleVertexBuffer = trianglesToDraw.Buffers[1];
            VariableLengthComputeBuffer lightSpaceTriangleVertexBuffer = trianglesToDraw.Buffers[2];

            int[] triangleData = new int[triangleBuffer.Count * TetSlicer.PTS_PER_TRIANGLE];


            triangleBuffer.Buffer.GetData(triangleData);


            return (triangleData, PackTriangleVertexData(triangleVertexBuffer, lightSpaceTriangleVertexBuffer, lightSourceManager.LightSources.Count));
        }

        public (ComputeBuffer, ComputeBuffer, ComputeBuffer, ComputeBuffer) TransformAndCullVertices(
            TransformMatrixAffine4D worldToCameraTransform, float farClipPlane, float nearClipPlane)
        {
            Camera camera3D = Camera4D.main.camera3D;

            var (vertexBuffer, lightSpaceVertexBuffer) = vertexShader.Render(
                            worldToCameraTransform * modelWorldTransform4D.localToWorldMatrix,
                            modelWorldTransform4D.localToWorldMatrix,
                            Matrix4x4.identity,
                            farClipPlane, nearClipPlane
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
                numTetsBuffer = RenderUtils.InitComputeBuffer<int>(sizeof(int), new int[1] { tetrahedraUnpacked.Length / TetMesh4D.PTS_PER_TET });
            }

            return (vertexBuffer, lightSpaceVertexBuffer, tetDrawBuffer, numTetsBuffer);
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

                var (vertexBuffer, lightSpaceVertexBuffer, tetDrawBuffer, numTetsBuffer) = TransformAndCullVertices(worldToCameraTransform, farClipPlane, nearClipPlane);

                for (float zSlice = zSliceStart; zSlice <= zSliceStart + zSliceLength; zSlice += zSliceInterval)
                {
                    (int[] triangleData, float[] vertexData) = GenerateTriangleMesh(zSlice, vertexBuffer, lightSpaceVertexBuffer, tetDrawBuffer, numTetsBuffer);
                    //RenderUtils.PrintTriMeshData(vertexData, triangleData);
                    renderMesh.UpdateData(vertexData, triangleData, lightSourceManager.LightSources.Count);
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

        private void OnDestroy()
        {
            OnDisable();
        }
    }

}

