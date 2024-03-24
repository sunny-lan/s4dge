using System;
using System.Collections;
using UnityEngine;
using S4DGE;

namespace S4DGE
{
    public abstract class Collider4D: MonoBehaviour
    {
        Transform4D _t4d;
        public Transform4D t4d => _t4d ?? (_t4d = GetComponent<Transform4D>()); //TODO sus

        public int Layer
        {
            get
            {
                return gameObject.layer;
            }
        }

        protected virtual void Start()
        {

        }

        private void OnEnable()
        {
            CollisionSystem.Instance.Add(this);
        }

        private void OnDisable()
        {
            CollisionSystem.Instance.Remove(this);
        }

        public bool IsCollidingThisFrame { get; internal set; }

        /// <summary>
        /// This event is triggered every frame, for every object 
        /// that is colliding with
        /// </summary>
        public event Action<Collider4D> OnCollisionStay;

        /// <summary>
        /// Called by CollisionSystem to trigger the event.
        /// Should NOT be called by anyone else
        /// </summary>
        /// <param name="other"></param>
        public void TriggerCollision(Collider4D other)
        {
            OnCollisionStay?.Invoke(other);
        }

        public abstract Ray4D.Intersection? RayIntersect(Ray4D ray);

        public abstract bool ContainsPoint(Vector4 p);

        public abstract bool DoesCollide(Collider4D b);
    }
}