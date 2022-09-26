using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class Cube : Shape4D
{
    public Cube(Render4D r4) : base(r4)
    {
        for (int i = 0; i < (1 << 4); i++)
        {
            points.Add(new(i & 1, (i >> 1) & 1, (i >> 2) & 1, (i >> 3) & 1));
            adj[points[i]] = new();
        }

        // add lines
        foreach (var p in points)
        {
            foreach (var q in points) 
            {
                // draw a line between points if they differ in exactly one coordinate
                if (Mathf.Round((p - q).sqrMagnitude) == 1)
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
            render.drawSquare(face.Select(rotate).ToArray());
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
                    if (!adj[p_j].Contains(p_k)) continue; // 2nd pair of lines, ...
                    for (int l = i + 1; l < points.Count; l++)
                    {
                        Vector4 p_l = points[l];
                        if (!adj[p_k].Contains(p_l) || !adj[p_l].Contains(p_i)) continue;

                        faces.Add(new Vector4[] { p_i, p_j, p_k, p_l });
                    }
                }
            }
        }
    }

}