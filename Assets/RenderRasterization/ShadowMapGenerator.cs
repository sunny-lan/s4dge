using RasterizationRenderer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RasterizationRenderer
{
    [RequireComponent(typeof(TetMeshRenderer4D))]
    public class ShadowMapGenerator : MonoBehaviour
    {
        TetMeshRenderer4D tetMeshRenderer;

        Texture _shadowMap;
        public Texture ShadowMap
        {
            get => _shadowMap; internal set { _shadowMap = value; }
        }

        public float zSliceStart, zSliceLength, zSliceInterval;

        // Updates tet mesh with tets for every object in the scene
        public void UpdateTetMesh()
        {
            var tetMeshRenderers = FindObjectsByType<TetMeshRenderer4D>(FindObjectsSortMode.InstanceID);
            foreach (var tetMeshRenderer in tetMeshRenderers)
            {
                if (tetMeshRenderer != this.tetMeshRenderer)
                {
                    this.tetMeshRenderer.AppendTetMesh(tetMeshRenderer.tetMesh);
                }
            }
        }

        public void UpdateShadowMap()
        {
            RenderTexture rt = new(Screen.width, Screen.height, 24);
            tetMeshRenderer.Render(zSliceStart, zSliceLength, zSliceInterval, rt);
            _shadowMap = rt;
        }

        // Start is called before the first frame update
        void Start()
        {
            tetMeshRenderer = GetComponent<TetMeshRenderer4D>();
        }

        // Update is called once per frame
        void Update()
        {
            if (tetMeshRenderer.tetMesh == null)
            {
                UpdateTetMesh();
            }

            UpdateShadowMap();
        }
    }
}
