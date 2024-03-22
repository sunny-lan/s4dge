using System;
using UnityEngine;
using static MeshGeneratorUtils;
using Manifold2D = System.Func<UnityEngine.Vector2, UnityEngine.Vector4>;
using Manifold3D = System.Func<UnityEngine.Vector3, UnityEngine.Vector4>;

namespace RasterizationRenderer
{

    public class MeshGenerator4D
    {
        /*
         **************************
         * PUBLIC FUNCTIONS BEGIN *
         **************************
         */

        public static TetMesh4D GenerateHypersphereMesh(float samplingInterval)
        {
            // same set of equations generate positions and normals
            Manifold3D sphereGenerator = params3D =>
            {
                return new Vector4(
                    Mathf.Cos(params3D.x),
                    Mathf.Sin(params3D.x) * Mathf.Sin(params3D.y) * Mathf.Sin(params3D.z),
                    Mathf.Sin(params3D.x) * Mathf.Sin(params3D.y) * Mathf.Cos(params3D.z),
                    Mathf.Sin(params3D.x) * Mathf.Cos(params3D.y)
                );
            };

            // Bounds are [(0, pi), (0, pi), (0, 2pi)]
            ParameterBounds3D samplingBounds = new(
                Vector3.zero, Mathf.PI * (Vector3.one + Vector3.forward), samplingInterval
            );

            return GenerateTetMesh(sphereGenerator, sphereGenerator, samplingBounds);
        }

        public static TetMesh4D Generate3TorusMesh(float samplingInterval, float thickness)
        {
            static Vector4 Generate2Torus(Vector2 params2D)
            {
                float u = params2D.x;
                float v = params2D.y;
                return new Vector4(
                    MathF.Cos(u),
                    MathF.Sin(u),
                    MathF.Cos(v),
                    MathF.Sin(v)
                );
            }

            static Vector4 Tangent1(Vector2 params2D)
            {
                float u = params2D.x;
                float v = params2D.y;
                return new Vector4(
                    -MathF.Sin(u),
                    MathF.Cos(u),
                    0,
                    0
                );
            }

            static Vector4 Tangent2(Vector2 params2D)
            {
                float u = params2D.x;
                float v = params2D.y;
                return new Vector4(
                    0,
                    0,
                    -MathF.Sin(v),
                    MathF.Cos(v)
                );
            }

            // Bounds are [0, 2pi] for u, v, theta
            ParameterBounds3D samplingBounds = new(
                Vector3.zero, 2 * Mathf.PI * Vector3.one, samplingInterval
            );

            return GenerateThickenedTetMesh(Generate2Torus, Tangent1, Tangent2, samplingBounds, thickness);
        }

        // tangent1: partial derivative of each output component of mf2d with respect to 1st variable (u)
        // tangent2: partial derivative of each output component of mf2d with respect to 2nd variable (v)
        // returns the position and normal generators for the thickened 3D manifold
        public static TetMesh4D GenerateThickenedTetMesh(Manifold2D mf2d, Manifold2D tangent1, Manifold2D tangent2, ParameterBounds3D samplingBounds, float thickenRadius)
        {
            var (positionGenerator, normalGenerator) = ManifoldThickener.ThickenManifold2D(mf2d, tangent1, tangent2, thickenRadius);
            return GenerateTetMesh(positionGenerator, normalGenerator, samplingBounds);
        }

        public static TetMesh4D GenerateTetMesh(Manifold3D positionGenerator, Manifold3D normalGenerator, ParameterBounds3D samplingBounds)
        {
            var hexMesh = GenerateHexMesh(positionGenerator, normalGenerator, samplingBounds);
            return Generate6TetMeshFromHexMesh(hexMesh);
        }

        public static int FlattenCoord3D(int x, int y, int z, Dimension3D dim)
        {
            return x * dim.y * dim.z + y * dim.z + z;
        }

