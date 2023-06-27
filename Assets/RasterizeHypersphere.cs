using RasterizationRenderer;
using UnityEngine;

public class RasterizeHypersphere : MonoBehaviour
{
    TetMesh4D tetMesh;
    TriangleMesh triMesh;

    public float zSlice;
    public float vanishingW, nearW;
    public float samplingInterval;

    void Awake()
    {
        tetMesh = GetComponent<TetMesh4D>();
        triMesh = GetComponent<TriangleMesh>();
    }

    private void Start()
    {
        MeshGenerator4D.GenerateHypersphereMesh(tetMesh, samplingInterval);
    }

    // Update is called once per frame
    void Update()
    {
        //for (float zSlice = -1.0f; zSlice <= 1.0f; zSlice += 0.1f) { 
        ////for (float zSlice = -0.5f; zSlice <= -0.5f; zSlice += 0.1f)
            
        //}
        tetMesh.Render(vanishingW, nearW);
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
