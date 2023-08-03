using RasterizationRenderer;
using UnityEngine;

public class Rasterize3Torus : RasterizeObject
{

    public float thickness;
    public float samplingInterval;

    protected override void InitGeometry()
    {
        tetMeshRenderer.SetTetMesh(MeshGenerator4D.Generate3TorusMesh(samplingInterval, thickness));
    }
}
