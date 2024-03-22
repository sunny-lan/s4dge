using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using S4DGE;

namespace RasterizationRenderer
{
    public class ShadowMapGenerator : MonoBehaviour
    {
        public Material shadowGenerateMaterial;

        List<TetMeshRenderer4D> sceneTetMeshRenderers;
        bool shadowsEnabled;

        RenderTexture newShadowMap;
        RenderTexture _shadowMap;
        public RenderTexture ShadowMap
        {
            get => _shadowMap;
        }

        public float zSliceStart, zSliceLength, zSliceInterval;

        // Updates tet mesh with tets for every object in the scene
        void UpdateTetMeshList()
        {
            sceneTetMeshRenderers = FindObjectsByType<TetMeshRenderer4D>(FindObjectsSortMode.InstanceID).ToList();
        }

        public RenderTexture GetClearedShadowMap(Color colour)
        {
            RenderTexture shadowMap = new(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            RenderTexture prevRT = RenderTexture.active;
            RenderTexture.active = shadowMap;
            GL.Clear(true, true, colour);
            RenderTexture.active = prevRT;

            return shadowMap;
        }

        void UpdateVisibleShadowMap(bool enableShadows)
        {
            // Fill the shadow map with min depth (farthest from camera) if shadows are disabled
            if (_shadowMap != null)
            {
                _shadowMap.Release();
            }
            _shadowMap = enableShadows ? newShadowMap : GetClearedShadowMap(Color.clear);
        }

        public void UpdateShadowMap(TransformMatrixAffine4D worldToLightTransform, bool enableShadows)
        {
            if (!enableShadows)
            {
                if (shadowsEnabled)
                {
                    UpdateVisibleShadowMap(enableShadows);
                    shadowsEnabled = false;
                }
            }
            else
            {
                if (sceneTetMeshRenderers == null)
                {
                    UpdateTetMeshList();
                }

                Camera cam3D = Camera4D.main.camera3D;
                newShadowMap = GetClearedShadowMap(Color.clear);
                foreach (var tetMeshRenderer in sceneTetMeshRenderers)
                {
                    tetMeshRenderer.Render(zSliceStart, zSliceLength, zSliceInterval,
                        worldToCameraTransform: worldToLightTransform,
                        farClipPlane: cam3D.farClipPlane,
                        nearClipPlane: cam3D.nearClipPlane,
                        overrideMaterial: shadowGenerateMaterial,
                        outputTexture: newShadowMap,
                        clear: true
                    );
                }

                shadowsEnabled = true;

                UpdateVisibleShadowMap(enableShadows);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            shadowsEnabled = true;
            _shadowMap = null;
            newShadowMap = null;
        }
    }
}
