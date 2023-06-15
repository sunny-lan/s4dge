using UnityEngine;
using static RasterizationRenderer.RenderUtils;

namespace RasterizationRenderer
{
    // Helper functions to help set up variable length buffer functionality in SlicerUtils.cginc
    public class VariableLengthComputeBuffer
    {
        public int Length
        {
            get => Length;
            internal set { Length = value; }
        }
        private ComputeShader shader;
        private int shaderKernel;
        public ComputeBuffer Buffer
        {
            get => Buffer;
            internal set { Buffer = value; }
        }
        ComputeBuffer curGlobalDrawIdx;

        string name;

        public VariableLengthComputeBuffer(string name, int capacity, int stride, ComputeShader shader, int shaderKernel)
        {
            this.name = name;
            this.shader = shader;
            this.shaderKernel = shaderKernel;
            Buffer = new(capacity, stride);
            this.Length = 0;
        }

        ~VariableLengthComputeBuffer()
        {
            Buffer.Dispose();
            Buffer = null;
        }

        public void PrepareForRender()
        {
            curGlobalDrawIdx = InitComputeBuffer<uint>(sizeof(uint), new uint[1] { 0 });
            shader.SetBuffer(shaderKernel, "curGlobalDrawIdx", curGlobalDrawIdx);

            shader.SetBuffer(shaderKernel, name, Buffer);
        }

        // Call after dispatching shader
        public void UpdateNumElements()
        {
            uint[] tmp = new uint[1];
            curGlobalDrawIdx.GetData(tmp);
            Length = (int)tmp[0];
            curGlobalDrawIdx.Dispose();
            curGlobalDrawIdx = null;
        }
    }
}