
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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
            a.z * (b.y * c.w - b.w * c.y) - a.y * (b.z * c.w - b.w * c.z) - a.w * (b.y * c.z - b.z * c.y),
            a.x * (b.z * c.w - b.w * c.z) - a.z * (b.x * c.w - b.w * c.x) + a.w * (b.x * c.z - b.z * c.x),
            -a.x * (b.y * c.w - b.w * c.y) + a.y * (b.x * c.w - b.w * c.x) - a.w * (b.x * c.y - b.y * c.x),
            a.x * (b.y * c.z - b.z * c.y) - a.y * (b.x * c.z - b.z * c.x) - a.z * (b.x * c.y - b.y * c.x)
        );
    }

    /// <summary>
    /// Finds the minimum rotation matrix between two unit vectors
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Matrix4x4 RotationBetween(Vector4 u, Vector4 v)
    {
        return Matrix4x4.identity.Subtract( MultiplyTranspose(u+v / (1 + Vector4.Dot(u, v)), u+v));
    }

    /// <summary>
    /// Multiplies column vector by row vector, resulting in a matrix
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public static Matrix4x4 MultiplyTranspose(Vector4 col, Vector4 row)
    {
        Matrix4x4 res = new();
        for (int i = 0; i < 4; i++)
            res.SetColumn(i, row[i] * col);
        return res;
    }

    public static Matrix4x4 Subtract(this Matrix4x4 m, Matrix4x4 s)
    {
        var res= new Matrix4x4();
        for (int i = 0; i < 4; i++)
            res.SetRow(i, m.GetRow(i)-s.GetRow(i));
        return res;
    }

    /// <summary>
    /// Finds 3 other vectors orthogonal to given vector.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="epsilon"></param>
    /// <returns>Matrix containing 4 orthogonal vectors as columns. Column 0 contains the given vector</returns>
    public static Matrix4x4 Orthogonal(Vector4 t, float epsilon = 0.01f)
    {
        Matrix4x4 res = new();
        res.SetColumn(0, t);
        for (int i = 1; i < 4; i++)
        {
            Vector4 res1;
            do
            {
                Vector4 v = res1 = Random();
                for (int j = 0; j < i; j++)
                {
                    res1 -= Vector4.Project(v, res.GetColumn(j));
                }
            } while (res1.magnitude < epsilon);

            res.SetColumn(1, res1.normalized);
        }
        return res;
    }

    /// <summary>
    /// Given 4 linearly independent vectors, return 4 orthogonal vectors
    /// </summary>
    /// <param name="m">Matrix containing a vector per column</param>
    /// <returns>Matrix containing orthogonal vectors</returns>
    public static Matrix4x4 GramSchmidt(Matrix4x4 m)
    {
        for (int i = 0; i < 4; i++)
        {
            Vector4 res1 = m.GetColumn(i);
            for (int j = 0; j < i; j++)
            {
                res1 -= Vector4.Project(res1, m.GetColumn(j));
            }

            m.SetColumn(i, res1.normalized);
        }
        return m;
    }


    public static Vector4 Random()
    {
        return new(
            UnityEngine.Random.value,
            UnityEngine.Random.value,
            UnityEngine.Random.value,
            UnityEngine.Random.value
        );
    }
}


