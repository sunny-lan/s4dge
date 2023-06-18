using System;
using UnityEngine;
using static RasterizationRenderer.RenderUtils;

namespace RasterizationRenderer
{
    // Helper functions to help set up variable length buffer functionality in SlicerUtils.cginc
    public class VariableLengthComputeBuffer
    {
        int _count;
        public int Count
        {
            get => _count;
            internal set { _count = value; }
        }

        ComputeBuffer _buffer;
        public ComputeBuffer Buffer
        {
            get => _buffer;
            internal set { _buffer = value; }
        }

        readonly string name;

        public VariableLengthComputeBuffer(string name, int capacity, int stride)
        {
            this.name = name;
            Buffer = new(capacity, stride);
            this.Count = 0;
        }

        public void Dispose()
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

            VariableLengthComputeBuffer[] _buffers;
            public VariableLengthComputeBuffer[] Buffers
            {
                get => _buffers;
                internal set { _buffers = value; }
            }
            ComputeBuffer curGlobalAppendIdx;
            private ComputeShader shader;
            private int shaderKernel;
            uint[] globalAppendIdxInitValues;

            public BufferList(VariableLengthComputeBuffer[] buffers, ComputeShader shader, int shaderKernel)
            {
                this.Buffers = buffers;
                this.shader = shader;
                this.shaderKernel = shaderKernel;
                this.globalAppendIdxInitValues = new uint[Buffers.Length];
                this.curGlobalAppendIdx = InitComputeBuffer<uint>(sizeof(uint), globalAppendIdxInitValues);
            }

            public void PrepareForRender()
            {
                shader.SetBuffer(shaderKernel, "curGlobalAppendIdx", curGlobalAppendIdx);

                foreach (var buffer in Buffers)
                {
                    buffer.PrepareForRender(shader, shaderKernel);
                }
            }

            // Call after dispatching shader
            public void UpdateBufferLengths()
            {
                curGlobalAppendIdx.GetData(globalAppendIdxInitValues);
                for (int i = 0; i < Buffers.Length; i++)
                {
                    Buffers[i].Count = (int)globalAppendIdxInitValues[i];
                }

                // zero global append idx buffer
                Array.Clear(globalAppendIdxInitValues, 0, Buffers.Length);
                RenderUtils.WriteToComputeBuffer(curGlobalAppendIdx, globalAppendIdxInitValues);
            }

            public void Dispose()
            {
                curGlobalAppendIdx.Dispose();
                curGlobalAppendIdx = null;
            }
        }
    }
}