using RasterizationRenderer;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using Manifold2D = System.Func<UnityEngine.Vector2, UnityEngine.Vector4>;
using Manifold3D = System.Func<UnityEngine.Vector3, UnityEngine.Vector4>;

public class MeshGeneratorUtils
{
    public struct Dimension3D
    {
        public int x;
        public int y;
        public int z;

        public Dimension3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public struct HexMesh4D
    {
        // 3D array storing where 3D points in the parametric space map to 4D points
        public Vector4[,,] vertices;
        public Vector4[,,] normals;

        public HexMesh4D(int xSize, int ySize, int zSize)
        {
            vertices = new Vector4[xSize, ySize, zSize];
            normals = new Vector4[xSize, ySize, zSize];
        }

        public readonly Dimension3D GetDimension()
        {
            return new(vertices.GetLength(0), vertices.GetLength(1), vertices.GetLength(2));
        }
    }

    public class ManifoldThickener
    {

        public static (Manifold3D, Manifold3D) ThickenManifold2D(Manifold2D mf2d, Manifold2D tangent1, Manifold2D tangent2, float thickenRadius)
        {
            // get functions that find the two normal vectors at a point on the 2D manifold
            // in a calculated cartesian frame
            var (a, b, c) = GetCartesianFrame(mf2d, tangent1, tangent2);
            Manifold2D normal1 = a;
            Vector4 normal2(Vector2 params2D) => Util.CrossProduct4D(a(params2D), b(params2D), c(params2D));

            // Now thicken the 2D manifold by attaching a circle to each point on the 2D manifold on the normal plane
            Vector4 normalGenerator(Vector3 params3D)
            {
                Vector2 mf2DPoint = new(params3D.x, params3D.y);
                float theta = params3D.z;

                // Assert.AreApproximatelyEqual(Vector4.Dot(a(mf2DPoint), b(mf2DPoint)), 0);
                // Assert.AreApproximatelyEqual(Vector4.Dot(b(mf2DPoint), c(mf2DPoint)), 0);
                // Assert.AreApproximatelyEqual(Vector4.Dot(a(mf2DPoint), c(mf2DPoint)), 0);
                // Assert.AreApproximatelyEqual(Vector4.Dot(c(mf2DPoint), normal2(mf2DPoint)), 0);
                // Assert.AreApproximatelyEqual(Vector4.Dot(b(mf2DPoint), normal2(mf2DPoint)), 0);
                // Assert.AreApproximatelyEqual(Vector4.Dot(a(mf2DPoint), normal2(mf2DPoint)), 0);

                return Mathf.Cos(theta) * normal1(mf2DPoint) + Mathf.Sin(theta) * normal2(mf2DPoint);
            }
            Vector4 positionGenerator(Vector3 params3D)
            {
                Vector2 mf2DPoint = new(params3D.x, params3D.y);
                float theta = params3D.z;
                return mf2d(mf2DPoint) + thickenRadius * normalGenerator(params3D);
            }

            return (positionGenerator, normalGenerator);
        }

        private static (Manifold2D, Manifold2D, Manifold2D) GetCartesianFrame(Manifold2D mf2d, Manifold2D tangent1, Manifold2D tangent2)
        {
            Manifold2D a = params2d => Vector4.Normalize(mf2d(params2d));
            Manifold2D b = params2d =>
            {
                Vector4 tangent1Value = tangent1(params2d);
                Vector4 onto = a(params2d);
                return Vector4.Normalize(tangent1Value - Vector4.Project(tangent1Value, onto));
            };
            Manifold2D c = params2d =>
            {
                Vector4 tangent2Value = tangent2(params2d);
                Vector4 onto = b(params2d);
                return Vector4.Normalize(tangent2Value - Vector4.Project(tangent2Value, onto));
            };

            return (a, b, c);
        }
    }

    public static void MakeTetsForwardFacing(TetMesh4D.Tet4D[] tets, TetMesh4D.VertexData[] meshVertices)
    {
        for (int idx = 0; idx < tets.Length; ++idx)
        {
            int p0Idx = tets[idx].tetPoints[0];
            int p1Idx = tets[idx].tetPoints[1];
            int p2Idx = tets[idx].tetPoints[2];
            int p3Idx = tets[idx].tetPoints[3];

            Vector4 p0Pos = meshVertices[p0Idx].position;
            Vector3 v1 = (meshVertices[p1Idx].position - p0Pos);
            Vector3 v2 = (meshVertices[p2Idx].position - p0Pos);
            Vector3 v3 = (meshVertices[p3Idx].position - p0Pos);

            int volumeSign = Math.Sign(Vector3.Dot(v1, Vector3.Cross(v2, v3)));

            // If the normal for p0 is pointing in the negative w-direction
            // The signed volume of the tetrahedron should be negative and vice-versa
            if (volumeSign < 0)
            {
                (tets[idx].tetPoints[2], tets[idx].tetPoints[3]) =
                    (tets[idx].tetPoints[3], tets[idx].tetPoints[2]);
            }
        }
    }
}
