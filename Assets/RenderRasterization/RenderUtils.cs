using System.Collections;
using Unity.Collections;
using UnityEngine;

namespace RasterizationRenderer
{
    public class RenderUtils
    {
        public static ComputeBuffer InitComputeBuffer<T>(int stride, T[] contents) where T : struct
        {
            int count = contents.Length;
            ComputeBuffer buf = new(count, stride);
            NativeArray<T> tetInputBuffer = buf.BeginWrite<T>(0, count);
            tetInputBuffer.CopyFrom(contents);
            buf.EndWrite<T>(count);

            return buf;
        }
    }
}