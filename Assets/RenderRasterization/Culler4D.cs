using System.Linq;
using Unity.Collections;
using UnityEngine;
using static RasterizationRenderer.TetMesh4D;


namespace RasterizationRenderer
{
    public class Culler4D
    {
        [SerializeField]
        public ComputeShader cullShader;
        ComputeBuffer tetrahedraBuffer;
        ComputeBuffer drawTetrahedron;
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
        public Tet4D[] Render(ComputeBuffer vertexBuffer)
        {
            // Run vertex shader to transform points and perform perspective projection
            cullShader.SetBuffer(cullShaderKernel, "transformedVertices", vertexBuffer);
            cullShader.SetBuffer(cullShaderKernel, "tetrahedra", tetrahedraBuffer);
            cullShader.SetBuffer(cullShaderKernel, "shouldDraw", drawTetrahedron);
            int numThreadGroups = (int)((vertexBuffer.count + (threadGroupSize - 1)) / threadGroupSize);
            cullShader.Dispatch(cullShaderKernel, numThreadGroups, 1, 1);

            bool[] drawTetrahedronArr = new bool[tetrahedra.Length];
            drawTetrahedron.GetData(drawTetrahedronArr);

            // return array of tetrahedra that drawTetrahedron says should be drawn
            return tetrahedra.Zip(drawTetrahedronArr, (tet, shouldDraw) => (tet, shouldDraw)).Where(elem => elem.shouldDraw).Select(elem => elem.tet).ToArray();
        }

        public void OnEnable()
        {
            tetrahedraBuffer = new(tetrahedra.Length, sizeof(int) * PTS_PER_TET);
            NativeArray<Tet4D> tetInputBuffer = tetrahedraBuffer.BeginWrite<Tet4D>(0, tetrahedra.Length);
            tetInputBuffer.CopyFrom(tetrahedra);
            tetrahedraBuffer.EndWrite<Vector4>(tetrahedra.Length);

            cullShaderKernel = cullShader.FindKernel("Culler4D");
            cullShader.GetKernelThreadGroupSizes(cullShaderKernel, out threadGroupSize, out _, out _);

            drawTetrahedron = new(tetrahedra.Length, sizeof(int) * PTS_PER_TET);
        }

        public void OnDisable()
        {
            // cleanup for vertex shader
            tetrahedraBuffer.Dispose();
            tetrahedraBuffer = null;
            drawTetrahedron.Dispose();
            drawTetrahedron = null;
        }
    }
}
