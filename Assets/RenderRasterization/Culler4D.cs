using System.Linq;
using Unity.Collections;
using UnityEngine;
using static RasterizationRenderer.TetMesh4D;
using static RasterizationRenderer.RenderUtils;
using UnityEngine.Rendering;

namespace RasterizationRenderer
{
    public class Culler4D
    {
        [SerializeField]
        public ComputeShader cullShader;
        ComputeBuffer tetrahedraBuffer;
        ComputeBuffer tetsToDraw;
        int cullShaderKernel;
        uint threadGroupSize;

        Tet4D[] tetrahedra;

        public Culler4D(Tet4D[] tetrahedra)
        {
            this.tetrahedra = tetrahedra;
        }

        /*
         * modelViewRotation4D, modelViewTranslation4D: transformation of 4D object
         * zSlice: z-coordinate of slicing plane for camera
         * vanishingW: camera clip plane - vanishing point at (0, 0, 0, vanishingW)
         * nearW: camera viewport plane at w = nearW
         */
        public CulledTetrahedraBuffer Render(ComputeBuffer vertexBuffer)
        {
            ComputeBuffer curGlobalDrawIdx = InitComputeBuffer<uint>(sizeof(uint), new uint[1] { 0 });

            // Run vertex shader to transform points and perform perspective projection
            cullShader.SetBuffer(cullShaderKernel, "transformedVertices", vertexBuffer);
            cullShader.SetBuffer(cullShaderKernel, "tetrahedra", tetrahedraBuffer);
            cullShader.SetBuffer(cullShaderKernel, "tetsToDraw", tetsToDraw);
            cullShader.SetBuffer(cullShaderKernel, "curGlobalDrawIdx", curGlobalDrawIdx);
            int numThreadGroups = (int)((vertexBuffer.count + (threadGroupSize - 1)) / threadGroupSize);
            cullShader.Dispatch(cullShaderKernel, numThreadGroups, 1, 1);

            uint[] tmp = new uint[1];
            curGlobalDrawIdx.GetData(tmp);
            uint numTetsToDraw = tmp[0];
            curGlobalDrawIdx.Dispose();

            // return array of tetrahedra that drawTetrahedron says should be drawn
            return new CulledTetrahedraBuffer(tetsToDraw, numTetsToDraw);
        }

        public void OnEnable()
        {
            tetrahedraBuffer = RenderUtils.InitComputeBuffer<Tet4D>(sizeof(int) * PTS_PER_TET, tetrahedra);

            cullShaderKernel = cullShader.FindKernel("Culler4D");
            cullShader.GetKernelThreadGroupSizes(cullShaderKernel, out threadGroupSize, out _, out _);

            tetsToDraw = new(tetrahedra.Length, sizeof(int) * PTS_PER_TET);
        }

        public void OnDisable()
        {
            // cleanup for vertex shader
            tetrahedraBuffer.Dispose();
            tetrahedraBuffer = null;
            tetsToDraw.Dispose();
            tetsToDraw = null;
        }

        public struct CulledTetrahedraBuffer
        {
            ComputeBuffer buffer;
            uint numTetsToDraw;

            public CulledTetrahedraBuffer(ComputeBuffer buffer, uint numTetsToDraw)
            {
                this.buffer = buffer;
                this.numTetsToDraw = numTetsToDraw;
            }
        }
    }
}
