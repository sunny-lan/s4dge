
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
}
