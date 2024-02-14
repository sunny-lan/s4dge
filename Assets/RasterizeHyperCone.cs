using RasterizationRenderer;
using UnityEngine;

public class RasterizeHyperCone : RasterizeObject
{
    public float thickness;
    public float samplingInterval;
    protected override void InitGeometry()
    {
        var line = new ParametricShape1D()
        {
            Divisions = 1 / samplingInterval,
            End = 1,
            Start = 0,
            Path = s =>
            {
                return new(
                    s,
                    0,
                    0,
                    0
                );
            }
        };

        var converted = ManifoldConverter.HyperCylinderify(line, s => s > 0 ? s : 0);

        var mesh = MeshGenerator4D.GenerateTetMesh(converted.Equation, converted.Normal, converted.Bounds);

        tetMeshRenderer.SetTetMesh(mesh);
    }
}
