Shader "Rasterize4D"
{
    Properties
    {
        _GlobalAmbientIntensity ("Ambient Intensity", Vector) = (0.2, 0.2, 0.2, 1.0)
        _GlobalDiffuseColour ("Diffuse Colour", Color) = (0.8, 0.8, 0.8, 1.0)
        _GlobalLightIntensity ("Light Intensity", Vector) = (1.0, 1.0, 1.0, 1.0)
        _GlobalSpecularColour ("Specular Colour", Color) = (1.0, 1.0, 1.0, 1.0)
        _GlobalShininess ("Shininess", Float) = 0.5
        _Opacity ("Opacity", Float) = 1.0
        _AttenuationFactor("Attenuation Factor", Float) = 0.5
        //_ShadowMap("Shadow Map", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert alpha
            #pragma fragment frag alpha:blend

            #include "UnityCG.cginc"
            #include "VertexShaderUtils.cginc"

            // Properties
            float4 _GlobalAmbientIntensity;
            fixed4 _GlobalDiffuseColour;
            float4 _GlobalLightIntensity;
            fixed4 _GlobalSpecularColour;
            float _GlobalShininess;
            float _Opacity;
            float _AttenuationFactor;

            float4x4 worldToCameraScaleAndRot;
            float4 worldToCameraTranslation;

            struct pointLight4D {
                Transform4D lightToWorldTransform;
                Transform4D worldToLightTransform;
            };

            StructuredBuffer<pointLight4D> lightSources;
            int numLights;

            sampler2D _ShadowMap;
            float4 _ShadowMap_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float4 vertexWorld: TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 normal : NORMAL;
                float4 vertexWorld : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;

                // we piggyback the w-coordinate into z to leverage hardware depth-testing
                o.vertex = UnityObjectToClipPos(v.vertex.xyw);

                o.normal = v.normal;
                o.vertexWorld = v.vertexWorld;
                return o;
            }

            fixed4 GetLightIntensity(float4 lightDirection, bool sqr)
            {
                float lightDistanceSqr = dot(lightDirection, lightDirection);
    
                return _GlobalLightIntensity * (1 / (1.0 + _AttenuationFactor * (sqr ? lightDistanceSqr : sqrt(lightDistanceSqr))));
            }


            float4 applyWorldToCameraTransform(float4 v) {
                return applyTranslation(applyScaleAndRot(v, worldToCameraScaleAndRot), worldToCameraTranslation);
            }

            float4 applyTransform(float4 v, Transform4D transform) {
                return applyTranslation(applyScaleAndRot(v, transform.scaleAndRot), transform.translation);
            }

            // Blinn-Phong Model from:
            // https://paroj.github.io/gltut/Illumination/Tut11%20BlinnPhong%20Model.html
            fixed4 frag (v2f i) : SV_Target
            {
                float4 lightSpaceVertex = UnityObjectToClipPos(
                    applyPerspectiveTransformation(
                        applyTransform(i.vertexWorld, lightSources[0].worldToLightTransform)
                    ).xyw
                );
                fixed4 sampledDepth = tex2D(_ShadowMap, lightSpaceVertex.xy); 
                //return fixed4(sampledDepth.x, 0, 0, 1);

                float4 vertex4D = applyWorldToCameraTransform(i.vertexWorld);
                float4 fragNormal = normalize(i.normal);

                fixed4 colour = _GlobalAmbientIntensity * _GlobalDiffuseColour;

                for (int idx = 0; idx < numLights; ++idx) {
                    float4 lightSource = applyWorldToCameraTransform(lightSources[idx].lightToWorldTransform.translation);

                    float4 lightVec = lightSource - vertex4D;
                    float4 lightDir = normalize(lightVec);
                    float cosAngIncidence = clamp(dot(fragNormal, lightDir), 0, 1);

                    float4 viewDir = normalize(-vertex4D);
                    float4 halfAngle = normalize(lightDir + viewDir);

                    float blinnTerm = clamp(dot(fragNormal, halfAngle), 0, 1.0);
                    blinnTerm = cosAngIncidence != 0.0 ? blinnTerm : 0.0;
                    blinnTerm = pow(blinnTerm, _GlobalShininess);

                    float lightIntensity = GetLightIntensity(lightVec, false);
                    float lightIntensitySqr = GetLightIntensity(lightVec, true);

                    float4 lightSpaceVertex = applyPerspectiveTransformation(
                        applyTransform(i.vertexWorld, lightSources[0].worldToLightTransform)
                    );
                    float3 lightClipSpaceVertex = UnityObjectToClipPos(lightSpaceVertex.xyw);
                    fixed sampledDepth = tex2D(_ShadowMap, lightClipSpaceVertex.xy).x;
                    float actualDepth = lightClipSpaceVertex.z;
                    //int shadowMultiplier = ((actualDepth <= (sampledDepth + 1e-3)) && (all(abs(lightSpaceVertex.xyz) < float3(1, 1, 1))));
                    //int shadowMultiplier = (all(abs(lightClipSpaceVertex.xy) < float2(1, 1)) && actualDepth >= 0);
                    int shadowMultiplier = (actualDepth <= (sampledDepth + 1e-3));
                    //int shadowMultiplier = 1;

                    colour += (_GlobalDiffuseColour * lightIntensity * cosAngIncidence) * shadowMultiplier;
                    colour += (_GlobalSpecularColour * lightIntensitySqr * blinnTerm) * shadowMultiplier;
                }

                return fixed4(colour.xyz, _Opacity);
            }
            ENDCG
        }
    }
}
