
using System;
using UnityEngine;

public static class Util
{
    public static Vector3 XYZ(this Vector4 v)
    {
        return new(v.x, v.y, v.z);
    }

    public static Vector4 withW(this Vector3 v, float w)
    {
        return new(v.x, v.y, v.z, w);
    }

    public static float Angle(Vector4 a, Vector4 b)
    {
        return (float)Math.Acos(Vector4.Dot(a, b) / Vector4.Magnitude(a) / Vector4.Magnitude(b));
    }

    public static Vector4 LimitLength(this Vector4 v, float maxLen)
    {
        float length = v.magnitude;
        if (length > maxLen)
            return v * maxLen / length;
        else
            return v;
    }

    public static T Min<T>(T a, T b) where T:IComparable<T>
    {
        if (a == null) return b;
        if (b == null) return a;
        return a.CompareTo(b) < 0 ? a : b;
    }
}