        public static TetMesh4D Generate6TetMeshFromHexMesh(HexMesh4D hexMesh)
        {
            var hexMeshDimension = hexMesh.GetDimension();
            TetMesh4D.VertexData[] meshVertices = new TetMesh4D.VertexData[hexMeshDimension.x * hexMeshDimension.y * hexMeshDimension.z];
            int numHexahedra = (hexMeshDimension.x - 1) * (hexMeshDimension.y - 1) * (hexMeshDimension.z - 1);
            int tetPerHex = 6;
            TetMesh4D.Tet4D[] tetrahedra = new TetMesh4D.Tet4D[numHexahedra * tetPerHex];
            int tetIdx = 0;

            for (int x = 0; x < hexMeshDimension.x; ++x)
            {
                for (int y = 0; y < hexMeshDimension.y; ++y)
                {
                    for (int z = 0; z < hexMeshDimension.z; ++z)
                    {
                        meshVertices[FlattenCoord3D(x, y, z, hexMeshDimension)].position = hexMesh.vertices[x, y, z];
                        meshVertices[FlattenCoord3D(x, y, z, hexMeshDimension)].normal = hexMesh.normals[x, y, z];
                        meshVertices[FlattenCoord3D(x, y, z, hexMeshDimension)].worldPosition4D = hexMesh.vertices[x, y, z];

                        if (x > 0 && y > 0 && z > 0)
                        {
                            // Performs 6-tetrahedra decomposition to decompose the cube into tetrahedra
                            // NOTE: order of the vertices needs to be p0, (p1, p2, p3 in clockwise order) for back-culling
                            int[] vertices = new int[] {
                                FlattenCoord3D(x, y, z, hexMeshDimension),
                                FlattenCoord3D(x, y, z - 1, hexMeshDimension),
                                FlattenCoord3D(x, y - 1, z, hexMeshDimension),
                                FlattenCoord3D(x, y - 1, z - 1, hexMeshDimension),
                                FlattenCoord3D(x - 1, y, z, hexMeshDimension),
                                FlattenCoord3D(x - 1, y, z - 1, hexMeshDimension),
                                FlattenCoord3D(x - 1, y - 1, z, hexMeshDimension),
                                FlattenCoord3D(x - 1, y - 1, z - 1, hexMeshDimension),
                            };

                            // (0, 0, 0) -> (1, 0, 0) -> (1, 1, 0) -> (0, 1, 0)
                            tetrahedra[tetIdx] = new TetMesh4D.Tet4D(new int[]
                            {
                                vertices[0], vertices[4], vertices[6], vertices[1]
                            });

                            // (0, 0, 0) -> (0, 1, 0) -> (0, 0, 1) -> (1, 1, 0)
                            tetrahedra[tetIdx + 1] = new TetMesh4D.Tet4D(new int[]
                            {
                                vertices[0], vertices[2], vertices[1], vertices[6]
                            });

                            // (0, 1, 0) -> (0, 1, 1) -> (0, 0, 1) -> (1, 1, 0)
                            tetrahedra[tetIdx + 2] = new TetMesh4D.Tet4D(new int[]
                            {
                                vertices[2], vertices[3], vertices[1], vertices[6]
                            });

                            // (1, 0, 0) -> (1, 0, 1) -> (1, 1, 0) -> (0, 0, 1)
                            tetrahedra[tetIdx + 3] = new TetMesh4D.Tet4D(new int[]
                            {
                                vertices[4], vertices[5], vertices[6], vertices[1]
                            });

                            // (0, 0, 1) -> (1, 1, 1) -> (1, 0, 1) -> (1, 1, 0)
                            tetrahedra[tetIdx + 4] = new TetMesh4D.Tet4D(new int[]
                            {
                                vertices[1], vertices[7], vertices[5], vertices[6]
                            });

                            // (0, 0, 1) -> (0, 1, 1) -> (1, 1, 1) -> (1, 1, 0)
                            tetrahedra[tetIdx + 5] = new TetMesh4D.Tet4D(new int[]
                            {
                                vertices[1], vertices[3], vertices[7], vertices[6]
                            });

                            tetIdx += tetPerHex;
                        }
                    }
                }
            }

            return new TetMesh4D(meshVertices, tetrahedra);
        }

        public static HexMesh4D GenerateHexMesh(Manifold3D positionGenerator, Manifold3D normalGenerator, ParameterBounds3D samplingBounds)
        {
            int xSize = (int)(Mathf.Ceil((samplingBounds.hi.x - samplingBounds.lo.x) / samplingBounds.samplingInterval.x)) + 1;
            int ySize = (int)(Mathf.Ceil((samplingBounds.hi.y - samplingBounds.lo.y) / samplingBounds.samplingInterval.y)) + 1;
            int zSize = (int)(Mathf.Ceil((samplingBounds.hi.z - samplingBounds.lo.z) / samplingBounds.samplingInterval.z)) + 1;
            HexMesh4D hexMesh = new HexMesh4D(xSize, ySize, zSize);

            // sample parametric equations positionGenerator, normalGenerator at intervals to generate a hexahedral (cube) Mesh
            for (int xSample = 0; xSample < xSize; ++xSample)
            {
                float u = samplingBounds.lo.x + xSample * samplingBounds.samplingInterval.x;
                for (int ySample = 0; ySample < ySize; ++ySample)
                {
                    float v = samplingBounds.lo.y + ySample * samplingBounds.samplingInterval.y;
                    for (int zSample = 0; zSample < zSize; ++zSample)
                    {
                        float t = samplingBounds.lo.z + zSample * samplingBounds.samplingInterval.z;

                        Vector3 sampledParams = new(u, v, t);
                        hexMesh.vertices[xSample, ySample, zSample] = positionGenerator(sampledParams);
                        hexMesh.normals[xSample, ySample, zSample] = normalGenerator(sampledParams);
                    }
                }
            }

            return hexMesh;
        }


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
    }

}
