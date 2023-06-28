using System.Linq;
using UnityEngine;
using static RasterizationRenderer.TetMesh4D;

namespace RasterizationRenderer
{
    public class Culler4D
    {
        ComputeShader cullShader;
        ComputeBuffer tetrahedraBuffer;
        VariableLengthComputeBuffer tetsToDraw;
        VariableLengthComputeBuffer.BufferList bufferList;
        int cullShaderKernel;
        uint threadGroupSize;

        int numTets;

        public Culler4D(ComputeShader cullShader, Tet4D[] tetrahedra)
        {
            this.cullShader = cullShader;
            OnEnable(tetrahedra);
        }

        /*
         * modelViewRotation4D, modelViewTranslation4D: transformation of 4D object
         * zSlice: z-coordinate of slicing plane for camera
         * vanishingW: camera clip plane - vanishing point at (0, 0, 0, vanishingW)
         * nearW: camera viewport plane at w = nearW
         */
        public VariableLengthComputeBuffer Render(ComputeBuffer vertexBuffer)
        {
            bufferList.PrepareForRender();

            // Run vertex shader to transform points and perform perspective projection
            cullShader.SetBuffer(cullShaderKernel, "transformedVertices", vertexBuffer);
            cullShader.SetBuffer(cullShaderKernel, "tetrahedra", tetrahedraBuffer);
            cullShader.SetInt("tetCount", numTets);
            int numThreadGroups = (int)((numTets + (threadGroupSize - 1)) / threadGroupSize);
            cullShader.Dispatch(cullShaderKernel, numThreadGroups, 1, 1);

            bufferList.UpdateBufferLengths();

            // return array of tetrahedra that drawTetrahedron says should be drawn
            return tetsToDraw;
        }

        public void OnEnable(Tet4D[] tetrahedra)
        {
            numTets = tetrahedra.Length;

            // We can't directly send the Tet4D struct as the points are not directly stored in the struct (references are)
            var tetrahedraUnpacked = tetrahedra.SelectMany(tet => tet.tetPoints).ToArray();
            tetrahedraBuffer = RenderUtils.InitComputeBuffer<int>(sizeof(int), tetrahedraUnpacked);

            cullShaderKernel = cullShader.FindKernel("Culler4D");
            cullShader.GetKernelThreadGroupSizes(cullShaderKernel, out threadGroupSize, out _, out _);

            tetsToDraw = new("tetsToDraw", tetrahedra.Length, sizeof(int) * PTS_PER_TET);
            bufferList = new(new VariableLengthComputeBuffer[1] { tetsToDraw }, cullShader, cullShaderKernel);
        }

        public void OnDisable()
        {
            if (tetrahedraBuffer != null) { tetrahedraBuffer.Dispose(); tetrahedraBuffer = null; }
            if (tetsToDraw != null) { tetsToDraw.Dispose(); tetsToDraw = null; }
            if (bufferList != null) { bufferList.Dispose(); bufferList = null; }
        }
    }
}
