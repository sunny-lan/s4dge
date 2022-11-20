using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace v2
{
    [ExecuteAlways]
    [RequireComponent(typeof(Transform4D))]
    public class SliceRenderer : MonoBehaviour, IShape4DRenderer
    {
        Transform4D t4d;
        Mesh mesh;
        Geometry3D slice = new();

        public InterpolationBasedShape Shape { get=>shape; set=>shape=value; }
        [DoNotSerialize] //TODO for now
        InterpolationBasedShape shape;

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
        }

        //
        // Here is the rendering code
        // We add a callback for every camera before it begins rendering
        //  - Upon any camera render, we calculate the slice and submit the geometry for that camera
        //

        private void OnEnable()
        {
            Camera4D.onBeginCameraRendering += RenderForCamera;
        }

        private void OnDisable()
        {
            Camera4D.onBeginCameraRendering -= RenderForCamera;
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