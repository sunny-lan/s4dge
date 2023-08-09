using UnityEngine;

namespace RasterizationRenderer
{
    public class TetSlicer
    {
        ComputeShader sliceShader;
        ComputeBuffer tetrahedraBuffer;
        VariableLengthComputeBuffer triangleVertices;
        VariableLengthComputeBuffer slicedTriangles;
        VariableLengthComputeBuffer.BufferList bufferList;
        int maxNumTets;
        int sliceShaderKernel;
        uint threadGroupSize;

        public static readonly int PTS_PER_TRIANGLE = 3;

        public TetSlicer(ComputeShader sliceShader, int maxNumTets)
        {
            this.sliceShader = sliceShader;
            OnEnable(maxNumTets);
        }

        /*
         * modelViewRotation4D, modelViewTranslation4D: transformation of 4D object
         * zSlice: z-coordinate of slicing plane for camera
         * vanishingW: camera clip plane - vanishing point at (0, 0, 0, vanishingW)
         * nearW: camera viewport plane at w = nearW
         * 
         * returns: The list of triangle vertices as well as the list of triangles (each point pointing to a vertex)
         */
        public VariableLengthComputeBuffer.BufferList Render(ComputeBuffer vertexBuffer, ComputeBuffer tetrahedraBuffer, ComputeBuffer numTets, float zSlice)
        {
            bufferList.PrepareForRender();

            // Run vertex shader to transform points and perform perspective projection
            sliceShader.SetBuffer(sliceShaderKernel, "transformedVertices", vertexBuffer);
            sliceShader.SetBuffer(sliceShaderKernel, "tetsToDraw", tetrahedraBuffer);
            sliceShader.SetBuffer(sliceShaderKernel, "numTets", numTets);
            sliceShader.SetFloat("zSlice", zSlice);
            int numThreadGroups = (int)((maxNumTets + (threadGroupSize - 1)) / threadGroupSize);
            sliceShader.Dispatch(sliceShaderKernel, numThreadGroups, 1, 1);

            // return array of tetrahedra that drawTetrahedron says should be drawn
            return bufferList;
        }

        public void OnEnable(int maxNumTets)
        {
            this.maxNumTets = maxNumTets;

            sliceShaderKernel = sliceShader.FindKernel("TetrahedronSlicer");
            sliceShader.GetKernelThreadGroupSizes(sliceShaderKernel, out threadGroupSize, out _, out _);

            slicedTriangles = new("slicedTriangles", maxNumTets * 2, sizeof(int) * PTS_PER_TRIANGLE);
            triangleVertices = new("triangleVertices", maxNumTets * TetMesh4D.PTS_PER_TET, TetMesh4D.VertexData.SizeBytes);
            bufferList = new(new VariableLengthComputeBuffer[2] { slicedTriangles, triangleVertices }, sliceShader, sliceShaderKernel);
        }

        public void OnDisable()
        {
            if (slicedTriangles != null) { slicedTriangles.Dispose(); slicedTriangles = null; }
            if (triangleVertices != null) { triangleVertices.Dispose(); triangleVertices = null; }
            if (bufferList != null) { bufferList.Dispose(); bufferList = null; }
        }
    }
}
