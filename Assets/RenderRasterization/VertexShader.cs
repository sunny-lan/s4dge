using UnityEngine;
using static RasterizationRenderer.TetMesh4D;

namespace RasterizationRenderer
{
    public class VertexShader
    {
        [SerializeField]
        ComputeShader vertexShader;
        ComputeBuffer inputVertices;
        ComputeBuffer transformedVertices;
        int vertexShaderKernel;
        uint threadGroupSize;

        VertexData[] vertices;
        Matrix4x4 modelViewProjection3D;

        public VertexShader(ComputeShader vertexShader, VertexData[] vertices)
        {
            this.vertexShader = vertexShader;
            this.vertices = vertices;
            this.modelViewProjection3D = Matrix4x4.identity;

            OnEnable();
        }

        /*
         * modelViewRotation4D, modelViewTranslation4D: transformation of 4D object
         * zSlice: z-coordinate of slicing plane for camera
         * vanishingW: camera clip plane - vanishing point at (0, 0, 0, vanishingW)
         * nearW: camera viewport plane at w = nearW
         */
        public ComputeBuffer Render(Matrix4x4 modelViewRotation4D, Vector4 modelViewTranslation4D, float zSlice, float vanishingW, float nearW)
        {
            if (inputVertices != null && transformedVertices != null)
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
            else
            {
                return null;
            }
        }

        public void OnEnable()
        {
            // set input data for vertex shader
            inputVertices = RenderUtils.InitComputeBuffer<VertexData>(VertexData.SizeBytes, vertices);

            vertexShaderKernel = vertexShader.FindKernel("vert");
            vertexShader.GetKernelThreadGroupSizes(vertexShaderKernel, out threadGroupSize, out _, out _);

            transformedVertices = new ComputeBuffer(vertices.Length, VertexData.SizeBytes * PTS_PER_TET, ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
        }

        public void OnDisable()
        {
            // cleanup for vertex shader
            if (inputVertices != null)
            {
                inputVertices.Dispose();
                inputVertices = null;
            }
            if (transformedVertices != null)
            {
                transformedVertices.Dispose();
                transformedVertices = null;
            }
        }
    }
}
