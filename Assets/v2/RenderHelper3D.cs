using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace v2
{
    /// <summary>
    /// Helper component for mesh rendering
    /// </summary>
    public class RenderHelper3D : MonoBehaviour
    {
        Mesh mesh;
        Geometry3D geometry;
        public MeshRenderer meshRenderer { get; private set; }
        private void Awake()
        {
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            var mf = gameObject.GetComponent<MeshFilter>();
            mesh = new() { vertices = new Vector3[] { }, triangles = new int[0] };
            mf.mesh = mesh;

        }


        private void Update()
        {
            if (geometry == null) return;
            foreach (var (a, b) in geometry.lines)
            {
                Debug.DrawLine(geometry.vertices[ a],geometry.vertices[ b]);
            }
        }
        public void SetGeometry(Geometry3D geometry)
        {
            this.geometry = geometry;

            // if no vertices, don't show anything
            if (geometry.vertices.Count == 0)
            {
                meshRenderer.enabled = false;
                return;
            }
            else
            {
                meshRenderer.enabled = true;
            }

            geometry.ApplyToMesh(mesh);
        }
    }
}