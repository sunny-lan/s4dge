using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace v2
{
    public static class S4DGE
    {


        // TODO add auto normalization such that all points have the same start and end w.


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
            //
            var shape = ScriptableObject.CreateInstance<InterpolationBasedShape>();
            var points = shape.points;
            var lines4D = shape.lines4D;
            var faces4D = shape.faces4D;

            string[] fileLines = File.ReadAllLines(fileName);
            foreach (string fileLine in fileLines)
            {


                // Remove comments
                string currentFileLine = fileLine;
                int index = currentFileLine.IndexOf("#");
                if (index >= 0)
                {
                    currentFileLine = currentFileLine.Substring(0, index);
                }
                currentFileLine = currentFileLine.Trim(); // Trim leading and ending white space

                if (currentFileLine == "")
                {
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
                    foreach (string term in terms.Skip(1))
                    { // Adds points to the face
                        facePoints.Add(points[term]);
                    }

                    faces4D.Add(new Face<InterpolationPoint4D>(facePoints));
                }
                else if (currentFileLine[0] == 't') //texture mapping
                {
                    string[] terms = currentFileLine.Split(
                        new char[] { ':', '(', ')' },
                        options: System.StringSplitOptions.RemoveEmptyEntries
                    );
                    Debug.Assert(terms.Length >= 3);

                    InterpolationPoint4D point = points[terms[1]];
                    Vector2 lastTerm = default;
                    for (int i = 0; i < point.subpoints.Count; i++)
                    {
                        Vector2 uv = lastTerm;
                        if (i + 2 < terms.Length)
                        {
                            string[] pTerms = terms[i + 2].Split(',');
                            //Debug.Log(terms[i+2]);
                            lastTerm = uv = new(
                                float.Parse(pTerms[0]),
                                float.Parse(pTerms[1])
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
                    string[] terms = currentFileLine.Split(':', '(', ')');
                    string pName = terms[0];
                    List<PointInfo> subpoints = new();

                    for (int i = 2; i < terms.Length; i += 2)
                    {
                        string[] pTerms = terms[i].Split(',');

                        Vector4 position = new Vector4(
                            float.Parse(pTerms[0]),
                            float.Parse(pTerms[1]),
                            float.Parse(pTerms[2]),
                            float.Parse(pTerms[3])
                        );

                        subpoints.Add(new PointInfo()
                        {
                            position4D = position,
                        });

                        shape.sliceW.Add(position.w);
                    }

                    InterpolationPoint4D p4d = new InterpolationPoint4D(pName, subpoints);

                    points.Add(pName, p4d);
                }
            }
            return shape;
        }

        public static IEnumerable<InterpolationBasedShape> MeshToS4DGE(Mesh mesh)
        {

            using (var meshDataArray = Mesh.AcquireReadOnlyMeshData(mesh))
            {
                for (int m = 0; m < meshDataArray.Length; m++)
                {
                    var shape = ScriptableObject.CreateInstance<InterpolationBasedShape>();
                    var points = shape.points;
                    var lines4D = shape.lines4D;
                    var faces4D = shape.faces4D;
                    char submeshChar = 'A';

                    var localMesh = meshDataArray[m];
                    var uvs = new NativeArray<Vector3>(localMesh.vertexCount, Allocator.TempJob); // allocate native array for uvs, same size as number of vertices
                    bool hasUVs = localMesh.HasVertexAttribute(VertexAttribute.TexCoord0); // check if vertices have texture attribute for uv0
                    if (hasUVs)
                    {
                        localMesh.GetUVs(0, uvs); // populate uvs with channel 0
                    }
                    var vertices = new NativeArray<Vector3>(localMesh.vertexCount, Allocator.TempJob); // allocate native array for vertices with vertex size
                    localMesh.GetVertices(vertices); // populate vertices with all vertices in local mesh
                    // TODO: support faces for other submeshes
                    var indices = new NativeArray<int>(localMesh.GetSubMesh(0).indexCount, Allocator.TempJob); // allocate native array for indices in submesh 0
                    localMesh.GetIndices(indices, 0); // populate indices from all faces in submesh 0
                    // add all the vertices from the local model to the shape
                    InterpolationPoint4D[] pointsByIndex = new InterpolationPoint4D[vertices.Length];
                    for (int index = 0; index < vertices.Length; index++)
                    {
                        var v = vertices[index];
                        List<PointInfo> subpoints = new();
                        subpoints.Add(new PointInfo()
                        {
                            position4D = new Vector4(v.x, v.y, v.z, -1), // put all the vertices at w=0
                            uv = hasUVs ? uvs[index] : Vector2.zero, // get the uv for this vertex
                        });
                        subpoints.Add(new PointInfo()
                        {
                            position4D = new Vector4(v.x, v.y, v.z, 1), // put all the vertices at w=0
                            uv = hasUVs ? uvs[index] : Vector2.zero, // get the uv for this vertex
                        });
                        shape.sliceW.Add(1);
                        shape.sliceW.Add(-1);

                        string pName = submeshChar + index.ToString();
                        InterpolationPoint4D p4d = new InterpolationPoint4D(pName, subpoints);
                        pointsByIndex[index] = p4d; // store point by index for easier access
                        points.Add(pName, p4d); // name points as just {index} from the vertices array
                    }
                    MeshTopology faceType = localMesh.GetSubMesh(0).topology; // gets the topology for submesh 0
                    int linesPerFace = faceType switch
                    {
                        MeshTopology.Triangles => 3,
                        MeshTopology.Quads => 4,
                        MeshTopology.Lines => 2,
                        MeshTopology.LineStrip => throw new System.NotImplementedException(),
                        MeshTopology.Points => throw new System.NotImplementedException(),
                        _ => throw new System.NotImplementedException(),
                    };
                    
                    // traverse each face in the indices of the mesh
                    Log.Print($"Submesh loading {vertices.Length}, {linesPerFace}, {indices.Length}, {localMesh.subMeshCount}", Log.meshRendering);
                    for (int i = 0; i < indices.Length; i += linesPerFace)
                    {
                        if (faceType == MeshTopology.Lines)
                            lines4D.Add(new(
                                pointsByIndex[indices[i]],
                                pointsByIndex[indices[i + 1]]
                            ));
                        else if (faceType is MeshTopology.Quads or MeshTopology.Triangles)
                            // add face for all points
                            faces4D.Add(new(indices.Skip(i).Take(linesPerFace).Select(index => pointsByIndex[index]).ToList()));
                    }

                    Log.Print($"Loaded mesh with {points.Count} points, {lines4D.Count} lines, and {faces4D.Count} faces {shape == null}", Log.meshRendering);
                    submeshChar++;

                    indices.Dispose();
                    vertices.Dispose();
                    uvs.Dispose();

                    yield return shape;
                }
            }
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
                        point.Value.PointToString()
                        ));

                    // texture mappings
                    sw.WriteLine(string.Format("t:{0}{1}",
                        point.Key,
                        string.Join("", point.Value.subpoints.Select(p => $"({p.uv.x},{p.uv.y})"))
                    ));
                }

                foreach (Line<InterpolationPoint4D> line in lines4D)
                {
                    sw.WriteLine(string.Format("l:{0}", line));
                }

                foreach (Face<InterpolationPoint4D> face in faces4D)
                {
                    sw.WriteLine(string.Format("f:{0}", face));
                }
            }
        }
    }
}