using UnityEngine;

namespace RasterizationRenderer
{
    public class Transform4D
    {
        public Matrix4x4 rotation;
        public Vector4 translation;

        public Transform4D()
        {
            this.rotation = Matrix4x4.identity;
            this.translation = Vector4.zero;
        }

        public Transform4D(Matrix4x4 rotation, Vector4 translation)
        {
            this.rotation = rotation;
            this.translation = translation;
        }

        public Vector4 ApplyTo(Vector4 pt)
        {
            return (rotation * pt) + translation;
        }
    }
}