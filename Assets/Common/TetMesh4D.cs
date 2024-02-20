using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RasterizationRenderer
{

    public class TetMesh4D
    {
        public static readonly int PTS_PER_TET = 4;
        public VertexData[] vertices
        {
            get; private set;
        }

        public Tet4D[] tets
        {
            get; private set;
        }

        // Make sure struct in passed in correct layout to the mesh vertex buffer
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        [Serializable]
        public struct VertexData
        {
            [SerializeField]
            public Vector4 position;

            [SerializeField]
            public Vector4 normal;

            [SerializeField]
            public Vector4 colour;

            // world position without 3D projection for lighting calculations
            [SerializeField]
            public Vector4 worldPosition4D;

            public VertexData(Vector4 position):this(position, Vector4.zero)
            {
            }

            public VertexData(Vector4 position, Vector4 normal):this(position,normal,Vector4.zero)
            {
			}

            public VertexData(Vector4 position, Vector4 normal, Vector4 worldPosition4D):this(position,normal,worldPosition4D, Vector4.zero)
            {
			}

			public VertexData(Vector4 position, Vector4 normal, Vector4 worldPosition4D, Vector4 colour)
			{
				this.position = position;
				this.normal = normal;
				this.worldPosition4D = worldPosition4D;
                this.colour = colour;
            }

            public static int SizeBytes = Marshal.SizeOf<VertexData>();
            public static int SizeFloats = SizeBytes / sizeof(float);

            public static VertexData[] ReadFromFloatArr(float[] arr)
            {
                List<VertexData> ret = new();
                for (int i = 0; i < arr.Length; i += SizeBytes)
                {
                    ret.Add(new(
                        new(arr[i], arr[i + 1], arr[i + 2], arr[i + 3]),
                        new(arr[i + 4], arr[i + 5], arr[i + 6], arr[i + 7]),
                        new(arr[i + 8], arr[i + 9], arr[i + 10], arr[i + 11]),
						new(arr[i + 12], arr[i + 13], arr[i + 14], arr[i + 15])
					));
                }
                return ret.ToArray();
            }

            public override string ToString()
            {
                return "(pos: " + position + ", norm: " + normal + ", worldPos: " + worldPosition4D + ", colour: " + colour + ")";
                //return "(" + position + ")";
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        [Serializable]
        public struct Tet4D
        {
            [SerializeField]
            public int[] tetPoints; // points to indices in Vector4 points array

            public Tet4D(int[] tetPoints)
            {
                this.tetPoints = tetPoints;
            }

            public override string ToString()
            {
                return "(" + string.Join(",", tetPoints) + ")";
            }
        }

        // Updates the mesh based on the vertices, tetrahedra
        public TetMesh4D(VertexData[] vertices, Tet4D[] tets)
        {

            //MeshGeneratorUtils.MakeTetsForwardFacing(tets, vertices);
            this.vertices = vertices;
            this.tets = tets;
        }

        public void AppendTets(VertexData[] vertices, Tet4D[] tets)
        {
            int curVertexCount = this.vertices.Length;
            this.vertices = this.vertices.Concat(vertices).ToArray();

            // adds curVertexCount to each tet index
            this.tets = this.tets.Concat(
                tets.Select(tet => new Tet4D(
                    tet.tetPoints.Select(idx => idx + curVertexCount).ToArray()
                ))
            ).ToArray();
        }

        public override string ToString()
        {
            return "Vertices:" + string.Join(",", vertices) + "\nTets:" + string.Join(",", tets);
        }
    }

}

