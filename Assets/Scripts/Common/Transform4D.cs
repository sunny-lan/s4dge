using System;
using System.Collections.Generic;
using UnityEngine;

namespace v2
{

    public enum Rot4D
    {
        xy = 0,
        xz,
        xw,
        yz,
        yw,
        zw
    }


    /// <summary>
    /// Represents a translation/rotation/scaling in 4D
    /// </summary>
    [ExecuteAlways]
    public class Transform4D : MonoBehaviour
    {
        public const int ROTATION_DOF = 6;

        public Vector4 localPosition;
        [HideInInspector]
        public float[] localRotation = new float[ROTATION_DOF];
        public Vector4 localScale = Vector4.one;

        static int[] rotationOrder = new int[ROTATION_DOF] { // The order of operation for these rotation does not match the order by which we generate them
            (int)Rot4D.xz,
            (int)Rot4D.yz, 
            (int)Rot4D.xy,
            (int)Rot4D.xw,
            (int)Rot4D.yw,
            (int)Rot4D.zw,
        };

        // caches the Transform4D of the parent for performance 
        Transform4D _parent;
        public Transform4D parent
        {
            get => _parent;
            set
            {
                _parent = value;
                transform.parent = value?.transform; //parent can be null
            }
        }

        void calculateParent()
        {
            if (transform.parent == null)
                parent = null;
            else
            {
                parent = transform.parent.GetComponent<Transform4D>();
                if (parent == null)
                    throw new NullReferenceException("Parent of Transform4D must also have a Transform4D");
            }
        }

        private void OnTransformParentChanged() => calculateParent();

        private void Awake()
        {
            calculateParent(); //make sure parent is initialized
        }

        /// <summary>
        /// Access the 3D part of rotation as euler angles (radians)
        /// </summary>
        public Vector3 localEulerAngles3D
        {
            get => new(
                localRotation[(int)Rot4D.yz],
                localRotation[(int)Rot4D.xz],
                localRotation[(int)Rot4D.xy]
            );
            set
            {
                localRotation[(int)Rot4D.yz] = value.x;
                localRotation[(int)Rot4D.xz] = value.y;
                localRotation[(int)Rot4D.xy] = value.z;
            }
        }

        public Quaternion localRotation3D
        {
            get => Quaternion.Euler(180 * localEulerAngles3D / Mathf.PI);
            set => localEulerAngles3D = Mathf.PI * value.eulerAngles / 180;
        }

        public Vector3 localPosition3D
        {
            get => localPosition.XYZ();
            set => localPosition = value.withW(localPosition.w);
        }

        public Vector4 LocalDirectionToWorld(Vector4 direction) => LocalToWorld(direction) - LocalToWorld(Vector4.zero);

        public Vector4 WorldDirectionToLocal(Vector4 direction) => WorldToLocal(direction) - WorldToLocal(Vector4.zero);

        // TODO idk if these are valid when rotation in 4D is non zero
        public Vector4 forward => LocalDirectionToWorld(Vector3.forward);
        public Vector4 left => LocalDirectionToWorld(Vector3.left);
        public Vector4 right => LocalDirectionToWorld(Vector3.right);
        public Vector4 back => LocalDirectionToWorld(Vector3.back);

        public Vector4 position
        {
            get => LocalToWorld(Vector4.zero);
            set => localPosition += WorldToLocal(value);
        }

        /// <summary>
        /// Gets the local to world transform as a matrix. TODO can be optimized
        /// </summary>
        public TransformMatrixAffine4D localToWorldMatrix => parent switch
        {
            null => localMatrix,
            Transform4D p => p.localToWorldMatrix * localMatrix
        };

        /// <summary>
        /// Gets local transform as a matrix. TODO can be cached
        /// </summary>
        public TransformMatrixAffine4D localMatrix => asMatrix();

        public TransformMatrixAffine4D worldToLocalMatrix => localToWorldMatrix.inverse;
        public TransformMatrixAffine4D inverselocalMatrix => localMatrix.inverse;


        // calculate local transform as matrix
        TransformMatrixAffine4D asMatrix()
        {
            return new()
            {
                scaleAndRot = RotationMatrix(localRotation, scaling: localScale),
                translation = localPosition,
            };
        }

        /// <summary>
        /// applies local transform to point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector4 ApplyLocalTransform(Vector4 point)
        {
            Vector4 v = Vector4.Scale(localScale, point);
            return RotationMatrix(localRotation) * v + localPosition;
        }

        public Ray4D ApplyLocalTransform(Ray4D ray)
        {
            return new Ray4D { src = ApplyLocalTransform(ray.src), direction = Rotate(ray.direction, localRotation) };
        }

        public Ray4D WorldToLocal(Ray4D ray)
        {
            var src = WorldToLocal(ray.src);
            return new()
            {
                src = src,
                direction = WorldToLocal(ray.src + ray.direction) - src,
            };
        }

        public Vector4 InverseLocalTransform(Vector4 point)
        {
            point -= localPosition;
            point = RotationMatrix(localRotation).inverse * point;
            point = InverseLocalScale((point), localScale);
            return point;
        }

