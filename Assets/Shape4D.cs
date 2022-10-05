using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Shape4D {
    protected Render4D render;
    protected List<Vector4> points = new List<Vector4>(){};
    protected HashSet<(Vector4 a, Vector4 b)> lines= new();
    protected Dictionary<Vector4, HashSet<Vector4>> adj = new();
    protected List<Vector4[]> faces = new();

    public bool useAxialRotations = false;
    public float rotation;
    public float[] allRotations = new float[6];

    public Shape4D(Render4D r4)
    {
        this.render = r4;

        getPoints();
        foreach (var p in this.points)
            adj[p] = new();

        getLines();
        foreach(var l in this.lines)
        {
            adj[l.a].Add(l.b);
            adj[l.b].Add(l.a);
        }

        findSquares();
        findTriangles();
    }

    protected abstract void getPoints();
    protected abstract void getLines();

    // applies all x axis of rotation to vector
    protected Vector4 AxialRotations( Vector4 vertex ) {
        Vector4 r = render.Vector4DeepCopy( vertex );
        int axisCount = 0;
        for ( int i = 0; i < 3; ++i )
        {
            for ( int j = i + 1; j < 4; ++j )
            {
                r = render.rotate( r, i, j, allRotations[ axisCount ] );
                axisCount++;
            }
        }
        return r;
    }

    protected Vector4 rotate(Vector4 vertex)
    {
        if ( useAxialRotations )
        {
            return AxialRotations( vertex );
        }
        else
        {
            return render.rotate(render.rotate(vertex, 1, 3, rotation),2,0,rotation*2);
        }
    }

    public virtual void drawLines()
    {
        foreach (var l in lines) 
            render.line(rotate(l.a), rotate(l.b));
    }

    public virtual void fillFaces()
    {
        foreach (var f in faces)
        {
            var f_transformed = f.Select(rotate).ToArray();
            if (f.Length == 3)
                render.drawTriangle(f_transformed);
            else if (f.Length == 4)
                render.drawSquare(f_transformed);
            else
                throw new System.NotImplementedException();
        }
    }

    void findTriangles()
    {
        for (int i = 0; i < points.Count; i++)
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

    void findSquares()
    {
        for (int i = 0; i < points.Count; i++)
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