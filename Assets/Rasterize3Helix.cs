using RasterizationRenderer;
using UnityEngine;

public class Rasterize3Helix : RasterizeObject
{
    public float thickness;
    public float samplingInterval;
    protected override void InitGeometry()
    {
        var line = new ParametricShape1D()
        {
            Divisions = 1 / samplingInterval,
            End = 2 * Mathf.PI,
            Start = 0,
            Path = s =>
            {
                return new(
                    2 * Mathf.Sin(s),
                    2 * Mathf.Cos(s),
                    s, 0
                );
            }
        };

        var converted = ManifoldConverter.HyperCylinderify(line, s => thickness);

        var mesh = MeshGenerator4D.GenerateTetMesh(converted.Equation, converted.Normal, converted.Bounds);

        tetMeshRenderer.SetTetMesh(mesh);
    }
}
