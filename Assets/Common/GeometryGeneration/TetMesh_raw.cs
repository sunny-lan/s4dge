using RasterizationRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TetMesh_raw
{
    [SerializeField]
    public List<TetMesh4D.VertexData> vertices = new();

    [SerializeField]
    public List<TetMesh4D.Tet4D> tets = new();

    public void Append(IEnumerable<Vector4> tet)
    {
        tets.Add(new()
        {
            tetPoints = Enumerable.Range(vertices.Count, vertices.Count + 4).ToArray()
        });

        vertices.AddRange(tet.Select(p => new TetMesh4D.VertexData()
        {
            position = p
        }));
    }

    public TetMesh4D ToTetMesh()
    {
        return new TetMesh4D(vertices.ToArray(), tets.ToArray());
    }
}