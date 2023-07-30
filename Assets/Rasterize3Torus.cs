using RasterizationRenderer;
using UnityEngine;

public class Rasterize3Torus : MonoBehaviour
{
    TetMeshRenderer4D tetMeshRenderer;
    TriangleMesh triMesh;

    public float thickness;
    public float zSliceStart, zSliceLength, zSliceInterval;
    public float samplingInterval;

    void Awake()
    {
        tetMeshRenderer = GetComponent<TetMeshRenderer4D>();
        triMesh = GetComponent<TriangleMesh>();
    }

    private void Start()
    {
        tetMeshRenderer.SetTetMesh(MeshGenerator4D.Generate3TorusMesh(samplingInterval, thickness));
    }

    // Update is called once per frame
    void Update()
    {
        tetMeshRenderer.Render(zSliceStart, zSliceLength, zSliceInterval);
    }

    private void OnEnable()
    {
        tetMeshRenderer.gameObject.SetActive(true);
        triMesh.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        tetMeshRenderer.gameObject.SetActive(false);
        triMesh.gameObject.SetActive(false);
    }
}
