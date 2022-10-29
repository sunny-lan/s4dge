using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using System;
using System.Text;
using System.Drawing;
using System.Reflection;

// TODO: 1) Add rendering of squares
namespace v2
{
    namespace interpolatev2
    {
        [Serializable]
        public class Point3D : Point4D
        {
            public Point4D to4D(float w)
            {
                Point4D ret = new Point4D(point, name);
                ret.point.w = w;
                return ret;
            }

            public Vector3 getPoint()
            {
                return point.XYZ();
            }

            public Point3D(Vector3 pt, string pName) : base(pt.withW(0), pName) { }

            // requires: len(pTerms) == 3
            public Point3D(string[] pTerms, string pName) : base(new Vector4(
                    int.Parse(pTerms[0]), 
                    int.Parse(pTerms[1]), 
                    int.Parse(pTerms[2]),
                    0
                ), pName) { }

            public override string ToString()
            {
                return string.Format("{0}:{1}", name, point.ToString());
            }
        }

        [Serializable]
        public class Line<T>
        {
            public T initialPoint, finalPoint;

            public Line(T initialPoint, T finalPoint)
            {
                this.initialPoint = initialPoint;
                this.finalPoint = finalPoint;
            }

            public override string ToString()
            {
                return initialPoint.ToString() + "-" + finalPoint.ToString();
            }
        }

        [Serializable]
        public class Face<T>
        {
            public List<T> points;
            public Face(List<T> points)
            {
                this.points = points;
            }

            public override string ToString()
            {
                return string.Join('-', points.Select(x => x.ToString()));
            }
        }

        [Serializable]
        public class Shape3D
        {
            public Dictionary<string, Point3D> points;
            public List<Line<Point3D>> lines;
            public List<Face<Point3D>> faces;

            /**
             * File Formatting:
             * Each line in the file defines either a Point, Line, or Face
             * To create a point, the format is: "<P Name>:(<x1>,<y1>,<z1>)"
             *      Note: <P Name> cannot start with 'l' or 'f'
             * To create a line, the format is: "l:<P1 Name>-<P2 Name>"
             *      Note: <P1 Name> and <P2 Name> must already be defined
             * To create a face, the format is: "f:<P1 Name>-<P2 Name>-<P3 Name>" or "f:<P1 Name>-<P2 Name>-<P3 Name>-<P4 Name>"
             *      Note: The first format creates a triangle face and the second creates a square face
             *      Note: <P1 Name> and <P2 Name> must already be defined
            */
            public Shape3D(string fileName)
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

                        Line<Point3D> line = new Line<Point3D>(points[lineP1], points[lineP2]); // get rereference to points and then create lines

