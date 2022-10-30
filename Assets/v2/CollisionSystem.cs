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
        public static CollisionSystem Instance { get; private set; }

        //TODO support things other than box colliders
        private List<BoxCollider4D> colliders = new List<BoxCollider4D>();

        public void Add(BoxCollider4D boxCollider)
        {
            colliders.Add(boxCollider);
        }

        public void Remove(BoxCollider4D boxCollider4D)
        {
            colliders.Remove(boxCollider4D);
        }

        private void Awake()
        {
            Instance ??= this;
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

                    if (a.DoesCollide(b))
                    {
                        a.TriggerCollision(b);
                        b.TriggerCollision(a);
                    }
                }
            }
        }
    }
}
