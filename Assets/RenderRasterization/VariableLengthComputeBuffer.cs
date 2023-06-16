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
        public ComputeBuffer Buffer
        {
            get => Buffer;
            internal set { Buffer = value; }
        }

        readonly string name;

        public VariableLengthComputeBuffer(string name, int capacity, int stride)
        {
            this.name = name;
            Buffer = new(capacity, stride);
            this.Length = 0;
        }

        ~VariableLengthComputeBuffer()
        {
            Buffer.Dispose();
            Buffer = null;
        }

        public void PrepareForRender(ComputeShader shader, int shaderKernel)
        {
            shader.SetBuffer(shaderKernel, name, Buffer);
        }

        // Helper functions to help set up variable length buffer functionality in SlicerUtils.cginc
        public class BufferList
        {
            public int Length
            {
                get => Buffers.Length;
            }
            public VariableLengthComputeBuffer[] Buffers
            {
                get => Buffers;
                internal set { Buffers = value; }
            }
            ComputeBuffer curGlobalDrawIdx;
            private ComputeShader shader;
            private int shaderKernel;

            public BufferList(VariableLengthComputeBuffer[] buffers, ComputeShader shader, int shaderKernel)
            {
                this.Buffers = buffers;
                this.shader = shader;
                this.shaderKernel = shaderKernel;
            }

            public void PrepareForRender()
            {
                uint[] globalDrawIdxInitValues = new uint[Buffers.Length];
                curGlobalDrawIdx = InitComputeBuffer<uint>(sizeof(uint), globalDrawIdxInitValues);
                shader.SetBuffer(shaderKernel, "curGlobalDrawIdx", curGlobalDrawIdx);

                foreach (var buffer in Buffers)
                {
                    buffer.PrepareForRender(shader, shaderKernel);
                }
            }

            // Call after dispatching shader
            public void UpdateBufferLengths()
            {
                uint[] bufferLengths = new uint[Buffers.Length];
                curGlobalDrawIdx.GetData(bufferLengths);
                for (int i = 0; i < Buffers.Length; i++)
                {
                    Buffers[i].Length = (int)bufferLengths[i];
                }
            }
        }
    }
}