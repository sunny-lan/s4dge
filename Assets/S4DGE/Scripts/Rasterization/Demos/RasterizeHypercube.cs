using RasterizationRenderer;
using UnityEngine;
using S4DGE;

namespace RasterizationRenderer
{
    public class RasterizeHypercube : RasterizeObject
    { 
        protected override void InitGeometry()
        {
            tetMeshRenderer.SetTetMesh(HypercubeGenerator.GenerateHypercube());
        }
    }
}