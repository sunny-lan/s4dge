using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class FiveCell : Shape4D
{
    private float goldenRatio = (1 + Mathf.Sqrt(5f)) / 2.0f ;

    public FiveCell(Render4D r4) : base(r4)
    {
        for (int i = 0; i < 4; i++)
        {
            Vector4 p = new Vector4(0,0,0,0);
            p[i] = 2;
            points.Add(p);
            adj[points[i]] = new();
        }
        points.Add( new( goldenRatio, goldenRatio, goldenRatio, goldenRatio ) );
        adj[points[4]] = new();

        // add lines
        foreach (var p in points)
        {
            foreach (var q in points) 
            {
                // draw a line between points if their distance is 8
                if ( Mathf.Abs(Mathf.Abs((p - q).sqrMagnitude) - 8.0f) <= 0.1)
                {
                    lines.Add((p, q));
                    adj[p].Add(q);
                }
            }
        }
        getFaces();
    }

    public override void fillFaces()
    {
        foreach(var face in faces)
            render.drawTriangle(face.Select(rotate).ToArray());
    }

    protected override void getFaces()
    {
        for (int i=0;i<points.Count;i++)
        {
            Vector4 p_i = points[i];
            for (int j = i + 1; j < points.Count; j++)
            {
                Vector4 p_j = points[j];
                if (!adj[p_i].Contains(p_j)) continue; // check if the first two lines connect
                for (int k = i + 1; k < points.Count; k++)
                {
                    Vector4 p_k = points[k];
                    if (!adj[p_j].Contains(p_k) || !adj[p_i].Contains(p_k)) continue; // 3rd point adjacent to both
                    faces.Add(new Vector4[] { p_i, p_j, p_k });
                }
            }
        }
    }

}