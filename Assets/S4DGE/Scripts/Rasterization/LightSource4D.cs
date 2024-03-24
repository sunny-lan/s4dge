using UnityEngine;
using S4DGE;

namespace RasterizationRenderer
{
    [RequireComponent(typeof(ShadowMapGenerator))]
    public class LightSource4D : MonoBehaviour
    {
        Transform4D transform4D;
        ShadowMapGenerator shadowMapGenerator;

        public TransformMatrixAffine4D LightToWorldTransform { get => transform4D.localToWorldMatrix; }

        public TransformMatrixAffine4D WorldToLightTransform { get => transform4D.worldToLocalMatrix; }

        public RenderTexture ShadowMap { get => shadowMapGenerator.ShadowMap; }

        public ShaderData Data { get => new(LightToWorldTransform, WorldToLightTransform); }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct ShaderData
        {
            Matrix4x4 LightToWorldScaleAndRot;
            Vector4 LightToWorldTranslation;
            Matrix4x4 WorldToLightScaleAndRot;
            Vector4 WorldToLightTranslation;

            public ShaderData(TransformMatrixAffine4D LightToWorldTransform, TransformMatrixAffine4D WorldToLightTransform)
            {
                LightToWorldScaleAndRot = LightToWorldTransform.scaleAndRot;
                LightToWorldTranslation = LightToWorldTransform.translation;
                WorldToLightScaleAndRot = WorldToLightTransform.scaleAndRot;
                WorldToLightTranslation = WorldToLightTransform.translation;
            }

            public static int SizeFloats { get => 2 * 20; }

            public static int SizeBytes { get => SizeFloats * sizeof(float); }
        }

        public override string ToString()
        {
            return "(pos: " + LightToWorldTransform.translation.ToString() + ")";
        }

        private void Awake()
        {
            transform4D = GetComponent<Transform4D>();
            shadowMapGenerator = GetComponent<ShadowMapGenerator>();
        }

        public void UpdateShadowMap(bool enableShadows)
        {
            shadowMapGenerator.UpdateShadowMap(WorldToLightTransform, enableShadows);
        }
    }
}
