using System;
using UnityEngine;

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
            uint[] globalAppendIdxValues;

            public BufferList(VariableLengthComputeBuffer[] buffers, ComputeShader shader, int shaderKernel)
            {
                this.Buffers = buffers;
                this.shader = shader;
                this.shaderKernel = shaderKernel;
                this.globalAppendIdxValues = new uint[Buffers.Length];
                this.curGlobalAppendIdx = new(Buffers.Length, sizeof(uint));
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
                curGlobalAppendIdx.GetData(globalAppendIdxValues);
                for (int i = 0; i < Buffers.Length; i++)
                {
                    Buffers[i].Count = (int)globalAppendIdxValues[i];
                }

                // zero global append idx buffer
                Array.Clear(globalAppendIdxValues, 0, Buffers.Length);
            }

            // Call after dispatching shader
            public ComputeBuffer GetBufferLengths()
            {
                return curGlobalAppendIdx;
            }

            public void Dispose()
            {
                curGlobalAppendIdx.Dispose();
                curGlobalAppendIdx = null;
            }
        }
    }
}