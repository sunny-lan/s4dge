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
        public List<Vector4> subpoints; // sort by increasing w

        // There shouldn't be multiple points with the same w coordinate
        public Point4D(List<Vector4> points)
        {
            subpoints = points;
        }

        /// <summary>
        /// First applies transforms to the current point's subpoints
        /// Then gets the value of this point in 3D at w=w
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public Vector3 getPoint(float w, Transform4D t4d)
        {
            SortedSet<Vector4> transformedPoints = new(subpoints.Select(x => t4d.Transform(x)).ToList(), 
                Comparer<Vector4>.Create((x, y) => x.w.CompareTo(y.w))); // sort by increasing w

            Vector4 right = transformedPoints.FirstOrDefault(x => x.w > w);

            // point does not exist at this w
            // TODO: figure out the convention here Royi
            if ( transformedPoints.Count() == 0 || w > right.w || right == transformedPoints.FirstOrDefault())
            {
                return new Vector3();
            }

            Vector4 left = transformedPoints.LastOrDefault(x => x.w <= w);
            return interpolatePoint(w, left, right);
        }

        /// <summary>
        /// Interpolates this point given w.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="initialPoint"></param>
        /// <param name="finalPoint"></param>
        /// <returns></returns>
        private Vector3 interpolatePoint(float w, Vector4 initialPoint, Vector4 finalPoint)
        {
            var percent = Mathf.InverseLerp(initialPoint.w, finalPoint.w, w);
            return Vector3.LerpUnclamped(
                initialPoint.XYZ(),
                finalPoint.XYZ(),
                percent);
        }


        public void addSubPoint(Vector4 pt)
        {
            subpoints.Add(pt);
        }

        public override string ToString()
        {
            return string.Join("", subpoints.Select((point, idx) => point.ToString()));
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
         * To create a point, the format is: "<P Name>:(<x1>,<y1>,<z1>,<w1>)(<x2>,<y2>,<z2>,<w2>) <- can have arbitrary number of subpoints
         *      Note: <P Name> cannot start with 'l' or 'f'
         * To create a line, the format is: "l:<P1 Name>-<P2 Name>"
         *      Note: <P1 Name> and <P2 Name> must already be defined
         * To create a face, the format is: "f:<P1 Name>-<P2 Name>-<P3 Name>" or "f:<P1 Name>-<P2 Name>-<P3 Name>-<P4 Name>"
         *      Note: The first format creates a triangle face and the second creates a square face
         *      Note: <P1 Name> and <P2 Name> must already be defined
        */
        public InterpolationBasedShape(string fileName)
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
                    List<Vector4> subpoints = new();

                    for (int i=2; i < terms.Length; i+=2)
                    {
                        string[] pTerms = terms[i].Split(',');

                        subpoints.Add(new Vector4(
                            int.Parse(pTerms[0]),
                            int.Parse(pTerms[1]),
                            int.Parse(pTerms[2]),
                            int.Parse(pTerms[3])
                            ));
                    }

                    Point4D p4d = new Point4D(subpoints);

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