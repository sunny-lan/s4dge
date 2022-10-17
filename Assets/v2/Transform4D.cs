using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a translation/rotation/scaling in 4D
/// </summary>
public class Transform4D : MonoBehaviour
{
    public Vector4 position;

    /// <summary>
    /// applies transform to point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public Vector4 Transform(Vector4 point)
    {
        return point + position;
    }

    public Vector4 InverseTransform(Vector4 point)
    {
        return point - position;
    }
}
