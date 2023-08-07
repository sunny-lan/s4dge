using RasterizationRenderer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using v2;

public class PolytopeGenerator : MonoBehaviour
{
    public enum Polytope {
        cell5,
        cell8,
        cell8odd,
        cell8even,
        cell16,
        cell24,
        cell120,
        cell600
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
                    
                    int tetCount = 0;
                    for (int i = 0; i < 5; i++) // 5 choose 4 (5) tets
                    {
                        for (int j = i + 1; j < 5; j++)
                        {
                            for (int k = j + 1; k < 5; k++)
                            {
                                for (int l = k + 1; l < 5; l++)
                                {
                                    Vector4 normal = Util.CrossProduct4D(pos[j] - pos[i], pos[k] - pos[i], pos[l] - pos[i]);
                                    normal = Vector4.zero;
                                    v.Add(new TetMesh4D.VertexData(pos[i], normal));
                                    v.Add(new TetMesh4D.VertexData(pos[j], normal));
                                    v.Add(new TetMesh4D.VertexData(pos[k], normal));
                                    v.Add(new TetMesh4D.VertexData(pos[l], normal));

                                    int[] vertices = {tetCount, tetCount+1, tetCount+2, tetCount+3};
                                    tetCount += 4;
                                    t.Add(new TetMesh4D.Tet4D(vertices));
                                }
                            }
                        }
                    }
                    break;
                }
                case Polytope.cell8:
                {
                    TetMesh_raw mesh = new();
                    TetMesh4D.VertexData[] cell16even = { // Vertices have to be ordered such that the corresponding negative coordinate is at index +4
                        new TetMesh4D.VertexData(new Vector4(1,1,1,1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(-1,-1,1,1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(-1,1,-1,1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(-1,1,1,-1), Vector4.zero),
                        
                        new TetMesh4D.VertexData(new Vector4(-1,-1,-1,-1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(1,1,-1,-1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(1,-1,1,-1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(1,-1,-1,1), Vector4.zero),
                    };
                    mesh.vertices.AddRange(cell16even);
                    Set16CellTets(mesh, 0);
                    
                    TetMesh4D.VertexData[] cell16odd = {
                        new TetMesh4D.VertexData(new Vector4(-1,1,1,1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(1,-1,1,1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(1,1,-1,1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(1,1,1,-1), Vector4.zero),

                        new TetMesh4D.VertexData(new Vector4(1,-1,-1,-1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(-1,1,-1,-1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(-1,-1,1,-1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(-1,-1,-1,1), Vector4.zero),
                    };
                    mesh.vertices.AddRange(cell16odd);
                    Set16CellTets(mesh, 8);

                    v = mesh.vertices.ToList();
                    t = mesh.tets.ToList();
                    break;
                }
                case Polytope.cell8odd:
                {
                    TetMesh_raw mesh = new();
                    TetMesh4D.VertexData[] cell16odd = {
                        new TetMesh4D.VertexData(new Vector4(-1,1,1,1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(1,-1,1,1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(1,1,-1,1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(1,1,1,-1), Vector4.zero),

                        new TetMesh4D.VertexData(new Vector4(1,-1,-1,-1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(-1,1,-1,-1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(-1,-1,1,-1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(-1,-1,-1,1), Vector4.zero),
                    };
                    mesh.vertices.AddRange(cell16odd);
                    Set16CellTets(mesh, 0);

                    v = mesh.vertices.ToList();
                    t = mesh.tets.ToList();
                    break;
                }
                case Polytope.cell8even:
                {
                    TetMesh_raw mesh = new();
                    TetMesh4D.VertexData[] cell16even = { // Vertices have to be ordered such that the corresponding negative coordinate is at index +4
                        new TetMesh4D.VertexData(new Vector4(1,1,1,1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(-1,-1,1,1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(-1,1,-1,1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(-1,1,1,-1), Vector4.zero),
                        
                        new TetMesh4D.VertexData(new Vector4(-1,-1,-1,-1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(1,1,-1,-1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(1,-1,1,-1), Vector4.zero),
                        new TetMesh4D.VertexData(new Vector4(1,-1,-1,1), Vector4.zero),
                    };
                    mesh.vertices.AddRange(cell16even);
                    Set16CellTets(mesh, 0);

                    v = mesh.vertices.ToList();
                    t = mesh.tets.ToList();
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
                    TetMesh4D centerCube = HypercubeGenerator.GenerateHypercube(); // center tesseract at radius 0.5, centered at (0.5,0.5,0.5,0.5)
                    v = centerCube.vertices.ToList();
                    t = centerCube.tets.ToList();

                    TetMesh_raw outer16 = Generate16Cell(1, 0.5f, v.Count); // outer 16 cell centered at (0.5,0.5,0.5,0.5), radius 1
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

    TetMesh_raw Generate16Cell(float radius, float offset, int startIndex)
    {
        TetMesh_raw mesh = new();

        float[] vals = {radius + offset, -radius + offset};
        foreach (float i in vals)
        {
            for (int j = 0; j < 4; j++)
            {
                Vector4 pos = Vector4.zero;
                pos[j] = i;
                TetMesh4D.VertexData vertex = new TetMesh4D.VertexData(pos, Vector4.zero);
                mesh.vertices.Add(vertex);
            }
        }

        Set16CellTets(mesh, startIndex);

        return mesh;
    }

    TetMesh_raw Set16CellTets(TetMesh_raw mesh, int startIndex)
    {
        int numVertices = 8;
        int half = numVertices / 2;
        int[] option = {0, half};

        foreach (int one in option)
        {
            foreach (int two in option)
            {
                foreach (int three in option)
                {
                    foreach (int four in option)
                    {
                        int[] tet = {one + startIndex, two + 1 + startIndex, three + 2 + startIndex, four + 3 + startIndex};
                        mesh.tets.Add(new TetMesh4D.Tet4D(tet));
                    }
                }
            }
        }

        return mesh;
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
