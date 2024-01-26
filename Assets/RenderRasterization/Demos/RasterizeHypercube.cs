using RasterizationRenderer;
using UnityEngine;

public class RasterizeHypercube : RasterizeObject
{ 
    protected override void InitGeometry()
    {
        tetMeshRenderer.SetTetMesh(HypercubeGenerator.GenerateHypercube());
    }
}
