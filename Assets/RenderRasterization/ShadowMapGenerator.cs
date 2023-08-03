using RasterizationRenderer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using v2;

namespace RasterizationRenderer
{
    [RequireComponent(typeof(TriangleMesh))]
    public class ShadowMapGenerator : MonoBehaviour
    {
        public TriangleMesh triangleMesh;
        RenderTexture _shadowMap;
        List<TetMeshRenderer4D> sceneTetMeshRenderers;
        bool shadowsEnabled;
        public Texture ShadowMap
        {
            get => _shadowMap;
        }

        public float zSliceStart, zSliceLength, zSliceInterval;

        // Updates tet mesh with tets for every object in the scene
        void UpdateTetMeshList()
        {
            sceneTetMeshRenderers = FindObjectsByType<TetMeshRenderer4D>(FindObjectsSortMode.InstanceID).ToList();
        }

        public void ClearShadowMap(Color colour)
        {
            RenderTexture rt = new(Screen.width, Screen.height, 0);
            RenderTexture prevRT = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, colour);
            RenderTexture.active = prevRT;
            _shadowMap = rt;
        }

        public void UpdateShadowMap(TransformMatrixAffine4D worldToLightTransform, bool enableShadows)
        {
            if (!enableShadows)
            {
                if (shadowsEnabled)
                {
                    // Fill the shadow map with max depth if shadows are disabled
                    ClearShadowMap(Color.red);
                    shadowsEnabled = false;
                }
            }
            else
            {
                _shadowMap?.Release();

                if (sceneTetMeshRenderers == null)
                {
                    UpdateTetMeshList();
                }

                triangleMesh.Reset();
                Camera cam3D = Camera4D.main.camera3D;
                foreach (var tetMeshRenderer in sceneTetMeshRenderers)
                {
                    tetMeshRenderer.RenderToTriangleMesh(zSliceStart, zSliceLength, zSliceInterval,
                        worldToCameraTransform: worldToLightTransform,
                        farClipPlane: cam3D.farClipPlane,
                        nearClipPlane: cam3D.nearClipPlane,
                        overrideOutputMesh: triangleMesh,
                        renderTriangleMeshToScreen: false,
                        clear: false
                    );
                }

                RenderTexture shadowMap = new(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                triangleMesh.RenderToRenderTexture(shadowMap);
                _shadowMap = shadowMap;

                shadowsEnabled = true;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            triangleMesh = GetComponent<TriangleMesh>();
            shadowsEnabled = true;
        }
    }
}
