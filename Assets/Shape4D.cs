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
    }

    protected abstract void getFaces();

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

    public void drawLines()
    {
        foreach (var l in lines) 
            render.line(rotate(l.a), rotate(l.b));
    }

    public abstract void fillFaces();
}