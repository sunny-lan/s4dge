using Unity.Collections;
using UnityEngine;
using static RasterizationRenderer.TetMesh4D;


namespace RasterizationRenderer
{
    public class VertexShader
    {
        [SerializeField]
        public ComputeShader vertexShader;
        ComputeBuffer inputVertices;
        ComputeBuffer transformedVertices;
        int vertexShaderKernel;
        uint threadGroupSize;

        VertexData[] vertices;
        Matrix4x4 modelViewProjection3D;

        public VertexShader(VertexData[] vertices)
        {
            this.vertices = vertices;
            this.modelViewProjection3D = Matrix4x4.identity;
        }

        /*
         * modelViewRotation4D, modelViewTranslation4D: transformation of 4D object
         * zSlice: z-coordinate of slicing plane for camera
         * vanishingW: camera clip plane - vanishing point at (0, 0, 0, vanishingW)
         * nearW: camera viewport plane at w = nearW
         */
        public ComputeBuffer Render(Matrix4x4 modelViewRotation4D, Vector4 modelViewTranslation4D, float zSlice, float vanishingW, float nearW)
        {
            // Run vertex shader to transform points and perform perspective projection

            // Set uniform variables
            vertexShader.SetMatrix("modelViewRotation4D", modelViewRotation4D);
            vertexShader.SetVector("modelViewTranslation4D", modelViewTranslation4D);
            vertexShader.SetFloat("zSlice", zSlice);
            vertexShader.SetFloat("vanishingW", vanishingW);
            vertexShader.SetFloat("nearW", nearW);

            // Set buffers
            vertexShader.SetBuffer(vertexShaderKernel, "vertices", inputVertices);
            vertexShader.SetBuffer(vertexShaderKernel, "transformedVertices", transformedVertices);

            // Run shader
            int numThreadGroups = (int)((vertices.Length + (threadGroupSize - 1)) / threadGroupSize);
            vertexShader.Dispatch(vertexShaderKernel, numThreadGroups, 1, 1);

            return transformedVertices;
        }

        public void OnEnable()
        {
            // set input data for vertex shader
            inputVertices = RenderUtils.InitComputeBuffer<VertexData>(sizeof(float) * PTS_PER_TET, vertices);

            vertexShaderKernel = vertexShader.FindKernel("vert");
            vertexShader.GetKernelThreadGroupSizes(vertexShaderKernel, out threadGroupSize, out _, out _);

            transformedVertices = new ComputeBuffer(vertices.Length, sizeof(float) * PTS_PER_TET);
        }

        public void OnDisable()
        {
            // cleanup for vertex shader
            inputVertices.Dispose();
            inputVertices = null;
            transformedVertices.Dispose();
            transformedVertices = null;
        }
    }
}
