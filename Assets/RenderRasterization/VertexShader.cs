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

        public VertexShader(VertexData[] vertices)
        {
            this.vertices = vertices;
        }

        public ComputeBuffer Render()
        {
            // Run vertex shader to transform points and perform perspective projection
            vertexShader.SetBuffer(vertexShaderKernel, "vertices", inputVertices);
            vertexShader.SetBuffer(vertexShaderKernel, "transformedVertices", transformedVertices);
            int numThreadGroups = (int)((vertices.Length + (threadGroupSize - 1)) / threadGroupSize);
            vertexShader.Dispatch(vertexShaderKernel, numThreadGroups, 1, 1);

            return transformedVertices;
        }

        public void OnEnable()
        {
            // set input data for vertex shader
            inputVertices = new(vertices.Length, sizeof(float) * PTS_PER_TET);
            NativeArray<VertexData> inputBuffer = inputVertices.BeginWrite<VertexData>(0, vertices.Length);
            inputBuffer.CopyFrom(vertices);
            inputVertices.EndWrite<Vector4>(vertices.Length);

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
