using RasterizationRenderer;
using UnityEngine;
using S4DGE;

namespace RasterizationRenderer
{
    public class RasterizeHyperCone : RasterizeObject
    {
        public float thickness;
        public float samplingInterval;
        public int coneEdges = 6;

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

            var converted = ManifoldConverter.HyperCylinderify(line, s => s > 0 ? s * thickness : 0, coneEdges);

            var mesh = MeshGenerator4D.GenerateTetMesh(converted.Equation, converted.Normal, converted.Bounds);

            tetMeshRenderer.SetTetMesh(mesh);
        }
    }
}