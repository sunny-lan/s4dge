using RasterizationRenderer;
using UnityEngine;
using v2;

public class TetMeshRenderer : RayTracedShape
{
    public TetMesh_UnityObj mesh;

    protected new void Awake()
    {
        base.Awake();

        shapeClass = ShapeClass.TetMesh;
    }
}

// Represents a single tet mesh, by indicating the range of the indices
// that form this mesh. The same transform is applied to each tet in the mesh

public struct TetMesh_shaderdata
{
    public TransformMatrixAffine4D inverseTransform;

    // Inclusive, exclusive
    public int idxStart, idxEnd;

    public RayTracingMaterial material;
}
