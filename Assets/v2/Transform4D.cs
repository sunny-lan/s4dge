using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a translation/rotation/scaling in 4D
/// </summary>
public class Transform4D : MonoBehaviour
{
    public Vector4 position;
    public Vector3 rotation = new Vector3(1.0f, 3.75f, -0.5f); // 3 coordinates representing the rotation on a 3-sphere in RADIANS [x=ξ1, y=η, z=ξ2] (see Hopf Coordinates)
    public Vector4 scale = Vector4.one;

    /// <summary>
    /// applies transform to point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public Vector4 Transform(Vector4 point)
    {
        Vector4 v = Vector4.Scale(scale, point) + position;
        return Rotate( v, rotation );
    }

    // TODO: Does not account for rotation
    public Vector4 InverseTransform(Vector4 point)
    {
        return InverseScale( (point - position), scale);
    }

    private Vector4 InverseScale(Vector4 v, Vector4 divisors)
    {
        Vector4 componentInverse = Vector4.one;
        for ( int i = 0; i < 4; i++ )
        {
            Debug.Assert( divisors[i] != 0, "ERROR: Scale vector with a 0 component attempted division by 0: " + divisors);
            componentInverse[i] = 1.0f / divisors[i];
        }
        return Vector4.Scale(v, componentInverse);
    }

    private Vector4 Rotate(Vector4 v, Vector3 hopf) {
        return new Vector4( // https://en.wikipedia.org/wiki/Rotations_in_4-dimensional_Euclidean_space
            Mathf.Cos( hopf.z ) * Mathf.Cos( hopf.y ) * v.x, // x' = Cos(ξ2) * Cos(η) * x
            Mathf.Sin( hopf.z ) * Mathf.Cos( hopf.y ) * v.y, // y' = Sin(ξ2) * Cos(η) * y
            Mathf.Sin( hopf.x ) * Mathf.Sin( hopf.y ) * v.z, // z' = Sin(ξ1) * Sin(η) * z
            Mathf.Cos( hopf.x ) * Mathf.Sin( hopf.y ) * v.w // w' = Cos(ξ1) * Sin(η) * w
        );
    }
}
