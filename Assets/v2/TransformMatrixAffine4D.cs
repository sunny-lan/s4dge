using System;
using UnityEngine;

namespace v2
{
    /// <summary>
    /// Represents 4D transformation via affine matrix
    /// </summary>
    public struct TransformMatrixAffine4D : IEquatable<TransformMatrixAffine4D>
    {
        /*
         * Matrix in form:
         * |                        |
         * | scaleRot   translation |
         * |                        |
         * |  0 ... 0         1     |
         */

        public Matrix4x4 scaleAndRot;
        public Vector4 translation;

        public static TransformMatrixAffine4D operator *(TransformMatrixAffine4D a, TransformMatrixAffine4D b)
        {
            // working out result 5x5 matrix mult by hand gives this:
            return new()
            {
                scaleAndRot = a.scaleAndRot * b.scaleAndRot,
                translation = a.scaleAndRot * b.translation + a.translation,
            };
        }
        public static Vector4 operator *(TransformMatrixAffine4D a, Vector4 b)
        {
            return a.scaleAndRot * b + a.translation;
        }

        public TransformMatrixAffine4D inverse
        {
            get
            {
                // working out result 5x5 matrix inverse by hand gives this:
                var invScaleRot = scaleAndRot.inverse;
                return new()
                {
                    scaleAndRot = invScaleRot, // gives identity matrix in first 4x4
                    translation = invScaleRot*-translation, // want to cancel out last column to get 0
                };
            }
        }

        //
        // IDE auto-code below
        //

        public override bool Equals(object obj)
        {
            return obj is TransformMatrixAffine4D d && Equals(d);
        }

        public bool Equals(TransformMatrixAffine4D other)
        {
            return scaleAndRot.Equals(other.scaleAndRot) &&
                   translation.Equals(other.translation);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(scaleAndRot, translation);
        }

        public static bool operator ==(TransformMatrixAffine4D left, TransformMatrixAffine4D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TransformMatrixAffine4D left, TransformMatrixAffine4D right)
        {
            return !(left == right);
        }
    }
}
