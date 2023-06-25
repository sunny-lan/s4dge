using RasterizationRenderer;
using UnityEngine;
using v2;

public class TetMesh : RayTracedShape
{
    public TetMesh4D_tmp mesh= new();

    protected new void Awake()
    {
        base.Awake();

        shapeClass = ShapeClass.Tet;

        mesh = new TetMesh4D_tmp();
        mesh.Append(new Vector4[]
        {
            new Vector4(1,-1,0,0),
            new Vector4(-1,-1,0,0),
            new Vector4(0,1,1,0),
            new Vector4(0,1,-1,0),
        });
        //HypercubeGenerator.GenerateHypercube(mesh);
    }
}

// Represents a single tet mesh, by indicating the range of the indices
// that form this mesh. The same transform is applied to each tet in the mesh

public struct TetMesh_shader
{
    public TransformMatrixAffine4D inverseTransform;

    // Inclusive, exclusive
    public int idxStart, idxEnd;
}
