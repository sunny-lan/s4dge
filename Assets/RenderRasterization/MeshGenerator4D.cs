using UnityEngine;

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

        public static void GenerateHypersphereMesh(TetMesh4D tetMesh, float samplingInterval)
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

            ParameterBounds samplingBounds = new(
                Vector3.zero, Mathf.PI * Vector3.one, samplingInterval
            );

            GenerateTetMesh(tetMesh, sphereGenerator, sphereGenerator, samplingBounds);
        }

        public static void GenerateTetMesh(TetMesh4D tetMesh, Manifold3D positionGenerator, Manifold3D normalGenerator, ParameterBounds samplingBounds)
        {
            var hexMesh = GenerateHexMesh(positionGenerator, normalGenerator, samplingBounds);
            Generate6TetMeshFromHexMesh(tetMesh, hexMesh);
        }

        /*
         **************************
         * PUBLIC FUNCTIONS END *
         **************************
         */

        private static int FlattenCoord3D(int x, int y, int z, Dimension3D dim)
        {
            return x * dim.y * dim.z + y * dim.z + z;
        }

        // Generates tetrahedral mesh using 6-tetrahedra algorithm
        private static void Generate6TetMeshFromHexMesh(TetMesh4D tetMesh, HexMesh4D hexMesh)
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
                            vertices[0], vertices[4], vertices[6], vertices[2]
                            });

                            // (0, 0, 0) -> (0, 1, 0) -> (1, 1, 0) -> (0, 0, 1)
                            tetrahedra[tetIdx + 1] = new TetMesh4D.Tet4D(new int[]
                            {
                            vertices[0], vertices[2], vertices[6], vertices[1]
                            });

                            // (0, 1, 0) -> (0, 1, 1) -> (0, 0, 1) -> (1, 1, 0)
                            tetrahedra[tetIdx + 1] = new TetMesh4D.Tet4D(new int[]
                            {
                            vertices[2], vertices[3], vertices[1], vertices[6]
                            });

                            // (1, 0, 0) -> (1, 0, 1) -> (1, 1, 0) -> (0, 0, 1)
                            tetrahedra[tetIdx + 1] = new TetMesh4D.Tet4D(new int[]
                            {
                            vertices[4], vertices[5], vertices[6], vertices[1]
                            });

                            // (0, 0, 1) -> (1, 1, 1) -> (1, 0, 1) -> (1, 1, 0)
                            tetrahedra[tetIdx + 1] = new TetMesh4D.Tet4D(new int[]
                            {
                            vertices[1], vertices[7], vertices[5], vertices[6]
                            });

                            // (0, 0, 1) -> (0, 1, 1) -> (1, 1, 1) -> (1, 1, 0)
                            tetrahedra[tetIdx + 1] = new TetMesh4D.Tet4D(new int[]
                            {
                            vertices[1], vertices[3], vertices[7], vertices[6]
                            });


                            tetIdx += tetPerHex;
                        }
                    }
                }
            }

            tetMesh.UpdateMesh(meshVertices, tetrahedra);
        }

        private static HexMesh4D GenerateHexMesh(Manifold3D positionGenerator, Manifold3D normalGenerator, ParameterBounds samplingBounds)
        {
            int xSize = (int)(Mathf.Ceil((samplingBounds.hi.x - samplingBounds.lo.x) / samplingBounds.samplingInterval));
            int ySize = (int)(Mathf.Ceil((samplingBounds.hi.y - samplingBounds.lo.y) / samplingBounds.samplingInterval));
            int zSize = (int)(Mathf.Ceil((samplingBounds.hi.z - samplingBounds.lo.z) / samplingBounds.samplingInterval));
            HexMesh4D hexMesh = new HexMesh4D(xSize, ySize, zSize);

            // sample parametric equations positionGenerator, normalGenerator at intervals to generate a hexahedral (cube) Mesh
            for (int xSample = 0; xSample < xSize; ++xSample)
            {
                float u = samplingBounds.lo.x + xSample * samplingBounds.samplingInterval;
                for (int ySample = 0; ySample < ySize; ++ySample)
                {
                    float v = samplingBounds.lo.y + ySample * samplingBounds.samplingInterval;
                    for (int zSample = 0; zSample < zSize; ++zSample)
                    {
                        float t = samplingBounds.lo.z + zSample * samplingBounds.samplingInterval;

                        Vector3 sampledParams = new(u, v, t);
                        hexMesh.vertices[xSample, ySample, zSample] = positionGenerator(sampledParams);
                        hexMesh.normals[xSample, ySample, zSample] = normalGenerator(sampledParams);
                    }
                }
            }

            return hexMesh;
        }

        public struct ParameterBounds
        {
            public Vector3 lo;
            public Vector3 hi;
            public float samplingInterval;

            public ParameterBounds(Vector3 lo, Vector3 hi, float interval)
            {
                this.lo = lo;
                this.hi = hi;
                this.samplingInterval = interval;
            }
        }

        struct Dimension3D
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

        struct HexMesh4D
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
