using RasterizationRenderer;
using UnityEngine;

namespace RasterizationRenderer
{
    public abstract class RasterizeObject: MonoBehaviour
    {
        protected TetMeshRenderer4D tetMeshRenderer;
        TriangleMesh triMesh;

        public float zSliceStart, zSliceLength, zSliceInterval;

        Camera4D camera4D;

        void Awake()
        {
            tetMeshRenderer = GetComponent<TetMeshRenderer4D>();
            triMesh = GetComponent<TriangleMesh>();
        }

        private void Start()
        {
            camera4D = Camera4D.main;
            InitGeometry();
            tetMeshRenderer.MeshInit();
        }

        protected abstract void InitGeometry();

        // Update is called once per frame
        public void DrawFrame()
        {
            tetMeshRenderer.Render(zSliceStart, zSliceLength, zSliceInterval,
                camera4D.WorldToCameraTransform, camera4D.camera3D.farClipPlane, camera4D.camera3D.nearClipPlane);
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
}
