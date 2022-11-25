using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace v2
{
    [ExecuteAlways]
    [RequireComponent(typeof(Transform4D))]
    public class MeshRenderer : MonoBehaviour, IShape4DRenderer
    {
        Transform4D t4d;
        Geometry3D slice = new();
        Mesh mesh;
        Mesh[] meshes;

        public InterpolationBasedShape Shape { get=>shape; set=>shape=value; }
        [DoNotSerialize] //TODO for now
        InterpolationBasedShape shape;

        public string fileName;
        public Material material;
        //
        // BEGIN editor variables/methods
        //

        // TODO: decide acceptable w range here
        [Range(-10.0f, 10.0f)]
        public float previewW = 0;

        // 
        // END editor variables/methods
        //

        void Start()
        {
            t4d = gameObject.GetComponent<Transform4D>();
            mesh = new();
            Mesh[] childMeshes = Array.ConvertAll<object, Mesh>(Resources.LoadAll(fileName, typeof(Mesh)), obj => (Mesh)obj); // find all meshes in children by their mesh filters
            meshes = childMeshes;
            Log.Print("Found " + childMeshes.Length + " submeshes in Mesh Renderer model", Log.meshRendering);
            LoadShapeFromMeshes( childMeshes );
        }

        private void LoadShapeFromMeshes(Mesh[] meshes)
        {
            shape = ScriptableObject.CreateInstance<InterpolationBasedShape>();
            var points = shape.points;
            var lines4D = shape.lines4D;
            var faces4D = shape.faces4D;
            char submeshChar = 'A';

            using ( var meshDataArray = Mesh.AcquireReadOnlyMeshData( meshes ) )
            {
                for ( int m = 0; m < meshDataArray.Length; m++ ) {
                    var localMesh = meshDataArray[m];
                    var uvs = new NativeArray<Vector3>( localMesh.vertexCount, Allocator.TempJob); // allocate native array for uvs, same size as number of vertices
                    bool hasUVs = localMesh.HasVertexAttribute( VertexAttribute.TexCoord0 ); // check if vertices have texture attribute for uv0
                    if ( hasUVs )
                    {
                        localMesh.GetUVs(0, uvs); // populate uvs with channel 0
                    }
                    var vertices = new NativeArray<Vector3>( localMesh.vertexCount, Allocator.TempJob); // allocate native array for vertices with vertex size
                    localMesh.GetVertices(vertices); // populate vertices with all vertices in local mesh
                    // TODO: support faces for other submeshes
                    var indices = new NativeArray<int>( localMesh.GetSubMesh( 0 ).indexCount, Allocator.TempJob); // allocate native array for indices in submesh 0
                    localMesh.GetIndices(indices, 0 ); // populate indices from all faces in submesh 0
                    // add all the vertices from the local model to the shape
                    for( int index = 0; index < vertices.Length; index++ ) {
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

                        InterpolationPoint4D p4d = new InterpolationPoint4D(subpoints);

                        points.Add(submeshChar + index.ToString(), p4d); // name points as just {index} from the vertices array
                    }
                    MeshTopology faceType = localMesh.GetSubMesh(0).topology; // gets the topology for submesh 0
                    int linesPerFace = 0;
                    switch( faceType ) {
                        case MeshTopology.Triangles: linesPerFace = 3; break;
                        case MeshTopology.Quads: linesPerFace = 4; break;
                        case MeshTopology.Lines: linesPerFace = 2; break;
                    }
                    Log.Print($"Submesh loading {vertices.Length}, {linesPerFace}, {indices.Length}, {localMesh.subMeshCount}", Log.meshRendering);
                    // traverse each face in the indices of the mesh
                    for ( int i = 0; i < indices.Length; i += linesPerFace ) {
                        List<InterpolationPoint4D> facePoints = new List<InterpolationPoint4D>();
                        // traverse each line on edge of this face
                        for (int j = i + 1; j < i + linesPerFace; ++j ) {
                            Line<InterpolationPoint4D> line = new Line<InterpolationPoint4D>(points[submeshChar + indices[j-1].ToString()], 
                                                                                            points[submeshChar + indices[j].ToString()]); // fetch points by index in string format
                            lines4D.Add(line);
                            facePoints.Add(points[submeshChar + indices[j-1].ToString()]);
                        }
                        // add last line connecting first and last point
                        Line<InterpolationPoint4D> firstLastLine = new Line<InterpolationPoint4D>(points[submeshChar + indices[i].ToString()], 
                                                                                                points[submeshChar + indices[i+linesPerFace-1].ToString()]);
                        lines4D.Add(firstLastLine);

                        facePoints.Add(points[submeshChar + indices[i+linesPerFace-1].ToString()]); // add the last point to the face list
                        // add face for all points
                        faces4D.Add(new Face<InterpolationPoint4D>(facePoints));
                    }
                    Log.Print($"Loaded mesh with {points.Count} points, {lines4D.Count} lines, and {faces4D.Count} faces {shape == null}", Log.meshRendering);
                    submeshChar++;

                    indices.Dispose();
                    vertices.Dispose();
                    uvs.Dispose();
                }
            }
        }

        //
        // Here is the rendering code
        // We add a callback for every camera before it begins rendering
        //  - Upon any camera render, we calculate the slice and submit the geometry for that camera
        //

        private void OnEnable()
        {
            Camera4D.onBeginCameraRendering += SimpleRender;
        }

        private void OnDisable()
        {
            Camera4D.onBeginCameraRendering -= SimpleRender;
        }

        static int cameraPosShaderID = Shader.PropertyToID("_4D_Camera_Pos");
        private void RenderForCamera(ScriptableRenderContext ctx, Camera4D cam)
        {
            if (shape == null) return;

            slice.Clear();
            //if (Application.IsPlaying(gameObject)) //TODO
            {
                // display one slice every 0.1 units from -1 to 1
                // TODO discussion on how visuals should look
                for (float dlt = -previewWidth; dlt <= previewWidth; dlt += 0.1f)
                {
                    drawSliceAt(cam.t4d.position.w + dlt);
                }
            }
            //else
            //{
            //    drawSliceAt(previewW);
            //}
            slice.ApplyToMesh(mesh);
            MaterialPropertyBlock blk = new();
            blk.SetVector(cameraPosShaderID, cam.t4d.position); //pass in camera position to shader
            Graphics.DrawMesh(
                mesh: mesh, 
                matrix: Matrix4x4.identity, 
                material: material, 
                layer: gameObject.layer, 
                camera: cam.camera3D,
                submeshIndex: 0,
                properties: blk
            );
        }

        private void SimpleRender( ScriptableRenderContext ctx, Camera4D cam )
        {
            if (meshes == null) return;

            MaterialPropertyBlock blk = new();
            blk.SetVector(cameraPosShaderID, cam.t4d.position); //pass in camera position to shader
            foreach (Mesh m in meshes )
            {
                for ( int i = 0; i < m.subMeshCount; ++i )
                {
                    Graphics.DrawMesh(
                        mesh: m, 
                        matrix: Matrix4x4.identity, 
                        material: material, 
                        layer: gameObject.layer, 
                        camera: cam.camera3D,
                        submeshIndex: i,
                        properties: blk
                    );
                }
               
            }
            
        }

        // TODO this should be determined by camera. This is just here for debugging purpose
        public float previewWidth = 1;
        public bool showWhenOutOfRange = true;


        // Iterates over all lines
        // Draws points between them
        // Draws all lines
        Dictionary<InterpolationPoint4D, int> tmp_interpolatedValue = new(); 
        void drawSliceAt(float w)
        {
            // interpolate all points and store in dictionary
            int invalidPoints = 0;
            foreach(var point in shape.points.Values)
            {
                var (interpolated, invalid) = point.GetPoint(w, t4d.Transform);
                if (invalid) invalidPoints++;
                int index = slice.AddPoint(interpolated);
                tmp_interpolatedValue[point] = index;
            }

            if (!showWhenOutOfRange)
            {
                // if all points out of range, don't draw
                if (invalidPoints == shape.points.Count)
                    return;
            }

            // lines
            // Debug.Log("Lines");
            foreach (Line<InterpolationPoint4D> line in shape.lines4D)
            { // iterate over every line 

                // Find point location given w
                // for each line, get each point from their start and end location and w
                // and make sure to apply the transform
                int p1 = tmp_interpolatedValue[line.p1];
                int p2 = tmp_interpolatedValue[line.p2];
                slice.line(p1, p2); // draw line
            }

            // faces
            // Debug.Log("Faces");
            foreach (Face<InterpolationPoint4D> face in shape.faces4D)
            { // face is an array of faces
              // Limit to drawing triangles and squares

                // generate calculated points
                //  1. Select = for each point x apply getPoint(x, w)
                //  3. Pass all calculated points to drawPolygon
                var slicedPoints = face.points
                    .Select(x => tmp_interpolatedValue[x])
                    .ToArray();
                slice.fillPolygon(slicedPoints);
            }
        }
    }

}