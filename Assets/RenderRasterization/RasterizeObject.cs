using UnityEngine;
using UnityEngine.Assertions;
using v2;

namespace RasterizationRenderer
{
    public abstract class RasterizeObject : MonoBehaviour
    {
        protected TetMeshRenderer4D tetMeshRenderer;
        TriangleMesh triMesh;

        public float zSliceStart, zSliceLength, zSliceInterval;

        void Awake()
        {
            tetMeshRenderer = GetComponent<TetMeshRenderer4D>();
            triMesh = GetComponent<TriangleMesh>();
            Assert.IsNotNull(tetMeshRenderer);
        }

        private void Start()
        {
            InitGeometry();
            tetMeshRenderer.MeshInit();
        }

        protected abstract void InitGeometry();

        // Update is called once per frame
        public void DrawFrame()
        {
            var pos4D = Camera4D.main.t4d.position;
            tetMeshRenderer.Render(zSliceStart, zSliceLength, zSliceInterval,
                Camera4D.main.WorldToCameraTransform, Camera4D.main.camera3D.farClipPlane, Camera4D.main.camera3D.nearClipPlane);
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
