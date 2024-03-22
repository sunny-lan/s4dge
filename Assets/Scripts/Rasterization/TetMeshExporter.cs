using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using S4DGE;

namespace RasterizationRenderer
{
    public class TetMeshExporter : MonoBehaviour
    {
        public bool generateButton = false;

        protected TetMeshRenderer4D tetMeshRenderer;
        
        void Awake()
        {
            tetMeshRenderer = GetComponent<TetMeshRenderer4D>();
        }

        void OnValidate()
        {
            if (generateButton)
            {
                TetMesh4D tetMesh = tetMeshRenderer.tetMesh;
                TetMesh_UnityObj mesh = ScriptableObject.CreateInstance<TetMesh_UnityObj>();
                
                mesh.mesh_Raw = new()
                {
                    tets = tetMesh.tets.ToList(),
                    vertices = tetMesh.vertices.ToList(),
                };

                UnityEditor.AssetDatabase.CreateAsset(mesh, $"Assets/Tets/Exports/export.asset");

                generateButton = false;
            }
        }

    }
}