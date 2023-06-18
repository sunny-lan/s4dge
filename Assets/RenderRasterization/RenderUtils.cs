using UnityEngine;

namespace RasterizationRenderer
{
    public class RenderUtils
    {
        public static ComputeBuffer InitComputeBuffer<T>(int stride, T[] contents) where T : struct
        {
            ComputeBuffer buf = new(contents.Length, stride, ComputeBufferType.Default, ComputeBufferMode.Immutable);
            WriteToComputeBuffer(buf, contents);

            return buf;
        }

        public static void WriteToComputeBuffer<T>(ComputeBuffer buf, T[] contents) where T : struct
        {
            //int count = contents.Length;
            //NativeArray<T> tetInputBuffer = buf.BeginWrite<T>(0, count);
            //tetInputBuffer.CopyFrom(contents);
            //buf.EndWrite<T>(count);
            buf.SetData(contents);
        }
    }
}