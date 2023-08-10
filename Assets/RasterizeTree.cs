using RasterizationRenderer;
using TreeGen;
using UnityEngine;
using v2;

public class RasterizeTree : MonoBehaviour
{
    TetMeshRenderer4D tetMeshRenderer;
    TriangleMesh triMesh;
    private Transform4D t4d;
    public float thickness;
    public float zSliceStart, zSliceLength, zSliceInterval;
    public float samplingInterval;

    void Awake()
    {
        tetMeshRenderer = GetComponent<TetMeshRenderer4D>();
        triMesh = GetComponent<TriangleMesh>();
        t4d = GetComponent<Transform4D>();
    }

    public TreeGenParameters parameters;
    public TetMesh_raw mesh;

    private void Render()
    {
            
        mesh = new TetMesh_raw();
        TreeBranch root = new TreeBranch()
        {
            BaseTransform = new()
            {
                //scaleAndRot=new Matrix4x4(
                //    new(1,0,0,0),
                //    new(0,0,1,0),
                //    new(0,1,0,0),
                //    new(0,0,0,1)
                //),
                scaleAndRot = Matrix4x4.identity,
                //scaleAndRot = new Matrix4x4(
                //    new(1, 0, 0, 0),
                //    new(0, 0, 0, 1),
                //    new(0, 1, 0, 0),
                //    new(0, 0, 1, 0)
                //),
                //scaleAndRot = new Matrix4x4(
                //    new(1, 0, 0, 0),
                //    new(0, 1, 0, 0),
                //    new(0, 0, 0, 1),
                //    new(0, 0, 1, 0)
                //),
                translation = Vector4.zero,
            },
            Depth = 0,
        };
        TreeGenerator.GrowSingleBranch(root, parameters, new(1));
        TreeGenerator.Render(root, mesh);
        tetMeshRenderer.SetTetMesh(mesh.ToRasterizableTetMesh());
    }

    // Update is called once per frame
    float timeSinceLast = 0;
    void Update()
    {
        timeSinceLast += Time.deltaTime;
        if (timeSinceLast > 1)
        {
            Render();
            timeSinceLast = 0;
        }
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
