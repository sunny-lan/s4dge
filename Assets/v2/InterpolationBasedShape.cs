using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;  
using UnityEngine;
using System;
using System.Text;

// TODO: 1) Add rendering of squares
namespace v2
{
    [Serializable]
    public class Point4D
    {
        public Vector4 initialPoint;
        public Vector4 finalPoint;
        public Point4D(Vector4 p1Var, Vector4 p2Var)
        {
            initialPoint = p1Var;
            finalPoint = p2Var;
        }

        /// <summary>
        /// Interpolates this point given w.
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public Vector3 getPoint(float w)
        {
            var percent = Mathf.InverseLerp(initialPoint.w, finalPoint.w, w);
            return Vector3.LerpUnclamped(
                initialPoint.XYZ(),
                finalPoint.XYZ(),
                percent);
        }

        public override string ToString()
        {
            return initialPoint.ToString() + finalPoint.ToString();
        }
    }

    [Serializable]
    public class Line4D
    {
        public Point4D p1; // initial point
        public Point4D p2; // ending point
        public Line4D(Point4D p1Var, Point4D p2Var)
        {
            p1 = p1Var;
            p2 = p2Var;
        }

        public override string ToString()
        {
            return p1.ToString() + "-" + p2.ToString();
        }
    }

    [Serializable]
    public class Face4D
    {
        public List<Point4D> points;
        public Face4D(List<Point4D> points)
        {
            this.points = points;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Point4D p in points)
            {
                sb.Append(p.ToString());
            }
            return sb.ToString();
        }
    }


    /// <summary>
    /// Shape based on interpolating between two points by w
    /// </summary>
    [CreateAssetMenu]
    public class InterpolationBasedShape : ScriptableObject
    {
        public List<Line4D> lines4D = new List<Line4D>() { };
        public List<Face4D> faces4D = new List<Face4D>() { };
        public Dictionary<string, Point4D> points = new Dictionary<string, Point4D>() { };


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
        public InterpolationBasedShape( string fileName)
        {

            // Static file name (for now)
            string[] fileLines = File.ReadAllLines(fileName);

            foreach (string fileLine in fileLines)
            {
                if (fileLine[0] == 'l')
                { // check that it is line
                    string[] terms = fileLine.Split(':', '-');
                    string lineP1 = terms[1];
                    string lineP2 = terms[2];

                    Line4D line = new Line4D(points[lineP1], points[lineP2]); // get rereference to points and then create lines

                    lines4D.Add(line);

                }
                else if (fileLine[0] == 'f')
                {
                    string[] terms = fileLine.Split(':', '-');

                    List<Point4D> facePoints = new List<Point4D>(); // optimization maybe possible lol
                    foreach (string term in terms.Skip(1).ToArray())
                    { // Adds points to the face
                        facePoints.Add(points[term]);
                    }

                    faces4D.Add(new Face4D(facePoints));
                }
                else
                {
                    string[] terms = fileLine.Split(':', '(', ')');
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
        }


        // Lines and points // TODO: Remove Boon
        //protected List<Point4D> points4D = new List<Point4D>(){};
        //protected List<Point4D> lines4D = new List<Point4D>(){};

        // writes shape information to file in shape4D file format
        public void toFile(string fileName)
        {
            using (StreamWriter sw = File.CreateText(fileName))
            {
                foreach (KeyValuePair<string, Point4D> point in points)
                {
                    sw.WriteLine(string.Format("{0}:{1}",
                        point.Key, 
                        point.Value
                        ));
                }

                foreach(Line4D line in lines4D)
                {
                    sw.WriteLine(string.Format("l:{1}", line));
                }

                foreach(Face4D face in faces4D)
                {
                    sw.WriteLine(string.Format("f:{1}", face));
                }
            }
        }

    }

}