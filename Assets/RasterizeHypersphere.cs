using RasterizationRenderer;
using UnityEngine;

public class RasterizeHypersphere : MonoBehaviour
{
    TetMesh4D tetMesh;
    TriangleMesh triMesh;

    // Start is called before the first frame update
    void Start()
    {
        tetMesh = GetComponent<TetMesh4D>();
        triMesh = GetComponent<TriangleMesh>();
        MeshGenerator4D.GenerateHypersphereMesh(tetMesh, 0.01f);
    }

    // Update is called once per frame
    void Update()
    {
        tetMesh.Render();
    }
}
