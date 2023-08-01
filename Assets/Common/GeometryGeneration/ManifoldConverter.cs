
using Manifold3D = System.Func<UnityEngine.Vector3, UnityEngine.Vector4>;
using Manifold1D = System.Func<float, UnityEngine.Vector4>;
using System;
using UnityEngine;

public class ManifoldConverter
{
    /// <summary>
    /// Converts a line path to a hypercylinder path.
    /// Visually, converts an infinitely thin line into a noodle.
    /// </summary>
    /// <param name="line">The line to thicken</param>
    /// <param name="radius">The radius function for the line</param>
    /// <param name="frenetFrame">The frenet frame at each point on the line</param>
    /// <param name="sphereDivisions">The number of divisions the construct the sphere from</param>
    /// <returns>The parametric equation for the thickened line</returns>
    public static ParametricShape3D HyperCylinderify(ParametricShape1D line, Func<float, float> radius, Func<float, Frame4D> frenetFrame, float sphereDivisions = 6)
    {
        return new()
        {
            Equation = p =>
            {
                // p = [sphere_azi, sphere_elevation, line_position]
                float r = radius(p.z);
                float c_r = r * Mathf.Cos(p.y);
                Vector4 sphere = new(
                    0, //T
                    c_r * Mathf.Cos(p.x),
                    c_r * Mathf.Sin(p.x),
                    r * Mathf.Sin(p.y)
                );
                return line.Path(p.z) + frenetFrame(p.z) * sphere;
            },
            Normal = p =>
            {
                float r = radius(p.z);
                float c_r = r * Mathf.Cos(p.y);
                Vector4 sphere = new(
                    0, //T
                    c_r * Mathf.Cos(p.x),
                    c_r * Mathf.Sin(p.x),
                    r * Mathf.Sin(p.y)
                );
                return frenetFrame(p.z) * sphere;
            },
            Bounds = new ParameterBounds3D(
                lo: new(0, 0, line.Start),
                hi: new(2 * Mathf.PI, 2 * Mathf.PI, line.End),
                interval: new Vector3(
                    2 * Mathf.PI / sphereDivisions,
                    2 * Mathf.PI / sphereDivisions,
                    (line.End - line.Start) / line.Divisions
                )
            )
        };
    }


    public static ParametricShape3D HyperCylinderify(ParametricShape1D line, Func<float, float> radius, float sphereDivisions = 6)
    {
        return HyperCylinderify(line, radius, line.Path.FrenetFrame(), sphereDivisions);
    }
}