using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace v2
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

            return new Intersection { delta=delta, point=point };
        }

        public Vector4 getPoint(float delta)
        {
            return src + direction * delta;
        }
    }

    /// <summary>
    /// Manages collisions. One of this must be added to the scene for collisions to work.
    /// </summary>
    public class CollisionSystem :MonoBehaviour
    {
        private static CollisionSystem _instance; // keep the actual instance private
        public static CollisionSystem Instance { get { // find the script instance in the scene if the private instance is null
            if ( _instance == null ) {
                _instance = FindObjectOfType(typeof(CollisionSystem)) as CollisionSystem;
            }
            return _instance;
        } }

        //TODO support things other than box colliders
        private List<BoxCollider4D> colliders = new List<BoxCollider4D>();

        private void Awake()
        {
            _instance ??= this; // assign this instance to the static variable if this Awake() occurs before any get call
        }

        public void Add(BoxCollider4D boxCollider)
        {
            colliders.Add(boxCollider);
        }

        public void Remove(BoxCollider4D boxCollider4D)
        {
            colliders.Remove(boxCollider4D);
        }

        public IEnumerable<Ray4D.Intersection?> Raycast(Ray4D ray, int layerMask)
        {
            IEnumerable<Ray4D.Intersection?> intersects = colliders.
                Where(collider => (layerMask & (1 << collider.gameObject.layer)) != 0).
                Select(collider => collider.RayIntersect(ray));
            return intersects;
        }

        private void Update()
        {
            //TODO performance
            for(int i = 0; i < colliders.Count; i++)
            {
                for(int j = 0; j < i; j++)
                {
                    var a = colliders[i];
                    var b = colliders[j];
                    if (Physics.GetIgnoreLayerCollision(a.gameObject.layer, b.gameObject.layer))
                        continue;

                    if (a.DoesCollide(b) || b.DoesCollide(a)) // need to check if points of a are in b OR points of b are in a
                    {
                        a.TriggerCollision(b);
                        b.TriggerCollision(a);
                    }
                }
            }
        }
    }
}
