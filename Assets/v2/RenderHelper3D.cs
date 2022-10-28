using System.Collections;
using UnityEngine;

/// <summary>
/// Helper component for mesh rendering
/// </summary>
[ExecuteAlways]
public class RenderHelper3D : MonoBehaviour
{
    Mesh mesh;
    Geometry3D geometry;
    private void Awake()
    {
        var renderer = gameObject.GetComponent<MeshRenderer>();
        var mf = gameObject.GetComponent<MeshFilter>();
        mesh = new() { vertices = new Vector3[] { }, triangles = new int[0] };
        mf.mesh = mesh;
    }


    private void Update()
    {
        if (geometry == null) return;
        foreach(var (a,b) in geometry.lines)
        {
            Debug.DrawLine(a, b);
        }
    }
    public void SetGeometry(Geometry3D geometry)
    {
        this.geometry = geometry;
        mesh.vertices = geometry.vertices.ToArray();
        mesh.triangles = geometry.triangles.ToArray();
    }
}
