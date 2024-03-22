using System;
using UnityEngine;

namespace S4DGE
{
    public struct Ray4D
    {
        public Vector4 src, direction;

        public struct Intersection : IComparable<Intersection>
        {
            public float delta;
            public Vector4 point;

            public int CompareTo(Intersection o)
            {
                return delta.CompareTo(o.delta);
            }

            public override string ToString()
            {
                return delta.ToString() + " " + point.ToString();
            }
        }

        // solve for intersection of ray with plane 'component=val'
        // e.g. x=2
        // returns delta in (intersect pt) = src + delta * direction, as well as the point
        public Intersection? intersectPlane(int component, float val, Vector4 boxSize)
        {
            // ray parallel to plane, so cannot intersect
            if (direction[component] == 0)
            {
                return null;
            }

            float delta = (val - src[component]) / direction[component];

            if (delta < 0)
            {
                // if delta is negative, there is no intersection
                // (the ray has to cast backwards to reach the point)
                return null;
            }
            var point = getPoint(delta);
            if (point.x < 0 || point.x > boxSize.x ||
                point.y < 0 || point.y > boxSize.y ||
                point.z < 0 || point.z > boxSize.z ||
                point.w < 0 || point.w > boxSize.w)
            {
                // collision with plane outside of box
                return null;
            }

            return new Intersection { delta = Mathf.Sign(delta) * (point - src).magnitude, point = point };
        }

        public Vector4 getPoint(float delta)
        {
            return src + direction * delta;
        }
    }
}
