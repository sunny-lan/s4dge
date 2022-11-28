using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace v2
{
    /// <summary>
    /// Helper class to store 3D meshes and modify them
    /// </summary>
    public class Geometry3D
    {

        public readonly List<Vector3> vertices = new();
        public readonly List<Vector2> uv = new();
        public readonly List<Vector2> uv2 = new();
        public readonly List<int> triangles = new();
        public readonly List<(int, int)> lines = new();

        public void Clear()
        {
            vertices.Clear();
            uv.Clear();
            uv2.Clear();
            triangles.Clear();
            lines.Clear();
        }

        public int AddPoint(PointInfo point)
        {
            vertices.Add(point.position);
            uv.Add(point.uv);
            uv2.Add(new(point.w, 0));
            return vertices.Count - 1;
        }

        /// <summary>
        /// Draws polygon out of triangles
        /// </summary>
        /// <param name="polygon">The vertex indices of polygon to draw</param>
        public void fillPolygon(params int[] polygon)
        {
            for (int i = 1; i + 1 < polygon.Length; i++)
            {
                //front face
                triangles.Add(polygon[0]);
                triangles.Add(polygon[i]);
                triangles.Add(polygon[i+1]);
            }
        }

        public void line(int p1, int p2)
        {
            lines.Add((p1, p2));
        }

        public void ApplyToMesh(Mesh mesh)
        {
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetUVs(1, uv2); 
            mesh.SetTriangles(triangles, 0);
        }
    }
}