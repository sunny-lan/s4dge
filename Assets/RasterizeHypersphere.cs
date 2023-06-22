using RasterizationRenderer;
using UnityEngine;

public class RasterizeHypersphere : MonoBehaviour
{
    TetMesh4D tetMesh;
    TriangleMesh triMesh;

    public float zSlice, vanishingW, nearW;

    void Awake()
    {
        tetMesh = GetComponent<TetMesh4D>();
        triMesh = GetComponent<TriangleMesh>();
    }

    private void Start()
    {
        MeshGenerator4D.GenerateHypersphereMesh(tetMesh, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        tetMesh.Render(zSlice, vanishingW, nearW);
    }

    private void OnEnable()
    {
        tetMesh.gameObject.SetActive(true);
        triMesh.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        tetMesh.gameObject.SetActive(false);
        triMesh.gameObject.SetActive(false);
    }
}
