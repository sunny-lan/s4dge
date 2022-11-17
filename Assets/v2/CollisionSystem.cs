using System;
using System.Collections.Generic;
using UnityEngine;

namespace v2
{
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
