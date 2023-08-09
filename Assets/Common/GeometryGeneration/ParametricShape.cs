
using UnityEngine;

using Manifold3D = System.Func<UnityEngine.Vector3, UnityEngine.Vector4>;
using Manifold1D = System.Func<float, UnityEngine.Vector4>;
using System;

public struct ParameterBounds3D
{
    public Vector3 lo;
    public Vector3 hi;
    public Vector3 samplingInterval;

    public ParameterBounds3D(Vector3 lo, Vector3 hi, float interval) : this(lo, hi, Vector3.one * interval) { }
    public ParameterBounds3D(Vector3 lo, Vector3 hi, Vector3 interval)
    {
        this.lo = lo;
        this.hi = hi;
        this.samplingInterval = interval;
    }
}

public struct ParametricShape1D
{
    public Manifold1D Path;
    public float Start, End, Divisions;
}

public struct ParametricShape3D
{
    public Manifold3D Position;
    public Manifold3D Normal;
    public ParameterBounds3D Bounds;
}

public static class ParametricUtils
{
    /// <summary>
    /// Derivative of the path by path length
    /// </summary>
    /// <param name="f">The function to derive</param>
    /// <param name="s">Point on the line</param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public static Vector4 DerivativeByDsAt(this Manifold1D f, float s, float epsilon = 0.005f)
    {
        var a = f(s + epsilon);
        var b = f(s - epsilon);
        return (a - b).normalized;
    }

    public static Manifold1D DerivativeByDs(this Manifold1D f, float epsilon = 0.005f)
    {
        return s => f.DerivativeByDsAt(s, epsilon);
    }

    public static (Vector4 direction, float magnitude) Split(this Vector4 v)
    {
        return (v.normalized, v.magnitude);
    }

    public static Manifold1D Normalized(this Manifold1D v)
    {
        return s => v(s).normalized;
    }

    // https://www.reed.edu/physics/faculty/wheeler/documents/Miscellaneous%20Math/N-dimensional%20Frenet-Serret.pdf
    // https://bearworks.missouristate.edu/cgi/viewcontent.cgi?article=4469&context=theses#:~:text=The%20Frenet%20equations%20describe%20the,Frenet%20frame%20for%20n%2Ddimensions.&text=Two%20vectors%20are%20orthogonal%20to,their%20dot%20product%20is%20zero.&text=%E2%88%A5%20%E2%89%A5%200%20and%20N%20is%20orthogonal%20to%20T.
    public static Func<float, Frame4D> FrenetFrame(this Manifold1D p, float epsilon = 0.005f)
    {
        var T = p.DerivativeByDs(epsilon);
        var N = T.DerivativeByDs(epsilon);
        Manifold1D b = s =>
        {
            var dNdS = N.DerivativeByDsAt(s, epsilon);
            var t = T(s);
            return dNdS - Vector4.Dot(dNdS, t) * t;
        };
        var B = b.Normalized();
        return s =>
        {
            var dBds = B.DerivativeByDsAt(s, epsilon);
            var t = T(s);
            var n = N(s);
            var b = B(s);
            var d = dBds - Vector4.Dot(dBds, t) * t - Vector4.Dot(dBds, n) * n;
            return new()
            {
                T = t,
                N = n,
                B = b,
                D = d,
            };
        };
    }

    public static Matrix4x4[] GetFrames(this ParametricShape1D p)
    {
        Matrix4x4[] res = new Matrix4x4[Mathf.CeilToInt(p.Divisions) + 1];
        Matrix4x4 curFrame = Matrix4x4.identity;

        for (int i = 0; i <= p.Divisions; i++)
        {
            float s = i * (p.End - p.Start) / p.Divisions + p.Start;
            float s1 = (i+1) * (p.End - p.Start) / p.Divisions + p.Start;
            Vector4 tangent = p.Path(s1) - p.Path(s);

            curFrame = curFrame.ParallelTransport(tangent);
            res[i] = curFrame;
        }

        return res;
    }

    public static Func<float, Matrix4x4> ParallelTransport(this ParametricShape1D p)
    {
        var frames = GetFrames(p);
        return s =>
        {
            int idx = Mathf.RoundToInt((s - p.Start)*p.Divisions/(p.End-p.Start));
            idx = Math.Clamp(idx, 0, frames.Length - 1);
            return frames[idx]; 
        };
    }
}

public struct Frame4D
{
    public Vector4 T, N, B, D;

    public static Vector4 operator *(Frame4D lhs, Vector4 rhs)
    {
        return lhs.T * rhs.x + lhs.N * rhs.y + lhs.B * rhs.z + lhs.D * rhs.w;
    }

    public override string ToString()
    {
        return $"{T} {N} {B} {D}";
    }
}