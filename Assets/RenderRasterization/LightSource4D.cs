using UnityEngine;
using UnityEngine.UIElements;
using v2;

namespace RasterizationRenderer
{
    [RequireComponent(typeof(ShadowMapGenerator))]
    public class LightSource4D : MonoBehaviour
    {
        Transform4D transform4D;
        ShadowMapGenerator shadowMapGenerator;

        public TransformMatrixAffine4D LightToWorldTransform { get => transform4D.localToWorldMatrix; }

        public TransformMatrixAffine4D WorldToLightTransform { get => transform4D.worldToLocalMatrix; }

        public override string ToString()
        {
            return "(pos: " + LightToWorldTransform.translation.ToString() + ")";
        }

        private void Awake()
        {
            transform4D = GetComponent<Transform4D>();
            shadowMapGenerator = GetComponent<ShadowMapGenerator>();
        }
    }
}
