using System;
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

            public VertexData(Vector4 position, Vector4 normal)
            {
                this.position = position;
                this.normal = normal;
            }

            public static int SizeFloats
            {
                get => 8;
            }

            public static int SizeBytes
            {
                get => sizeof(float) * SizeFloats;
            }

            public override string ToString()
            {
                return "(pos: " + position + ", norm: " + normal + ")";
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
            //mesh.Clear();

            //// Override vertex buffer params so that position, normal take in 4D vectors
            //mesh.SetVertexBufferParams(
            //    vertices.Length,
            //    new[]
            //    {
            //    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, PTS_PER_TET),
            //    new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, PTS_PER_TET),
            //    }
            //);

            //// Set vertices, normals for the mesh
            //mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

            //// Set tetrahedra vertex indices for mesh
            //mesh.SetIndices(tets.SelectMany(tet => tet.tetPoints).ToArray(), MeshTopology.Quads, 0);

            this.vertices = vertices;
            this.tets = tets;
        }
    }

}

