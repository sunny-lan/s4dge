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
    public class InterpolationPoint4D
    {
        public List<Vector4> subpoints; // sort by increasing w

        // There shouldn't be multiple points with the same w coordinate
        public InterpolationPoint4D(List<Vector4> points)
        {
            subpoints = points;
        }

        public void AddSubpoint(Vector4 subpoint)
        {
            subpoints.Add(subpoint);
        }

        // TODO: may need optimizing Royi
        public void RemoveSubpoint(float w)
        {
            subpoints.RemoveAll(subpt => subpt.w == w);
        }

        /// <summary>
        /// First applies transforms to the current point's subpoints
        /// Then gets the value of this point in 3D at w=w
        /// TODO: may need optimizing Royi
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public Vector3 GetPoint(float w, Func<Vector4, Vector4> transform)
        {
            SortedSet<Vector4> transformedPoints = new(subpoints.Select(transform).ToList(), 
                Comparer<Vector4>.Create((x, y) => x.w.CompareTo(y.w))); // sort by increasing w

            Vector4 right = transformedPoints.FirstOrDefault(x => x.w > w);

            // point does not exist at this w
            // TODO: figure out the convention here Royi
            if ( transformedPoints.Count() == 0 || w > right.w || right == transformedPoints.FirstOrDefault())
            {
                return new Vector3();
            }

            Vector4 left = transformedPoints.LastOrDefault(x => x.w <= w);
            return InterpolatePoint(w, left, right);
        }

        /// <summary>
        /// Interpolates this point given w.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="initialPoint"></param>
        /// <param name="finalPoint"></param>
        /// <returns></returns>
        private Vector3 InterpolatePoint(float w, Vector4 initialPoint, Vector4 finalPoint)
        {
            var percent = Mathf.InverseLerp(initialPoint.w, finalPoint.w, w);
            return Vector3.LerpUnclamped(
                initialPoint.XYZ(),
                finalPoint.XYZ(),
                percent);
        }

        public override string ToString()
        {
            return string.Join("", subpoints.Select((point, idx) => point.ToString()));
        }
    }


    [Serializable]
    public class Line<T>
    {
        public T p1, p2; // initial, final points

        public Line(T initialPoint, T finalPoint)
        {
            this.p1 = initialPoint;
            this.p2 = finalPoint;
        }

        public override string ToString()
        {
            return p1.ToString() + "-" + p2.ToString();
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

    /// <summary>
    /// Shape based on interpolating between two points by w
    /// </summary>
    [CreateAssetMenu]
    public class InterpolationBasedShape : ScriptableObject
    {
        public List<Line<InterpolationPoint4D>> lines4D = new List<Line<InterpolationPoint4D>>() { };
        public List<Face<InterpolationPoint4D>> faces4D = new List<Face<InterpolationPoint4D>>() { };
        public Dictionary<string, InterpolationPoint4D> points = new Dictionary<string, InterpolationPoint4D>() { };
        HashSet<float> sliceW = new();


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

                // Remove comments
                string currentFileLine = fileLine;
                int index = currentFileLine.IndexOf("#");
                if (index >= 0){
                    currentFileLine = currentFileLine.Substring(0, index);
                }
                currentFileLine = currentFileLine.Trim(); // Trim leading and ending white space

                if(currentFileLine == ""){
                    continue;
                }


                if (currentFileLine[0] == 'l')
                { // check that it is line
                    string[] terms = currentFileLine.Split(':', '-');
                    string lineP1 = terms[1];
                    string lineP2 = terms[2];

                    Line<InterpolationPoint4D> line = new Line<InterpolationPoint4D>(points[lineP1], points[lineP2]); // get rereference to points and then create lines

                    lines4D.Add(line);

                }
                else if (currentFileLine[0] == 'f')
                {
                    string[] terms = currentFileLine.Split(':', '-');

                    List<InterpolationPoint4D> facePoints = new List<InterpolationPoint4D>(); // optimization maybe possible lol
                    foreach (string term in terms.Skip(1).ToArray())
                    { // Adds points to the face
                        facePoints.Add(points[term]);
                    }

                    faces4D.Add(new Face<InterpolationPoint4D>(facePoints));
                }
                else
                {
                    string[] terms = currentFileLine.Split(':', '(', ')');
                    string pName = terms[0];
                    List<Vector4> subpoints = new();

                    for (int i=2; i < terms.Length; i+=2)
                    {
                        string[] pTerms = terms[i].Split(',');

                        Vector4 pt = new Vector4(
                            int.Parse(pTerms[0]),
                            int.Parse(pTerms[1]),
                            int.Parse(pTerms[2]),
                            int.Parse(pTerms[3])
                            );

                        subpoints.Add(pt);
                        sliceW.Add(pt.w);
                    }

                    InterpolationPoint4D p4d = new InterpolationPoint4D(subpoints);

                    points.Add(pName, p4d);
                }
            }
        } // InterpolationBasedShape()


        // Lines and points // TODO: Remove Boon
        //protected List<InterpolationPoint4D> points4D = new List<InterpolationPoint4D>(){};
        //protected List<InterpolationPoint4D> lines4D = new List<InterpolationPoint4D>(){};

        // writes shape information to file in shape4D file format
        public void ToFile(string fileName)
        {
            using (StreamWriter sw = File.CreateText(fileName))
            {
                foreach (KeyValuePair<string, InterpolationPoint4D> point in points)
                {
                    sw.WriteLine(string.Format("{0}:{1}",
                        point.Key, 
                        point.Value
                        ));
                }

                foreach(Line<InterpolationPoint4D> line in lines4D)
                {
                    sw.WriteLine(string.Format("l:{1}", line));
                }

                foreach(Face<InterpolationPoint4D> face in faces4D)
                {
                    sw.WriteLine(string.Format("f:{1}", face));
                }
            }
        }

        /// <summary>
        /// adds a 3D slice to the shape at a specified w coordinate
        /// </summary>
        /// <param name="w"></param>
        /// <param name="slice"></param>
        /// <returns>false if a slice already exists at point w (new slice won't be added), true otherwise</returns>
        public bool AddSlice(float w, Dictionary<string, Vector3> slice)
        {
            if (sliceW.Contains(w))
            {
                return false;
            }

            sliceW.Add(w);
            foreach((string name, Vector3 point) in slice)
            {
                points[name].AddSubpoint(point.withW(w));
            }

            return true;
        }

        /// <summary>
        /// removes all points in slice with specified w coordinate
        /// does nothing if no slice has the specified w coordinate
        /// </summary>
        /// <param name="w"></param>
        /// <returns>true if the slice was removed</returns>
        public bool RemoveSlice(float w)
        {
            if (!sliceW.Contains(w))
            {
                return false;
            }

            sliceW.Remove(w);
            foreach((_, InterpolationPoint4D pt) in points)
            {
                pt.RemoveSubpoint(w);
            }
            return true;
        }
    } // class InterpolationBasedShape

} // namespace v2