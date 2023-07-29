namespace RayTracing
{
    /// <summary>
    /// A hypersphere that can be attached to a 4D gameobject in a raytracing scene
    /// </summary>
    public class RayTracedHyperSphere : RayTracedShape
    {
        /// <summary>
        /// The radius of the hypersphere
        /// </summary>
        public float radius;

        protected new void Awake()
        {
            base.Awake();

            shapeClass = ShapeClass.HyperSphere;
        }
    }
}