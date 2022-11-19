using System.Collections;
using System.Linq;
using UnityEngine;

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
            Debug.DrawLine(a, b);
        }
    }
    public void SetGeometry(Geometry3D geometry)
    {
        this.geometry = geometry;
        mesh.vertices = geometry.vertices.Select(x => x.position).ToArray();
        mesh.uv = geometry.vertices.Select(x => x.uv).ToArray();
        // use uv8.x to store w
        mesh.uv8 = geometry.vertices.Select(x => new Vector2(x.w, 0)).ToArray();

        mesh.triangles = geometry.triangles.ToArray();
    }
}