                        lines.Add(line);

                    }
                    else if (fileLine[0] == 'f')
                    {
                        string[] terms = fileLine.Split(':', '-');

                        List<Point3D> facePoints = new List<Point3D>(); // optimization maybe possible lol
                        foreach (string term in terms.Skip(1).ToArray())
                        { // Adds points to the face
                            facePoints.Add(points[term]);
                        }

                        faces.Add(new Face<Point3D>(facePoints));
                    }
                    else
                    {
                        string[] terms = fileLine.Split(':', '(', ')');
                        string pName = terms[0];

                        string[] pTerms = terms[2].Split(',');

                        points.Add(pName, new Point3D(pTerms, pName));
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
                    foreach (KeyValuePair<string, Point3D> point in points)
                    {
                        sw.WriteLine(string.Format("{0}:{1}",
                            point.Key,
                            point.Value
                            ));
                    }

                    foreach (Line<Point3D> line in lines)
                    {
                        sw.WriteLine(string.Format("l:{1}", line));
                    }

                    foreach (Face<Point3D> face in faces)
                    {
                        sw.WriteLine(string.Format("f:{1}", face));
                    }
                }
            }
        }

        public class Point4D : IComparable<Point4D> {
            public Vector4 point;
            public string name;

            public Point4D(Vector4 point, string pName)
            {
                this.point = point;
                name = pName;
            }

            public override string ToString()
            {
                return string.Format("{0}:{1}", name, point.ToString());
            }

            public int CompareTo(Point4D other)
            {
                return point.w.CompareTo(other.point.w);
            }


        }

        /// <summary>
        /// Shape based on interpolating between two points by w
        /// </summary>
        [Serializable]
        [CreateAssetMenu]
        public class InterpolationBasedShapeModel : ScriptableObject
        {
            public SortedList<float, Shape3D> slices = new();

            /**
             * File Formatting:
            */
            public InterpolationBasedShapeModel(string fileName)
            {

            }

            // writes shape information to file in shape4D file format
            public void toFile(string fileName)
            {
                using (StreamWriter sw = File.CreateText(fileName))
                {
                }
            }

            public void addSlice(float w, Shape3D shape)
            {
                slices.Add(w, shape);
            }

            public void removeSlice(float w)
            {
                slices.Remove(w);
            }

            // Converts 3D-shape + w-coordinate model to an actual shape with points, lines and faces
            public InterpolationBasedShapev2 toInterpolationBasedShape()
            {
                // add existing points/lines/faces
                Dictionary<string, Point4D> points = new();
                List<Line<Point4D>> lines = new();
                List<Face<Point4D>> faces = new();
                foreach (var(w, shape) in slices)
                {
                    // converts 3D points to 4D
                    // append 'w' coordinate to point name so that there are no duplicate keys
                    // point name remains the same
                    points.Concat(
                        shape.points.Select(kvp => (name: kvp.Key, point4d: kvp.Value.to4D(w))).
                            ToDictionary(kvp => kvp.name + w.ToString(), kvp => kvp.point4d)
                    );

                    // add current shape's lines
                    lines.Concat(
                        shape.lines.Select(line3D =>
                        {
                            // lines refer to just-created point4Ds
                            return new Line<Point4D>(points[line3D.initialPoint.name + w], points[line3D.finalPoint.name + w]);
                        }
                        ).ToList()
                    );

                    // add current shape's faces
                    faces.Concat(
                        shape.faces.Select(face =>
                            // faces refer to just-created point4Ds
                            new Face<Point4D>(face.points.Select(pt => points[pt.name + w]).ToList())
                        ).ToList()
                    );
                }

                // TODO:add lines/faces across w coordinate if needed Royi

                // Add interpolation lines (analogous to Point4D in InterpolationBaseShapeV1), a set of 3D points with different w
                // given w, we interpolate between the two points on the interpolation line that surround w
                // for now, each interpolation line just connects all points with same name.
                Dictionary<string, List<Point4D>> interpolationLines = new();
                foreach (Point4D pt in points.Values)
                {
                    interpolationLines[pt.name].Add(pt);
                }

                return new InterpolationBasedShapev2(points, lines, faces, interpolationLines.Values.ToList());
            }

        }

        [Serializable]
        public class InterpolationBasedShapev2
        {
            public Dictionary<string, Point4D> points = new Dictionary<string, Point4D>() { };
            public List<Line<Point4D>> lines4D = new List<Line<Point4D>>() { };
            public List<Face<Point4D>> faces4D = new List<Face<Point4D>>() { };
            List<List<Point4D>> interpolationLines = new();

            public InterpolationBasedShapev2(Dictionary<string, Point4D> points, List<Line<Point4D>> lines4D, List<Face<Point4D>> faces4D, List<List<Point4D>> interpolationLines)
            {
                this.points = points;
                this.lines4D = lines4D;
                this.faces4D = faces4D;
                this.interpolationLines = interpolationLines;
            }

            // applies transform to each point in the shape
            public void transform(Transform4D t4d)
            {
                foreach((_, Point4D pt) in points)
                {
                    pt.point = t4d.Transform(pt.point);
                }
            }

            // uses interpolation to get the slice of the shape at w=w
            public Shape3D getSlice(float w)
            {
                // for each interpolation line, find the point that falls on the desired w
                foreach(List<Point4D> interpLine in interpolationLines)
                {
                    // sort list by w
                    // TODO: may need to optimize if this runs too slowly Royi
                    interpLine.Sort();
                }
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
        }
    }
}