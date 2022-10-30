using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a translation/rotation/scaling in 4D
/// </summary>
public class Transform4D : MonoBehaviour
{
    public const int ROTATION_DOF = 6;

    public Vector4 position;
    [HideInInspector]
    public float[] rotation = new float[ROTATION_DOF];
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

    public Vector4 InverseTransform(Vector4 point)
    {
        var unscaled = InverseScale( (point - position), scale );
        float[] negativeRotation = new float[rotation.Length];
        for ( int i = 0; i < rotation.Length; i++ ) {
            negativeRotation[i] = -rotation[i];
        }
        return Rotate( unscaled, negativeRotation );
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

    /// <summary>
    /// Rotates given point around 6 planes of rotation in 4d
    /// </summary>
    /// <param name="v"> The point for which a rotated version is returned </param>
    /// <param name="allRotations"> An array of 6 floats representing rotation around each plane </param>
    /// <returns> The rotated vector </returns>
    private Vector4 Rotate(Vector4 v, float[] allRotations) {
        int axisCount = 0;
        Vector4 r = v;
        for ( int i = 0; i < 3; ++i )
        {
            for ( int j = i + 1; j < 4; ++j )
            {
                r = Rotate(r, i, j, allRotations[ axisCount ] );
                axisCount++;
            }
        }
        return r;
    }
    /// <summary>
    /// Rotates the given point around theta radians around the defined plane
    /// </summary>
    /// <param name="v"> The point for which a rotated version is returned </param>
    /// <param name="axis1"> The first axis of the plane of rotation </param>
    /// <param name="axis2"> The second axis of the plane of rotation </param>
    /// <param name="theta"> The angle of rotation in radians </param>
    /// <returns> The rotated vector </returns>
    public Vector4 Rotate(Vector4 v, int axis1, int axis2, float theta)
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix[axis1, axis1] = Mathf.Cos(theta);
        matrix[axis1, axis2] = -Mathf.Sin(theta);
        matrix[axis2, axis1] = Mathf.Sin(theta);
        matrix[axis2, axis2] = Mathf.Cos(theta);

        return matrix * v;
    }

    /// <summary>
    /// Applies the 3D part of this transform to a normal Unity transform.
    /// This is used as an optimization, by offloading 3D transforms to the engine instead
    /// </summary>
    /// <param name="t3d"></param>
    public void ApplyTo(Transform t3d)
    {
        //TODO
    }
}
