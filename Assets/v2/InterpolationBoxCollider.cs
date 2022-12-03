using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine.UIElements;

namespace v2
{
    [RequireComponent(typeof(Transform4D))]
    public class InterpolationBoxCollider : Collider4D, IShape4DRenderer
    {
        List<Box> colliders = new();
        public float numSlices = 10f;

        public InterpolationBasedShape Shape { get; set; }

        protected override void Start()
        {
            // interpolation collider always attached to interpolation shape
            SetColliders();
        }

        Box GetBoundingBox(InterpolationBasedShape.Slice slice)
        {
            Vector4 minCorner = Vector4.positiveInfinity;
            Vector4 maxCorner = Vector4.negativeInfinity;
            foreach (PointInfo pt in slice.points)
            {
                for (int axis = 0;axis < 4;++axis)
                {
                    minCorner[axis] = Mathf.Min(minCorner[axis], pt.position4D[axis]);
                    maxCorner[axis] = Mathf.Max(maxCorner[axis], pt.position4D[axis]);
                }
            }

            return new Box { corner = minCorner, size = (maxCorner - minCorner).XYZ().withW(numSlices), t4d = t4d };
        }

        void SetColliders()
        {
            colliders.Clear();

            // get the colliders for the shape
            float minW = Shape.minW();
            float maxW = Shape.maxW();
            for (float w = minW; w <= maxW; w += (maxW-minW)/numSlices)
            {
                var slice = Shape.GetSliceAt(w, p => p);
                Box boundingBox = GetBoundingBox(slice);

                colliders.Add(boundingBox);
            }
        }

        public override Ray4D.Intersection? RayIntersect(Ray4D ray)
        {
            var localRay = t4d.WorldToLocal(ray);
            var best = colliders.Select(collider => BoxCollider4DChecker.RayIntersectLocal(collider, localRay)).Min();
            if (best is Ray4D.Intersection intersect)
            {
                intersect.point = t4d.LocalToWorld(intersect.point);
                intersect.delta = (intersect.point - ray.src).magnitude;
                return intersect;
            }
            return null;
        }

        public override bool ContainsPoint(Vector4 p)
        {
            //TODO performancew
            return colliders.Any(collider => BoxCollider4DChecker.ContainsPoint(collider, p));
        }

        public override bool DoesCollide(Collider4D o)
        {
            if (o is BoxCollider4D b) return colliders.Any(collider => BoxCollider4DChecker.DoesCollide(collider, b.GetBox()));
            else if (o is InterpolationBoxCollider bx) return colliders.Any(collider => bx.colliders.Any(box => BoxCollider4DChecker.DoesCollide(collider, box)));

            // dead code
            throw new NotImplementedException();
        }

    }
}
