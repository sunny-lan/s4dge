using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace v2
{
    [ExecuteAlways]
    public class SliceRenderer : MonoBehaviour
    {
        Transform4D t4d;
        RenderHelper3D render3d;
        Geometry3D slice = new();

        public InterpolationBasedShape shape;

        // Start is called before the first frame update
        void Start()
        {
            t4d = gameObject.GetComponent<Transform4D>();
            render3d = gameObject.GetComponent<RenderHelper3D>();
            
        }

        void Update()
        {
            if (shape == null) return;

            slice.Clear();
            calculateSlice(Camera4D.main.t4d.position.w);
            render3d.SetGeometry(slice);
        }

        // helper function which transforms point and then interpolates it
        Vector3 getPoint(Point4D point, float w)
        {
            var initialPoint = t4d.Transform(point.subpoints.FirstOrDefault());
            var finalPoint = t4d.Transform(point.subpoints.LastOrDefault());
            var percent = Mathf.InverseLerp(initialPoint.w, finalPoint.w, w);
            return Vector3.LerpUnclamped(
                initialPoint.XYZ(),
                finalPoint.XYZ(),
                percent);
        }

        // Iterates over all lines
        // Draws points between them
        // Draws all lines
        // Also draws faces??? TODO:// Ask someone about this
        void calculateSlice(float w)
        {
            // lines
            // Debug.Log("Lines");
            foreach (Line4D line in shape.lines4D)
            { // iterate over every line 

                // Find point location given w
                // for each line, get each point from their start and end location and w
                Vector3 p1 = line.p1.getPoint(w, t4d);
                Vector3 p2 = line.p2.getPoint(w, t4d);
                slice.line(p1, p2); // draw line
            }

            // faces
            // Debug.Log("Faces");
            foreach (Face4D face in shape.faces4D)
            { // face is an array of faces
              // Limit to drawing triangles and squares

                // generate calculated points
                //  1. Select = for each point x apply getPoint(x, w)
                //  3. Pass all calculated points to drawPolygon
                var slicedPoints = face.points
                    .Select(x => x.getPoint(w, t4d))
                    .ToArray();
                slice.fillPolygon(slicedPoints);
            }
        }
    }

}