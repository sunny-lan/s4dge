using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace v2
{
    public class S4DGELoader : MonoBehaviour
    {
        public string filePath = "Assets/Models/inclinedPlaneModel.s4dge";
        // Start is called before the first frame update
        void Start()
        {
            GetComponent<SliceRenderer>().shape = LoadS4DGE(filePath);
        }


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
         * To add a texture mapping, the format is "t:<Point name>, (<uv[0].x>, <uv[0].y>), ..."
         *      Where uv[] is the array of uv coordinates, which should be
         *      the same length as the number of subpoints
         *      If the length of uv[] is 1, the same uv value will be used for all subpoints
        */
        public static InterpolationBasedShape LoadS4DGE(string fileName)
        {
            var shape = ScriptableObject.CreateInstance<InterpolationBasedShape>();
            var points = shape.points;
            var lines4D = shape.lines4D;
            var faces4D = shape.faces4D;

            string[] fileLines = File.ReadAllLines(fileName);
            foreach (string fileLine in fileLines)
            {
                if (fileLine[0] == 'l')
                { // check that it is line
                    string[] terms = fileLine.Split(':', '-');
                    string lineP1 = terms[1];
                    string lineP2 = terms[2];

                    Line<InterpolationPoint4D> line = new Line<InterpolationPoint4D>(points[lineP1], points[lineP2]); // get rereference to points and then create lines

                    lines4D.Add(line);

                }
                else if (fileLine[0] == 'f')
                {
                    string[] terms = fileLine.Split(':', '-');

                    List<InterpolationPoint4D> facePoints = new List<InterpolationPoint4D>(); // optimization maybe possible lol
                    foreach (string term in terms.Skip(1))
                    { // Adds points to the face
                        facePoints.Add(points[term]);
                    }

                    faces4D.Add(new Face<InterpolationPoint4D>(facePoints));
                }
                else if (fileLine[0] == 't') //texture mapping
                {
                    string[] terms = fileLine.Split(':', ',', '(', ')');
                    Debug.Assert(terms.Length >= 3);

                    InterpolationPoint4D point = points[terms[1]];
                    Vector2 lastTerm = default;
                    for (int i = 0; i < point.subpoints.Count; i++)
                    {
                        Vector2 uv = lastTerm;
                        if (i + 2 < terms.Length)
                        {
                            string[] pTerms = terms[i + 2].Split(',');
                            lastTerm = uv = new(
                                int.Parse(pTerms[0]),
                                int.Parse(pTerms[1])
                            );
                        }

                        // update mapping
                        PointInfo pointInfo = point.subpoints[i];
                        pointInfo.uv = uv;
                        point.subpoints[i] = pointInfo;
                    }
                }
                else //point declaration
                {
                    string[] terms = fileLine.Split(':', '(', ')');
                    string pName = terms[0];
                    List<PointInfo> subpoints = new();

                    for (int i = 2; i < terms.Length; i += 2)
                    {
                        string[] pTerms = terms[i].Split(',');

                        Vector4 position = new Vector4(
                            int.Parse(pTerms[0]),
                            int.Parse(pTerms[1]),
                            int.Parse(pTerms[2]),
                            int.Parse(pTerms[3])
                        );

                        subpoints.Add(new PointInfo()
                        {
                            position4D = position,
                        });

                        shape.sliceW.Add(position.w);
                    }

                    InterpolationPoint4D p4d = new InterpolationPoint4D(subpoints);

                    points.Add(pName, p4d);
                }
            }
            return shape;
        } 



        /// <summary>
        /// writes shape information to file in shape4D file format
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="fileName"></param>
        public static void SaveS4DGE(InterpolationBasedShape shape, string fileName)
        {
            var points = shape.points;
            var lines4D = shape.lines4D;
            var faces4D = shape.faces4D;

            using (StreamWriter sw = File.CreateText(fileName))
            {
                foreach (KeyValuePair<string, InterpolationPoint4D> point in points)
                {
                    sw.WriteLine(string.Format("{0}:{1}",
                        point.Key,
                        point.Value
                        ));
                }

                foreach (Line<InterpolationPoint4D> line in lines4D)
                {
                    sw.WriteLine(string.Format("l:{1}", line));
                }

                foreach (Face<InterpolationPoint4D> face in faces4D)
                {
                    sw.WriteLine(string.Format("f:{1}", face));
                }
            }
        }
    }
}