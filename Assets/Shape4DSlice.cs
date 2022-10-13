using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;  
using UnityEngine;

// TODO: 1) Add rendering of squares

class Point4D{
    public Vector4 initialPoint;
    public Vector4 finalPoint;
    public Point4D(Vector4 p1Var, Vector4 p2Var){
        initialPoint = p1Var;
        finalPoint = p2Var;
    }

    /// <summary>
    /// Interpolates this point given w.
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public Vector3 getPoint( float w)
    {
        var percent = Mathf.InverseLerp(initialPoint.w, finalPoint.w, w);
        /*if (percent<0 || percent>1)
        {
            return null; //point out of range
        }*/
        return Vector3.Lerp(
            initialPoint.XYZ(), 
            finalPoint.XYZ(), 
            percent);
    }
}

class Line4D{
    public Point4D p1; // initial point
    public Point4D p2; // ending point
    public Line4D(Point4D p1Var, Point4D p2Var){
        p1 = p1Var;
        p2 = p2Var;
    }
}

class Face4D{
    public List<Point4D> points;
    public Face4D(List<Point4D> points){
        this.points = points;
    }
}


class Shape4DSlice : Shape4D
{
    private List<Line4D> lines4D = new List<Line4D>(){};
    private List<Face4D> faces4D = new List<Face4D>(){};


    //public class Shape4DSlice(Render4D r4) : base(r4) {}

    /**
     * File Formatting:
     * Each line in the file defines either a Point, Line, or Face
     * To create a point, the format is: "<P Name>:(<x1>,<y1>,<z1>,<w1>)(<x2>,<y2>,<z2>,<w2>)
     *      Note: <P Name> cannot start with 'l' or 'f'
     * To create a line, the format is: "l:<P1 Name>-<P2 Name>"
     *      Note: <P1 Name> and <P2 Name> must already be defined
     * To create a face, the format is: "f:<P1 Name>-<P2 Name>-<P3 Name>" or "f:<P1 Name>-<P2 Name>-<P3 Name>-<P4 Name>"
     *      Note: The first format creates a triangle face and the second creates a square face
     *      Note: <P1 Name> and <P2 Name> must already be defined
    */
    public Shape4DSlice(Render4D r4, string fileName) : base(r4) {
        Dictionary<string, Point4D> points = new Dictionary<string, Point4D>();

        // Static file name (for now)
        string[] fileLines = File.ReadAllLines(fileName);

        foreach (string fileLine in fileLines) { 
            if(fileLine[0] == 'l'){ // check that it is line
                string[] terms = fileLine.Split(':', '-');
                string lineP1 = terms[1];
                string lineP2 = terms[2];
                
                Line4D line = new Line4D(points[lineP1], points[lineP2]); // get rereference to points and then create lines
                
                lines4D.Add(line);
                
            } else if (fileLine[0] == 'f') {
                string[] terms = fileLine.Split(':', '-');
                
                List<Point4D> facePoints = new List<Point4D>(); // optimization maybe possible lol
                foreach (string term in terms.Skip(1).ToArray()) { // Adds points to the face
                    facePoints.Add(points[term]);
                }

                faces4D.Add(new Face4D(facePoints));
            } else {
                string[] terms = fileLine.Split( ':', '(', ')');
                string pName = terms[0];
                string[] p1Terms = terms[2].Split(',');
                string[] p2Terms = terms[4].Split(',');

                //Convert p1 & p2 to vector4
                Vector4 p1 = new Vector4();
                Vector4 p2 = new Vector4();
                p1[0] = int.Parse(p1Terms[0]);
                p1[1] = int.Parse(p1Terms[1]);
                p1[2] = int.Parse(p1Terms[2]);
                p1[3] = int.Parse(p1Terms[3]);

                p2[0] = int.Parse(p2Terms[0]);
                p2[1] = int.Parse(p2Terms[1]);
                p2[2] = int.Parse(p2Terms[2]);
                p2[3] = int.Parse(p2Terms[3]);
    
                Point4D p4d = new Point4D(p1, p2);
                
                points.Add(pName, p4d);
            }
        }

        float w = 0.0f;
        //Debugging
        if (true) {
            Debug.Log("Lines");
            foreach (Line4D line in lines4D) {
                Debug.Log(getPoint(line.p1.initialPoint, line.p1.finalPoint, w));
                Debug.Log(getPoint(line.p2.initialPoint, line.p2.finalPoint, w));
            }
            Debug.Log("Faces");
            foreach (Face4D face in faces4D) {
                Debug.Log(getPoint(face.points[0].initialPoint, face.points[0].finalPoint, w));
                Debug.Log(getPoint(face.points[1].initialPoint, face.points[1].finalPoint, w));
                Debug.Log(getPoint(face.points[2].initialPoint, face.points[2].finalPoint, w));
                Debug.Log(getPoint(face.points[3].initialPoint, face.points[3].finalPoint, w));
            }
        }
    }


    // Lines and points // TODO: Remove Boon
    //protected List<Point4D> points4D = new List<Point4D>(){};
    //protected List<Point4D> lines4D = new List<Point4D>(){};


    protected override void getPoints(/*int w*/)
    {

    }
    
    // helper to calculate all points
    // Pass in p1, p2, w, calculate location of current point given w
    // p1 initial point, p2 final point
    // [x,y,z,w]
    private Vector3 getPoint(Vector4 p1, Vector4 p2, float w){
        return Vector3.Lerp(p1.XYZ(), p2.XYZ(), Mathf.InverseLerp(p1.w, p2.w, w));
    }

    protected override void getLines()
    {

    }


    // Iterates over all lines
    // Draws points between them
    // Draws all lines
    // Also draws faces??? TODO:// Ask someone about this
    override public void drawLines(float w){

        // lines
        // Debug.Log("Lines");
        foreach(Line4D line in lines4D){ // iterate over every line 

            // Find point location given w
            // for each line, get each point from their start and end location and w
            Vector3 p1 = line.p1.getPoint(w);
            Vector3 p2 = line.p2.getPoint(w);
            render.line(p1, p2); // draw line
        }
        
        // faces
        // Debug.Log("Faces");
        foreach(Face4D face in faces4D){ // face is an array of faces
                                         // Limit to drawing triangles and squares

            // generate calculated points
            //  1. Select = for each point x apply x.getPoint(w)
            //  3. Pass all calculated points to drawPolygon
            var slicedPoints = face.points
                .Select(x => x.getPoint(w))
                .ToArray();
            render.drawPolygon(slicedPoints);
        }
    }



    override public void drawLines()
    {
        /*
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
*/
    }
}
