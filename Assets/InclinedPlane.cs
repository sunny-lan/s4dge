using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class InclinedPlane : Shape4D
{
    // int w = 5; // hard code variable for 4th plane

    
    protected override void getPoints(/*int w*/)
    {
        //TODO: Min max bounds
        // Vector3 p1 = new Vector3(w,0,0);
        // Vector3 p2 = new Vector3(w+5,0,0);
        // Vector3 p3 = new Vector3(w+5,0,w+5);
        // Vector3 p4 = new Vector3(w,5,0);
        // Vector3 p5 = new Vector3(w,5,w+5);
        // Vector3 p6 = new Vector3(w+5,5,w+5);

        // points.Add(p1);
        // points.Add(p2);
        // points.Add(p3);
        // points.Add(p4);
        // points.Add(p5);
        // points.Add(p6);
    }

    protected override void getLines()
    {
        // //TODO: Min max bounds (or use the interpolate between 2 points)
        // Vector3 p1 = new Vector3(w      ,10     ,0);  // 0, 10, 0
        // Vector3 p2 = new Vector3(w+5    ,10     ,0);  // 5, 10, 0
        // Vector3 p3 = new Vector3(w+5    ,10     ,w+5);// 5, 10, 5
        // Vector3 p4 = new Vector3(w      ,15     ,0);  // 0, 15, 0
        // Vector3 p5 = new Vector3(w+5    ,15     ,w);  // 5, 15, 0
        // Vector3 p6 = new Vector3(w+5    ,15     ,w+5);// 5, 15, 5
        // //p1 = points[0]

        // // front triangle
        // render.line(p1, p2);
        // render.line(p2, p3);
        // render.line(p3, p1);
        // // //backtriangle
        // render.line(p4, p5);
        // render.line(p5, p6);
        // render.line(p6, p4);
        // // // connectors
        // render.line(p1, p4);
        // render.line(p2, p5);
        // render.line(p3, p6);
    }

    public InclinedPlane(Render4D r4) : base(r4) { }

    override public void drawLines() 
    {
        Vector3 p1 = new Vector3(w      ,10     ,0);  // 0, 10, 0
        Vector3 p2 = new Vector3(w+5    ,10     ,0);  // 5, 10, 0
        Vector3 p3 = new Vector3(w+5    ,10     ,w+5);// 5, 10, 5
        Vector3 p4 = new Vector3(w      ,15     ,0);  // 0, 15, 0
        Vector3 p5 = new Vector3(w+5    ,15     ,0);  // 5, 15, 0
        Vector3 p6 = new Vector3(w+5    ,15     ,w+5);// 5, 15, 5
        Vector3 p7 = new Vector3(w      ,10     ,w);  // 0, 10, 0
        Vector3 p8 = new Vector3(w      ,15     ,w);  // 5, 10, 0
        
        render.drawSquare(p1, p2, p3, p7); // front triangle-trapezoid
        render.drawSquare(p4, p5, p6, p8); // back triangle-trapezoid
        render.drawSquare(p1, p2, p5, p4); // bottom
        render.drawSquare(p2, p3, p6, p5); // back 
        // render.drawSquare(p3, p1, p4, p6); // angle

/*
        // front triangle
        render.line(p1, p2);
        render.line(p2, p3);
        render.line(p3, p7);
        // //backtriangle
        render.line(p4, p5);
        render.line(p5, p6);
        render.line(p6, p4);
        // // connectors
        render.line(p1, p4);
        render.line(p2, p5);
        render.line(p3, p6);
        // // other connectors
        render.line(p1, p7);
        render.line(p7, p8);
        render.line(p8, p2);
        */

        // front trapezoid
        render.line(p1, p2);
        render.line(p2, p3);
        render.line(p3, p7);
        render.line(p7, p1);

        // back trapezoid);
        render.line(p4, p5);
        render.line(p5, p6);
        render.line(p6, p8);
        render.line(p8, p4);

        // connectors);
        render.line(p1, p4);
        render.line(p2, p5);
        render.line(p3, p6);
        render.line(p7, p8);

    }
}