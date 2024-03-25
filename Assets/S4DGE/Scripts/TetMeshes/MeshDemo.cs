using RasterizationRenderer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RaytraceRenderer;

namespace S4DGE
{
    public class MeshDemo : MonoBehaviour
    {
        TetMeshRenderer meshRenderer;

        // Start is called before the first frame update
        void Start()
        {
            meshRenderer = GetComponent<TetMeshRenderer>();

            var newMesh = MeshGenerator4D.GenerateTetMesh(
                p => new(
                    Mathf.Sin(p.x) - p.z,
                    Mathf.Cos(p.y),
                    Mathf.Sin(p.z) + p.x,
                    Mathf.Cos(p.x)* Mathf.Sin(p.y)
                ), _ => new(), new(
                    lo:new(-1,-1,-1),
                    hi:new(1,1,1),
                    interval:0.3f
                ));

            meshRenderer.mesh = ScriptableObject.CreateInstance<TetMesh_UnityObj>();
            meshRenderer.mesh.mesh_Raw = new()
            {
                tets = newMesh.tets.ToList(),
                vertices = newMesh.vertices.ToList(),
            };
        }
    }
}