
using System;
using System.Collections.Generic;
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
    public static Vector4 withY(this Vector4 v, float y)
    {
        return new(v.x, y, v.z, v.w);
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

    public static T Min<T>(T a, T b) where T : IComparable<T>
    {
        if (a == null) return b;
        if (b == null) return a;
        return a.CompareTo(b) < 0 ? a : b;
    }

    public static void Swap<T>(ref T a, ref T b)
    {
        T tmp = a;
        a = b;
        b = tmp;
    }

    //
    // copied from stackoverflow
    //
    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
    Func<TSource, TKey> selector)
    {
        return source.MinBy(selector, null);
    }

    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector, IComparer<TKey> comparer)
    {
        if (source == null) throw new ArgumentNullException("source");
        if (selector == null) throw new ArgumentNullException("selector");
        comparer ??= Comparer<TKey>.Default;

        using (var sourceIterator = source.GetEnumerator())
        {
            if (!sourceIterator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
            var min = sourceIterator.Current;
            var minKey = selector(min);
            while (sourceIterator.MoveNext())
            {
                var candidate = sourceIterator.Current;
                var candidateProjected = selector(candidate);
                if (comparer.Compare(candidateProjected, minKey) < 0)
                {
                    min = candidate;
                    minKey = candidateProjected;
                }
            }
            return min;
        }
    }

    public static Vector4 CrossProduct4D(Vector4 a, Vector4 b, Vector4 c)
    {
        return new Vector4(
            a.y * (b.z * c.w - b.w * c.z) - a.z * (b.y * c.w - b.w * c.y) - a.w * (b.y * c.z - b.z * c.y),
            a.x * (b.z * c.w - b.w * c.z) - a.z * (b.x * c.w - b.w * c.x) - a.w * (b.x * c.z - b.z * c.x),
            a.x * (b.y * c.w - b.w * c.y) - a.y * (b.x * c.w - b.w * c.x) - a.w * (b.x * c.y - b.y * c.x),
            a.x * (b.y * c.z - b.z * c.y) - a.y * (b.x * c.z - b.z * c.x) - a.z * (b.x * c.y - b.y * c.x)
        );
    }
}
