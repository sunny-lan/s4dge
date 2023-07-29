
using UnityEngine;

using Manifold3D = System.Func<UnityEngine.Vector3, UnityEngine.Vector4>;
using Manifold1D = System.Func<float, UnityEngine.Vector4>;

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
    public Manifold3D Equation;
    public ParameterBounds3D Bounds;
}

public static class ParametricUtils
{

}