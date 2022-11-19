using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Helper class to store 3D meshes and modify them
/// </summary>
public class Geometry3D
{
    public readonly List<PointInfo> vertices = new();
    public readonly List<int> triangles = new();
    public readonly List<(Vector3,Vector3)> lines = new();

    public void Clear()
    {
        vertices.Clear();
        triangles.Clear();
        lines.Clear();
    }

    /// <summary>
    /// Draws polygon out of triangles
    /// </summary>
    /// <param name="polygon">The polygon to draw</param>
    public void fillPolygon(params PointInfo[] polygon)
    {
        int tmp = vertices.Count;
        this.vertices.AddRange(polygon);
        

        for (int i = 1; i + 1 < polygon.Length; i++)
        {
            //front face
            triangles.Add(tmp);
            triangles.Add(tmp + i);
            triangles.Add(tmp + (i + 1));

            //back face
            triangles.Add(tmp + (i + 1));
            triangles.Add(tmp + i);
            triangles.Add(tmp);
        }
    }

    public void line(Vector3 p1, Vector3 p2)
    {
        lines.Add((p1, p2));
    }
}
