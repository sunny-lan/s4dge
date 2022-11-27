
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

        public Ray4D.Intersection? RayIntersect(Ray4D ray)
        {
            // transform to local coordinates
            ray = t4d.ApplyLocalTransform(ray);
            ray.src -= corner;

            // faces : x = 0, x = size.x, ..., w = 0, w = size.x
            Ray4D.Intersection? firstIntersect = Enumerable.Range(0, 4).Select(face => ray.intersectPlane(face, 0)).Min();
            return firstIntersect;
        }

        public bool ContainsPoint(Vector4 p)
        {
            p = t4d.InverseLocalTransform(p); //transform to local coordinates

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
                yield return t4d.ApplyLocalTransform(corner + offset);
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
