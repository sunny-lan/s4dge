using UnityEngine;
using static RasterizationRenderer.TetMesh4D;

namespace RasterizationRenderer
{
    public class Culler4D
    {
        [SerializeField]
        public ComputeShader cullShader;
        ComputeBuffer tetrahedraBuffer;
        VariableLengthComputeBuffer tetsToDraw;
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
        public VariableLengthComputeBuffer Render(ComputeBuffer vertexBuffer)
        {
            tetsToDraw.PrepareForRender();

            // Run vertex shader to transform points and perform perspective projection
            cullShader.SetBuffer(cullShaderKernel, "transformedVertices", vertexBuffer);
            cullShader.SetBuffer(cullShaderKernel, "tetrahedra", tetrahedraBuffer);
            int numThreadGroups = (int)((tetrahedraBuffer.count + (threadGroupSize - 1)) / threadGroupSize);
            cullShader.Dispatch(cullShaderKernel, numThreadGroups, 1, 1);

            tetsToDraw.UpdateNumElements();

            // return array of tetrahedra that drawTetrahedron says should be drawn
            return tetsToDraw;
        }

        public void OnEnable()
        {
            tetrahedraBuffer = RenderUtils.InitComputeBuffer<Tet4D>(sizeof(int) * PTS_PER_TET, tetrahedra);

            cullShaderKernel = cullShader.FindKernel("Culler4D");
            cullShader.GetKernelThreadGroupSizes(cullShaderKernel, out threadGroupSize, out _, out _);

            tetsToDraw = new("tetsToDraw", tetrahedra.Length, sizeof(int) * PTS_PER_TET, cullShader, cullShaderKernel);
        }

        public void OnDisable()
        {
            tetrahedraBuffer.Dispose();
            tetrahedraBuffer = null;
            tetsToDraw = null;
        }
    }
}
