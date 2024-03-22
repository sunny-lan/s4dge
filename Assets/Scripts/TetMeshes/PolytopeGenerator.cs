using RasterizationRenderer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using v2;

public class PolytopeGenerator : MonoBehaviour // Produces asset files for regular convex 4-polytopes. By accident, some are stellated
{
    public enum Polytope {
        cell5, // regular, has normals
        cell8, // regular, no normals
        cell8s, // stellated, has normals
        cell16, // regular, has normals
        cell24, // regular, has normals (looks wrong . . .)
        cell120, // not implemented
        cell600 // not implemented
    }

    public Polytope type = Polytope.cell5;
    public bool generateButton = false;

    void OnValidate()
    {
        if (generateButton)
        {
            List<TetMesh4D.VertexData> v = new();
            List<TetMesh4D.Tet4D> t = new();

            switch (type)
            {
                case Polytope.cell5:
                {
                    // Setup array of vertex positions
                    Vector4[] pos = new Vector4[5];
                    float goldenRatio = 1.618033988749894f;
                    for (int i = 0; i < 4; i++)
                    {
                        pos[i] = Vector4.zero;
                        pos[i][i] = 2;
                    }
                    pos[4] = new Vector4(goldenRatio, goldenRatio, goldenRatio, goldenRatio);
                    
                    for (int i = 0; i < 5; i++) // 5 choose 4 (5) tets
                    {
                        for (int j = i + 1; j < 5; j++)
                        {
                            for (int k = j + 1; k < 5; k++)
                            {
                                for (int l = k + 1; l < 5; l++)
                                {
                                    AddTetWithNormals(pos, new int[] {i, j, k, l}, v, t);
                                }
                            }
                        }
                    }
                    break;
                }
                case Polytope.cell8:
                {
                    TetMesh_raw mesh = new();
                    HypercubeGenerator.GenerateHypercubeTmp(mesh);
                    v = mesh.vertices.ToList();
                    t = mesh.tets.ToList();
                    break;
                }
                case Polytope.cell8s:
                {
                    TetMesh_raw cell8 = GenerateStellated8Cell(); // convex hull of the 8-cell
                    v = cell8.vertices.ToList();
                    t = cell8.tets.ToList();
                    break;
                }
                case Polytope.cell16:
                {
                    TetMesh_raw raw = Generate16Cell(1, 0, 0);
                    v = raw.vertices;
                    t = raw.tets;
                    break;
                }
                case Polytope.cell24:
                {
                    TetMesh_raw centerCube = GenerateStellated8Cell(); // center tesseract at radius 1 centered 0,0,0,0
                    v = centerCube.vertices.ToList();
                    t = centerCube.tets.ToList();

                    TetMesh_raw outer16 = Generate16Cell(2, 0, v.Count); // outer 16 cell radius 2 centered 0,0,0,0
                    v.AddRange(outer16.vertices);
                    t.AddRange(outer16.tets);
                    break;
                }
                default:
                {
                    Debug.LogError($"Tetmesh Generation of Polytope: {type} Not Implemented.");
                    break;
                }
            }
            
            CreateScriptableObject(v, t);

            generateButton = false;
        }
    }

    TetMesh_raw GenerateStellated8Cell()
    {
        TetMesh_raw mesh = new();

        Vector4[] cell16even = { // Vertices have to be ordered such that the corresponding negative coordinate is at index +4
            new Vector4(1,1,1,1),
            new Vector4(-1,-1,1,1),
            new Vector4(-1,1,-1,1),
            new Vector4(-1,1,1,-1),
            
            new Vector4(-1,-1,-1,-1),
            new Vector4(1,1,-1,-1),
            new Vector4(1,-1,1,-1),
            new Vector4(1,-1,-1,1)
        };
        Add16CellTets(mesh, cell16even);

        Vector4[] cell16odd = {
            new Vector4(-1,1,1,1),
            new Vector4(1,-1,1,1),
            new Vector4(1,1,-1,1),
            new Vector4(1,1,1,-1),

            new Vector4(1,-1,-1,-1),
            new Vector4(-1,1,-1,-1),
            new Vector4(-1,-1,1,-1),
            new Vector4(-1,-1,-1,1)
        };
        Add16CellTets(mesh, cell16odd);

        return mesh;
    }

    void AddTetWithNormals(Vector4[] pos, int[] tet, List<TetMesh4D.VertexData> v, List<TetMesh4D.Tet4D> t)
    {
        Vector4 normal = Util.CrossProduct4D(pos[tet[1]] - pos[tet[0]], pos[tet[2]] - pos[tet[0]], pos[tet[3]] - pos[tet[0]]);
        normal.Normalize();
        int vIndex = v.Count;
        v.Add(new TetMesh4D.VertexData(pos[tet[0]], normal));
        v.Add(new TetMesh4D.VertexData(pos[tet[1]], normal));
        v.Add(new TetMesh4D.VertexData(pos[tet[2]], normal));
        v.Add(new TetMesh4D.VertexData(pos[tet[3]], normal));

        int[] vertices = {vIndex, vIndex+1, vIndex+2, vIndex+3};
        t.Add(new TetMesh4D.Tet4D(vertices));
    }

    TetMesh_raw Generate16Cell(float radius, float offset, int startIndex)
    {
        TetMesh_raw mesh = new();
        Vector4[] pos = new Vector4[8]; // Array for 8 positions options for the vertices

        int index = 0;
        float[] vals = {radius, -radius};
        foreach (float i in vals)
        {
            for (int j = 0; j < 4; j++)
            {
                Vector4 vertexPos = new Vector4(offset, offset, offset, offset);
                vertexPos[j] += i;
                pos[index++] = vertexPos; // Accumulate into the positions array
            }
        }

        Add16CellTets(mesh, pos);

        return mesh;
    }

    // Adds tets correspoding to a 16 cell to the given mesh, using the vertex positions provided in pos
    void Add16CellTets(TetMesh_raw mesh, Vector4[] pos)
    {
        int startIndex = mesh.vertices.Count;
        int numVertices = 8;
        int half = numVertices / 2;
        int[] option = {0, half};

        foreach (int x in option)
        {
            foreach (int y in option)
            {
                foreach (int z in option)
                {
                    foreach (int w in option)
                    {
                        int[] tet = {x, y + 1, z + 2, w + 3};
                        AddTetWithNormals(pos, tet, mesh.vertices, mesh.tets);
                    }
                }
            }
        }
    }

    void CreateScriptableObject(List<TetMesh4D.VertexData> v, List<TetMesh4D.Tet4D> t)
    {
        TetMesh4D tetMesh = new TetMesh4D(v.ToArray(), t.ToArray()); // Use tetmesh4D constructor to ensure positive tet volumes
        TetMesh_UnityObj mesh = ScriptableObject.CreateInstance<TetMesh_UnityObj>();
        mesh.mesh_Raw = new()
        {
            tets = tetMesh.tets.ToList(),
            vertices = tetMesh.vertices.ToList(),
        };
        UnityEditor.AssetDatabase.CreateAsset(mesh, $"Assets/Tets/Polytopes/{type}.asset");
        Debug.Log($"Created polytope: Assets/Tets/Polytopes/{type}.asset");
    }
}
