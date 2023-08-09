using RasterizationRenderer;
using UnityEngine;

public class Rasterize3Helix : MonoBehaviour
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
        var line = new ParametricShape1D()
        {
            Divisions = 1/samplingInterval,
            End = 2 * Mathf.PI,
            Start = 0,
            Path = s =>
            {
                return new(
                    2 * Mathf.Sin(s),
                    2 * Mathf.Cos(s),
                    s, 0
                );
            }
        };

        var converted = ManifoldConverter.HyperCylinderify(line, s => thickness);

        var mesh = MeshGenerator4D.GenerateTetMesh(converted.Position, converted.Normal, converted.Bounds);

        tetMeshRenderer.SetTetMesh(mesh);
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
