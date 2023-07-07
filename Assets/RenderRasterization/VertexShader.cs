using UnityEngine;
using v2;
using static RasterizationRenderer.TetMesh4D;

namespace RasterizationRenderer
{
    public class VertexShader
    {
        ComputeShader vertexShader;
        ComputeBuffer inputVertices;
        ComputeBuffer transformedVertices;
        int vertexShaderKernel;
        uint threadGroupSize;

        Matrix4x4 modelViewProjection3D;

        int numVertices;

        public VertexShader(ComputeShader vertexShader, VertexData[] vertices)
        {
            this.vertexShader = vertexShader;
            this.modelViewProjection3D = Matrix4x4.identity;

            OnEnable(vertices);
        }

        /*
         * modelViewRotation4D, modelViewTranslation4D: transformation of 4D object
         * zSlice: z-coordinate of slicing plane for camera
         * vanishingW: camera clip plane - vanishing point at (0, 0, 0, vanishingW)
         * nearW: camera viewport plane at w = nearW
         */
        public ComputeBuffer Render(TransformMatrixAffine4D modelViewTransform4D, float zSlice, float vanishingW, float nearW)
        {
            if (inputVertices != null && transformedVertices != null)
            {
                // Run vertex shader to transform points and perform perspective projection

                // Set uniform variables
                vertexShader.SetMatrix("modelViewScaleAndRot4D", modelViewTransform4D.scaleAndRot);
                vertexShader.SetMatrix("modelViewScaleAndRotInv4D", modelViewTransform4D.inverse.scaleAndRot);
                vertexShader.SetMatrix("modelViewProjection3D", modelViewProjection3D);
                vertexShader.SetVector("modelViewTranslation4D", modelViewTransform4D.translation);
                vertexShader.SetFloat("zSlice", zSlice);
                vertexShader.SetFloat("vanishingW", vanishingW);
                vertexShader.SetFloat("nearW", nearW);
                vertexShader.SetInt("vertexCount", inputVertices.count);

                // Set buffers
                vertexShader.SetBuffer(vertexShaderKernel, "vertices", inputVertices);
                vertexShader.SetBuffer(vertexShaderKernel, "transformedVertices", transformedVertices);

                // Run shader
                int numThreadGroups = (int)((numVertices + (threadGroupSize - 1)) / threadGroupSize);
                vertexShader.Dispatch(vertexShaderKernel, numThreadGroups, 1, 1);

                return transformedVertices;
            }
            else
            {
                return null;
            }
        }

        public void OnEnable(VertexData[] vertices)
        {
            numVertices = vertices.Length;

            // set input data for vertex shader
            inputVertices = RenderUtils.InitComputeBuffer(VertexData.SizeBytes, vertices);

            vertexShaderKernel = vertexShader.FindKernel("vert");
            vertexShader.GetKernelThreadGroupSizes(vertexShaderKernel, out threadGroupSize, out _, out _);

            transformedVertices = new ComputeBuffer(vertices.Length, VertexData.SizeBytes, ComputeBufferType.Default, ComputeBufferMode.Immutable);
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