        private Vector4 InverseLocalScale(Vector4 v, Vector4 divisors)
        {
            Vector4 componentInverse = Vector4.one;
            for (int i = 0; i < 4; i++)
            {
                if (divisors[i] == 0)
                    Debug.LogWarning("ERROR: Scale vector with a 0 component attempted division by 0: " + divisors);
                componentInverse[i] = 1.0f / divisors[i];
            }
            return Vector4.Scale(v, componentInverse);
        }

        /// <summary>
        /// Rotates given point around 6 planes of rotation in 4d
        /// </summary>
        /// <param name="v"> The point for which a rotated version is returned </param>
        /// <param name="allRotations"> An array of 6 floats representing rotation around each plane </param>
        /// <returns> The rotated vector </returns>
        private Vector4 Rotate(Vector4 v, float[] allRotations)
        {
            int axisCount = 0;
            Vector4 r = v;
            for (int i = 0; i < 3; ++i)
            {
                for (int j = i + 1; j < 4; ++j)
                {
                    r = Rotate(r, i, j, allRotations[axisCount]);
                    axisCount++;
                }
            }
            return r;
        }

        // can also apply scaling by setting initialScale
        private static Matrix4x4 RotationMatrix(float[] allRotations, Vector4? scaling = null)
        {
            int axisCount = 0;
            Matrix4x4 total = scaling switch
            {
                null => Matrix4x4.identity,
                Vector4 scale => new Matrix4x4(
                    new(scale.x, 0, 0, 0),
                    new(0, scale.y, 0, 0),
                    new(0, 0, scale.z, 0),
                    new(0, 0, 0, scale.w)
                )
            };
            
            Matrix4x4[] matrices = new Matrix4x4[ROTATION_DOF];
            for (int i = 0; i < 3; ++i)
            {
                for (int j = i + 1; j < 4; ++j)
                {
                    matrices[axisCount] = RotationMatrix(i, j, allRotations[axisCount]);
                    axisCount++;
                }
            }
            // Apply our calculated rotation matrices in predefined order
            for (int i = 0; i < ROTATION_DOF; i++)
            {
                total = total * matrices[rotationOrder[i]];
            }
            return total;
        }

        /// <summary>
        /// Rotates the given point around theta radians around the defined plane
        /// </summary>
        /// <param name="v"> The point for which a rotated version is returned </param>
        /// <param name="axis1"> The first axis of the plane of rotation </param>
        /// <param name="axis2"> The second axis of the plane of rotation </param>
        /// <param name="theta"> The angle of rotation in radians </param>
        /// <returns> The rotated vector </returns>
        public Vector4 Rotate(Vector4 v, int axis1, int axis2, float theta)
        {
            return RotationMatrix(axis1, axis2, theta) * v;
        }

        public static Matrix4x4 RotationMatrix(int axis1, int axis2, float theta)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix[axis1, axis1] = Mathf.Cos(theta);
            matrix[axis1, axis2] = -Mathf.Sin(theta);
            matrix[axis2, axis1] = Mathf.Sin(theta);
            matrix[axis2, axis2] = Mathf.Cos(theta);

            return matrix;
        }

        /// <summary>
        /// Transforms a point from local coordinates to global coordinates
        /// </summary>
        public Vector4 LocalToWorld(Vector4 p)
        {
            // apply all transforms of parents (TODO performance)
            var currentT4D = this;
            do
            {
                p = currentT4D.ApplyLocalTransform(p);
                currentT4D = currentT4D.parent;
            } while (currentT4D != null);
            return p;
        }

        // temp list for use in WorldToLocal
        static List<Transform4D> tmp_transformations = new();

        /// <summary>
        /// Transforms a point from global coordinates to local coordinates
        /// </summary>
        public Vector4 WorldToLocal(Vector4 p)
        {
            Debug.Assert(tmp_transformations.Count == 0);

            // get all parent transforms and store in temp list
            var currentT4D = this;
            do
            {
                tmp_transformations.Add(currentT4D);
                currentT4D = currentT4D.parent;
            } while (currentT4D != null);

            // apply all transforms of parents in reverse order (TODO performance)
            for (int i = tmp_transformations.Count - 1; i >= 0; i--)
            {
                p = tmp_transformations[i].InverseLocalTransform(p);
            }

            tmp_transformations.Clear();
            return p;
        }

        // Apply details from a 3D TRS camera matrix to this t4d
        public void ApplyTransform3D(Matrix4x4 worldToLocal3D)
        {
            localEulerAngles3D = Quaternion.LookRotation(worldToLocal3D.GetColumn(2), worldToLocal3D.GetColumn(1)).eulerAngles * Mathf.PI / 180f;
            localRotation[(int)Rot4D.xz] = (Mathf.PI - localRotation[(int)Rot4D.xz]); // Reverse the z forward direction - cameras use OpenGL standard which has -z axis
            localRotation[(int)Rot4D.yz] =  -localRotation[(int)Rot4D.yz]; // TODO: force flip left/right camera controls, not sure why
            
            localPosition3D = worldToLocal3D.GetPosition();
            localScale = worldToLocal3D.lossyScale.withW(localScale.w);
        }
    }

}