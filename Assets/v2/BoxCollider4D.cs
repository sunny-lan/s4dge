
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

namespace v2
{
    /// <summary>
    /// Represents hypercube collision area
    /// </summary>
    [RequireComponent(typeof(Transform4D))]
    public class BoxCollider4D:Collider4D
    {

        public Vector4 corner;
        public Vector4 size;

        public Box GetBox()
        {
            return new Box
            {
                corner = corner,
                size = size,
                t4d = t4d
            };
        }

        public override Ray4D.Intersection? RayIntersect(Ray4D ray)
        {
            return BoxCollider4DChecker.RayIntersect(GetBox(), ray);
        }

        public override bool ContainsPoint(Vector4 p)
        {
            return BoxCollider4DChecker.ContainsPoint(GetBox(), p); 
        }

        public override bool DoesCollide(Collider4D o)
        {
            if (o is BoxCollider4D b) return BoxCollider4DChecker.DoesCollide(GetBox(), b.GetBox());
            else if (o is InterpolationBoxCollider bx) return bx.DoesCollide(this);

            // dead code
            throw new NotImplementedException();
        }
    }
}
