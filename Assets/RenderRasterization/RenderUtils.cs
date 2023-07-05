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

        public static T[] GetComputeBufferData<T>(ComputeBuffer buf) where T : struct
        {
            T _ = new();
            T[] values = new T[buf.count * buf.stride / System.Runtime.InteropServices.Marshal.SizeOf(_)];
            buf.GetData(values);
            return values;
        }

        public static void PrintComputeBufferData<T>(ComputeBuffer buf, string name) where T : struct
        {
            T[] values = GetComputeBufferData<T>(buf);
            Debug.Log(name + ": " + string.Join(", ", values));
        }
    }
}
