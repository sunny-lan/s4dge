using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class FiveCell : Shape4D
{
    private float goldenRatio = (1 + Mathf.Sqrt(5f)) / 2.0f ;

    protected override void getPoints()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector4 p = new Vector4(0, 0, 0, 0);
            p[i] = 2;
            points.Add(p);
        }
        points.Add(new(goldenRatio, goldenRatio, goldenRatio, goldenRatio));
    }

    protected override void getLines()
    {
        // add lines
        foreach (var p in points)
        {
            foreach (var q in points)
            {
                // draw a line between points if their distance is 8
                if (Mathf.Abs(Mathf.Abs((p - q).sqrMagnitude) - 8.0f) <= 0.1)
                {
                    lines.Add((p, q));
                }
            }
        }
    }

    public FiveCell(Render4D r4) : base(r4) { }
}