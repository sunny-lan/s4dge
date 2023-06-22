using UnityEngine;

namespace RasterizationRenderer
{
    public class Transform4D
    {
        public Matrix4x4 rotation;
        public Vector4 scale;
        public Vector4 translation;

        public Transform4D()
        {
            this.rotation = Matrix4x4.identity;
            this.scale = Vector4.one;
            this.translation = Vector4.zero;
        }

        public Transform4D(Matrix4x4 rotation, Vector4 scale, Vector4 translation)
        {
            this.rotation = rotation;
            this.scale = scale;
            this.translation = translation;
        }

        public Vector4 ApplyTo(Vector4 pt)
        {
            return (rotation * Vector4.Scale(pt, scale)) + translation;
        }

        public void ApplyScale(Vector4 amount)
        {
            this.scale = Vector4.Scale(this.scale, amount);
        }
    }
}