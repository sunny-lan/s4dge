using RasterizationRenderer;
using UnityEngine;

public class RasterizeHypersphere : RasterizeObject
{
    public float samplingInterval;

    protected override void InitGeometry()
    {
        tetMeshRenderer.SetTetMesh(MeshGenerator4D.GenerateHypersphereMesh(samplingInterval));
    }
}
