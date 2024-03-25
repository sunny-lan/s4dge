using RasterizationRenderer;
using UnityEngine;
using S4DGE;

/// <summary>
/// This component represents a 4D tetrahedral mesh that can be
/// attached to any 4D GameObject within a Ray Tracing scene.
/// The mesh will appear at the position and orientation specified by the Transform4D
/// component attached to the 4D GameObject.
/// </summary>
namespace RaytraceRenderer
{
    public class TetMeshRenderer : RayTracedShape
    {
        /// <summary>
        /// The 4D tetrahedral mesh to render.
        /// </summary>
        public TetMesh_UnityObj mesh;

        protected new void Awake()
        {
            base.Awake();

            shapeClass = ShapeClass.TetMesh;
        }
    }

    public struct TetMesh_shaderdata
    {
        public TransformMatrixAffine4D inverseTransform;

        // Inclusive, exclusive
        public int idxStart, idxEnd;

        public RayTracingMaterial material;
    }
}