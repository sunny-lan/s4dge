using System;
using System.Collections;
using UnityEngine;
using v2;

public abstract class Collider4D: MonoBehaviour
{

    public int Layer
    {
        get
        {
            return gameObject.layer;
        }
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