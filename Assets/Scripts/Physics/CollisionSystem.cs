using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace v2
{

    /// <summary>
    /// Manages collisions. One of this must be added to the scene for collisions to work.
    /// </summary>
    public class CollisionSystem : MonoBehaviour
    {
        public bool run = false;

        private static CollisionSystem _instance; // keep the actual instance private
        public static CollisionSystem Instance
        {
            get
            { // find the script instance in the scene if the private instance is null
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CollisionSystem>();
                }
                return _instance;
            }
        }

        //TODO support things other than box colliders
        private List<Collider4D> colliders = new();

        private void Awake()
        {
            _instance ??= this; // assign this instance to the static variable if this Awake() occurs before any get call
        }

        public void Add(Collider4D collider)
        {
            colliders.Add(collider);
        }

        public void Remove(Collider4D collider)
        {
            colliders.Remove(collider);
        }

        public IEnumerable<Ray4D.Intersection> Raycast(Ray4D ray, int layerMask)
        {
            Profiler.BeginSample("Raycast");
            foreach (var collider in colliders.
                Where(collider => (layerMask & (1 << collider.Layer)) != 0))
            {
                if (collider.RayIntersect(ray) is Ray4D.Intersection intersection)
                    yield return intersection;
            }
            Profiler.EndSample();
        }

        private void Update()
        {
            if (!run) return; // TODO

            //TODO performance
            for (int i = 0; i < colliders.Count; i++)
                colliders[i].IsCollidingThisFrame = false;

            for (int i = 0; i < colliders.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    var a = colliders[i];
                    var b = colliders[j];
                    if (Physics.GetIgnoreLayerCollision(a.Layer, b.Layer))
                        continue;

                    if (a.DoesCollide(b) || b.DoesCollide(a)) // need to check if points of a are in b OR points of b are in a
                    {
                        a.IsCollidingThisFrame = b.IsCollidingThisFrame = true;
                        a.TriggerCollision(b);
                        b.TriggerCollision(a);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Info about a single point of contact during a collision
    /// </summary>
    public struct ContactPoint4D
    {
        public Vector4 point;
        public Vector4 normal;
    }

    public class Collision4D
    {
        public BoxCollider4D collider;
        public List<ContactPoint4D> contacts;
    }
}
