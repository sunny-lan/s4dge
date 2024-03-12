﻿using System.Buffers;
using System.Collections.Generic;
using System.Linq;
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

        public static T[] GetComputeBufferData<T>(VariableLengthComputeBuffer buf) where T : struct
        {
            T _ = new();
            T[] values = new T[buf.Count];
            buf.Buffer.GetData(values);
            return values;
        }

        public static void PrintComputeBufferData<T>(ComputeBuffer buf, string name) where T : struct
        {
            T[] values = GetComputeBufferData<T>(buf);
            Debug.Log(name + ": " + string.Join(", ", values));
        }

        public static int[] GetTrianglesFromTet(TetMesh4D.Tet4D tet)
        {
            List<int> triPts = new();
            for (int excludePt = 0; excludePt < 4; ++excludePt)
            {
                var pts = Enumerable.Range(0, 4).Where(pt => pt != excludePt);
                triPts.AddRange(pts);
            }
            return triPts.ToArray();
        }

        public static TetMesh4D ReadMesh(ComputeBuffer vertices, ComputeBuffer indices, int numTets)
        {
            var indexBuf = GetComputeBufferData<int>(indices);
            var stuff = new List<TetMesh4D.Tet4D>();
            for (int i = 0; i < numTets * 4; i += 4)
            {
                stuff.Add(new TetMesh4D.Tet4D(indexBuf[i..(i + 4)]));
            }

            TetMesh4D mesh = new(
                GetComputeBufferData<TetMesh4D.VertexData>(vertices),
                stuff.ToArray()
            );
            return mesh;
        }

        public static Vector4 V4FromRawData(float[] rawData)
        {
            return new(rawData[0], rawData[1], rawData[2], rawData[3]);
        }

        public static TetMesh4D.VertexData[] ParseRawMeshVertices(float[] rawVertexData)
        {
            TetMesh4D.VertexData[] ret = new TetMesh4D.VertexData[rawVertexData.Length / TetMesh4D.VertexData.SizeFloats];
            List<float> dataList = rawVertexData.ToList();
            for (int i = 0; i < rawVertexData.Length; i += TetMesh4D.VertexData.SizeFloats)
            {
                ret[i / TetMesh4D.VertexData.SizeFloats] = new(
                    V4FromRawData(dataList.GetRange(i, 4).ToArray()),
                    V4FromRawData(dataList.GetRange(i + 4, 4).ToArray()),
                    V4FromRawData(dataList.GetRange(i + 8, 4).ToArray())
                );
            }
            return ret;
        }

        public static void PrintTriMeshData(float[] vertexData, int[] indices)
        {
            Debug.Log("vertices: " + string.Join(",", ParseRawMeshVertices(vertexData)) + "\n");
            Debug.Log("indices: " + string.Join(",", indices) + "\n");
        }

        public static void DebugDrawTet(TetMesh4D mesh)
        {
            foreach (var tet in mesh.tets)
            {
                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < 4; ++j)
                    {
                        var p1 = mesh.vertices[tet.tetPoints[i]];
                        var p2 = mesh.vertices[tet.tetPoints[j]];
                        Debug.DrawLine(p1.position, p2.position);
                    }
                }
            }
        }

        public static (float[], int[]) GetTriMeshDataFromTetMesh(TetMesh4D tetMesh)
        {
            float[] vertices = tetMesh.vertices.SelectMany(vertex => new float[] {
                vertex.position.x, vertex.position.y, vertex.position.z, vertex.position.w,
                0, 0, 0, 0
            }).ToArray();

            int[] tris = tetMesh.tets.SelectMany(tet => GetTrianglesFromTet(tet)).ToArray();

            return (vertices, tris);
        }

        public static Texture2D Texture2DFromRenderTexture(RenderTexture renderTexture, int texWidth, int texHeight)
        {
            RenderTexture oldRt = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Texture2D tex = new(texWidth, texHeight);
            tex.ReadPixels(new(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();
            RenderTexture.active = oldRt;
            return tex;
        }

        public static void PrintTexture(Texture2D tex, Color ignoreColour, int mipLevel = 0)
        {
            string str = "";
            for (int i = 0; i < tex.width / (1 << mipLevel); i++)
            {
                for (int j = 0; j < tex.height / (1 << mipLevel); j++)
                {
                    Color col = tex.GetPixel(i, j, mipLevel);
                    if (col != ignoreColour)
                    {
                        str += string.Format("({0}, {1}): {2}", i, j, col);
                    }
                }
                str += "\n";
            }
            Debug.Log(str);
        }
    }
}
