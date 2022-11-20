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
    /// <summary>
    /// Stores all info for a single subpoint
    /// </summary>
    [Serializable]
    public struct PointInfo
    {
        public Vector4 position4D;

        /// <summary>
        /// Texture mapping of the point
        /// </summary>
        public Vector2 uv;

        // for easier access to w/3d position
        public Vector3 position { get => position4D.XYZ(); set => position4D = value.withW(w); }
        public float w { get => position4D.w; set => position4D = position.withW(value); }

        // Interpolates between two subpoints given w
        // (Interpolates every field linearly)
        public static PointInfo Interpolate(float w, PointInfo initialPoint, PointInfo finalPoint)
        {
            var percent = Mathf.InverseLerp(initialPoint.w, finalPoint.w, w);
            return new()
            {
                position4D = Vector4.Lerp(initialPoint.position4D, finalPoint.position4D, percent),
                uv = Vector2.Lerp(initialPoint.uv, finalPoint.uv, percent),
            };
        }
    }

    /// <summary>
    /// Represents a 4D point by a series of interpolated 3D points
    /// </summary>
    [Serializable]
    public class InterpolationPoint4D
    {
        public List<PointInfo> subpoints;

        // There shouldn't be multiple points with the same w coordinate
        public InterpolationPoint4D(List<PointInfo> points)
        {
            subpoints = points;
        }

        public void AddSubpoint(Vector4 subpoint)
        {
            //TODO add editor support for PointInfo
            subpoints.Add(new PointInfo()
            {
                position = subpoint.XYZ(),
                w = subpoint.w,
            });
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
        /// <param name="w">Where to slice at</param>
        /// <param name="transform">Transformation to apply</param>
        /// <returns></returns>
        public (PointInfo, bool ended) GetPoint(float w, Func<Vector4, Vector4> transform)
        {
            
            var transformedPoints = subpoints.Select(point =>
            {
                point.position4D = transform(point.position4D);
                return point;
            }).ToArray();

            Array.Sort(transformedPoints, (x, y) => x.w.CompareTo(y.w)); // sort by increasing w

            // if w is out of range, return the closest endpoint
            if (w <= transformedPoints[0].w)
            {
                return (transformedPoints.First(), true);
            }
            else if (w >= transformedPoints[transformedPoints.Length - 1].w)
            {
                return (transformedPoints.Last(), true);
            }
            else
            {
                PointInfo right = transformedPoints.First(x => x.w > w);

                PointInfo left = transformedPoints.LastOrDefault(x => x.w <= w);
                return (PointInfo.Interpolate(w, left, right), false);
            }
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
        // Lines and points 
        public List<Line<InterpolationPoint4D>> lines4D = new();
        public List<Face<InterpolationPoint4D>> faces4D = new();
        [SerializeField]
        public Dictionary<string, InterpolationPoint4D> points = new();
        internal HashSet<float> sliceW = new();

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
            foreach ((string name, Vector3 point) in slice)
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
            foreach ((_, InterpolationPoint4D pt) in points)
            {
                pt.RemoveSubpoint(w);
            }
            return true;
        }
    } // class InterpolationBasedShape

} // namespace v2