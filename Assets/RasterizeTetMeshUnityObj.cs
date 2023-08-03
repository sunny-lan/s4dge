using RasterizationRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RasterizationRenderer;

public class RasterizeTetMeshUnityObj : MonoBehaviour
{
    TetMeshRenderer4D tetMeshRenderer;
    TriangleMesh triMesh;

    public float zSliceStart, zSliceLength, zSliceInterval;
    public TetMesh_UnityObj rawMesh;

    void Awake()
    {
        tetMeshRenderer = GetComponent<TetMeshRenderer4D>();
        triMesh = GetComponent<TriangleMesh>();
    }

    private void Start()
    {
        tetMeshRenderer.SetTetMesh(rawMesh.mesh_Raw.ToRasterizableTetMesh());
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
