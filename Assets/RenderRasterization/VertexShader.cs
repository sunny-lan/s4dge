using System.Collections.Generic;
using System.Linq;
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

        ComputeBuffer modelLightScaleAndRot4D;
        ComputeBuffer modelLightTranslation4D;
        ComputeBuffer transformedLightVertices;

        int vertexShaderKernel;
        uint threadGroupSize;

        int numVertices;
        List<LightSource4D> lightSources;

        public VertexShader(ComputeShader vertexShader, VertexData[] vertices, List<LightSource4D> lightSources)
        {
            this.vertexShader = vertexShader;
            this.lightSources = lightSources;

            OnEnable(vertices);
        }

        /*
         * modelViewRotation4D, modelViewTranslation4D: transformation of 4D object
         * vanishingW: camera clip plane - vanishing point at (0, 0, 0, vanishingW)
         * nearW: camera viewport plane at w = nearW
         */
        public (ComputeBuffer, ComputeBuffer) Render(TransformMatrixAffine4D modelViewTransform4D,
            TransformMatrixAffine4D modelWorldTransform4D,
            Matrix4x4 modelViewProjection3D,
            float vanishingW, float nearW)
        {
            if (inputVertices != null && transformedVertices != null)
            {
                // Run vertex shader to transform points and perform perspective projection

                // update light source transforms
                modelLightScaleAndRot4D?.Dispose();
                modelLightTranslation4D?.Dispose();
                var lightSourceScaleAndRots = lightSources.Select(lightSource => (lightSource.WorldToLightTransform * modelWorldTransform4D).scaleAndRot).ToArray();
                var lightSourceTranslations = lightSources.Select(lightSource => (lightSource.WorldToLightTransform * modelWorldTransform4D).translation).ToArray();
                modelLightScaleAndRot4D = RenderUtils.InitComputeBuffer(sizeof(float) * 4 * 4, lightSourceScaleAndRots);
                modelLightTranslation4D = RenderUtils.InitComputeBuffer(sizeof(float) * 4, lightSourceTranslations);
                vertexShader.SetBuffer(vertexShaderKernel, "modelLightScaleAndRot4D", modelLightScaleAndRot4D);
                vertexShader.SetBuffer(vertexShaderKernel, "modelLightTranslation4D", modelLightTranslation4D);

                // Set uniform variables
                vertexShader.SetVector("modelViewTranslation4D", modelViewTransform4D.translation);
                vertexShader.SetMatrix("modelViewScaleAndRot4D", modelViewTransform4D.scaleAndRot);
                vertexShader.SetMatrix("modelViewScaleAndRotInv4D", modelViewTransform4D.inverse.scaleAndRot);
                vertexShader.SetMatrix("modelViewProjection3D", modelViewProjection3D);
                vertexShader.SetFloat("vanishingW", vanishingW);
                vertexShader.SetFloat("nearW", nearW);
                vertexShader.SetInt("vertexCount", inputVertices.count);
                vertexShader.SetInt("numLights", lightSources.Count);

                // Set buffers
                vertexShader.SetBuffer(vertexShaderKernel, "vertices", inputVertices);
                vertexShader.SetBuffer(vertexShaderKernel, "transformedVertices", transformedVertices);
                vertexShader.SetBuffer(vertexShaderKernel, "transformedLightPos", transformedLightVertices);

                // Run shader
                int numThreadGroups = (int)((numVertices + (threadGroupSize - 1)) / threadGroupSize);
                vertexShader.Dispatch(vertexShaderKernel, numThreadGroups, 1, 1);

                return (transformedVertices, transformedLightVertices);
            }
            else
            {
                return (null, null);
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
            transformedLightVertices = new ComputeBuffer(vertices.Length * lightSources.Count, VertexData.SizeBytes, ComputeBufferType.Default, ComputeBufferMode.Immutable);
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
            transformedLightVertices?.Dispose();
            transformedLightVertices = null;
        }
    }
}
