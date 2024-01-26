using RasterizationRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RasterizeTetMeshUnityObj : RasterizeObject
{
    public TetMesh_UnityObj rawMesh;

    protected override void InitGeometry()
    {
        tetMeshRenderer.SetTetMesh(rawMesh.mesh_Raw.ToRasterizableTetMesh());
    }
}
