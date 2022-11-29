using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace v2
{
    [RequireComponent(typeof(Transform4D))]
    public class InterpolationBoxCollider : Collider4D
    {
        List<Box> colliders = new();
        InterpolationBasedShape shape;
        Transform4D t4d;
        public float deltaW = 0.5f;

        // Start is called before the first frame update
        void Start()
        {
            // interpolation collider always attached to interpolation shape
            shape = gameObject.GetComponent<InterpolationBasedShape>();
            t4d = gameObject.GetComponent<Transform4D>();
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

            return new Box { corner = minCorner, size = maxCorner - minCorner, t4d = t4d };
        }

        void SetColliders()
        {
            colliders.Clear();

            // get the colliders for the shape
            for (float w = shape.minW(); w <= shape.maxW(); w += deltaW)
            {
                var slice = shape.GetSliceAt(w, p => p);
                Box boundingBox = GetBoundingBox(slice);
                colliders.Add(boundingBox);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public override Ray4D.Intersection? RayIntersect(Ray4D ray)
        {
            return colliders.Select(collider => BoxCollider4DChecker.RayIntersect(collider, ray)).Min();
        }

        public override bool ContainsPoint(Vector4 p)
        {
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
