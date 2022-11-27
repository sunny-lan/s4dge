
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
    public class BoxCollider4D:MonoBehaviour
    {
        public Vector4 corner;
        public Vector4 size;

        private Transform4D t4d;

        /// <summary>
        /// This event is triggered every frame, for every object 
        /// that is colliding with
        /// </summary>
        public event Action<BoxCollider4D> OnCollisionStay;

        /// <summary>
        /// Called by CollisionSystem to trigger the event.
        /// Should NOT be called by anyone else
        /// </summary>
        /// <param name="other"></param>
        internal void TriggerCollision(BoxCollider4D other)
        {
            OnCollisionStay?.Invoke(other);
        }

        private void Awake()
        {
            t4d = GetComponent<Transform4D>();
        }

        private void OnEnable()
        {
            CollisionSystem.Instance.Add(this);
        }

        private void OnDisable()
        {
            CollisionSystem.Instance.Remove(this);
        }

        public static Ray4D.Intersection? Min(Ray4D.Intersection? a, Ray4D.Intersection? b)
        {
            if (a == null) return b;
            if (b == null) return a;
            if (a is Ray4D.Intersection aa && b is Ray4D.Intersection bb)
            {
                return aa.CompareTo(bb) < 0 ? aa : bb;
            } else
            {
                // dead code
                return null;
            }
        }

        public Ray4D.Intersection? RayIntersect(Ray4D ray)
        {
            Debug.Log("src: " + ray.src.ToString() + " dir: " + ray.direction.ToString());
            // transform to local coordinates
            ray = t4d.WorldToLocal(ray);
            ray.src -= corner;

            Debug.Log("trans src: " + ray.src.ToString() + " trans dir: " + ray.direction.ToString());

            // faces : x = 0, x = size.x, ..., w = 0, w = size.x
            Ray4D.Intersection? firstIntersect = null;
            for (int face = 0; face < 4; ++face)
            {
                firstIntersect = Min(firstIntersect, ray.intersectPlane(face, 0, size));
                firstIntersect = Min(firstIntersect, ray.intersectPlane(face, size[face], size));
            }

            foreach (var curIntersect in Enumerable.
                Range(0, 4).
                Select(face => ray.intersectPlane(face, 0, size)))
            {
                if (curIntersect is Ray4D.Intersection isct) {
                    Debug.Log("dlt: " + isct.delta + " pt: " + isct.point);
                }
            }

            // transform back to world coordinates
            if (firstIntersect is Ray4D.Intersection intersect)
            {
                intersect.point = t4d.LocalToWorld(intersect.point + corner);
                firstIntersect = intersect;
            }

            return firstIntersect;
        }

        public bool ContainsPoint(Vector4 p)
        {
            p = t4d.WorldToLocal(p); //transform to local coordinates

            //check if in box
            p -= corner;
            bool colliding = p.x>=0 && p.y>=0 && p.z>=0 && p.w>=0 
                && 
                p.x<=size.x && p.y<=size.y && p.z<=size.z && p.w<=size.w; 
            
            if ( colliding ) {
                Log.Print("Collision detected at: " + p.x + " " + p.y + " " + p.z + " " + p.w, Log.collisions ); 
            }
            return colliding;
                
        }

        //TODO performance
        public IEnumerable<Vector4> GetCorners()
        {
            for(int i = 0; i < (1<<4); i++)
            {
                Vector4 offset = size;
                offset.Scale(new Vector4(
                    (i >> 0) & 1,
                    (i >> 1) & 1,
                    (i >> 2) & 1,
                    (i >> 3) & 1
                ));
                yield return t4d.LocalToWorld(corner + offset);
            }
        }

        public bool DoesCollide(BoxCollider4D b)
        {
            foreach(var corner in b.GetCorners())
            {
                if (ContainsPoint(corner))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
