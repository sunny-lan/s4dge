using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class Cube : Shape4D
{
    protected override void getLines()
    {
        foreach (var p in points)
        {
            foreach (var q in points)
            {
                // draw a line between points if they differ in exactly one coordinate
                if (Mathf.Round((p - q).sqrMagnitude) == 1)
                {
                    lines.Add((p, q));
                }
            }
        }
    }

    protected override void getPoints()
    {
        for (int i = 0; i < (1 << 4); i++)
        {
            points.Add(new(i & 1, (i >> 1) & 1, (i >> 2) & 1, (i >> 3) & 1));
        }
    }

    public Cube(Render4D r4) : base(r4)
    {
    }

}