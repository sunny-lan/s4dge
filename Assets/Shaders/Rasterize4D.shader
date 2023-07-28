Shader "Rasterize4D"
{
    Properties
    {
        _GlobalAmbientIntensity ("Ambient Intensity", Vector) = (0.2, 0.2, 0.2, 1.0)
        _GlobalDiffuseColour ("Diffuse Colour", Color) = (0.8, 0.8, 0.8, 1.0)
        _GlobalLightIntensity ("Light Intensity", Vector) = (1.0, 1.0, 1.0, 1.0)
        _GlobalSpecularColour ("Specular Colour", Color) = (1.0, 1.0, 1.0, 1.0)
        _GlobalShininess ("Shininess", Float) = 0.5
        _AttenuationFactor("Attenuation Factor", Float) = 0.5
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

            // Properties
            float4 _GlobalAmbientIntensity;
            fixed4 _GlobalDiffuseColour;
            float4 _GlobalLightIntensity;
            fixed4 _GlobalSpecularColour;
            float _GlobalShininess;
            float _AttenuationFactor;

            struct pointLight4D {
                float4 pos;
            };

            StructuredBuffer<pointLight4D> lightSources;
            int numLights;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 normal : NORMAL;
                float4 vertexOriginal : POSITION1;
            };

            v2f vert (appdata v)
            {
                v2f o;

                // we piggyback the w-coordinate into z fto leverage hardware depth-testing
                o.vertex = UnityObjectToClipPos(v.vertex.xyw);

                o.normal = v.normal;
                o.vertexOriginal = v.vertex;
                return o;
            }

            fixed4 GetLightIntensity(float4 lightDirection)
            {
                float lightDistanceSqr = dot(lightDirection, lightDirection);
    
                return _GlobalLightIntensity * (1 / (1.0 + _AttenuationFactor * sqrt(lightDistanceSqr)));
            }

            // Phong Model from:
            // https://paroj.github.io/gltut/Illumination/Tut11%20Phong%20Model.html
            fixed4 frag (v2f i) : SV_Target
            {
                float4 vertex4D = i.vertexOriginal;
                float4 fragNormal = normalize(i.normal);

                fixed4 colour = _GlobalAmbientIntensity * _GlobalDiffuseColour;

                for (int i = 0; i < numLights; ++i) {
                    float4 lightSource = lightSources[i].pos;

                    float4 lightDir = lightSource - vertex4D;
                    float cosAngIncidence = clamp(dot(fragNormal, lightDir), 0, 1);

                    float4 viewDir = normalize(vertex4D);
                    float4 reflectDir = reflect(-lightDir, fragNormal);

                    float phongTerm = clamp(dot(viewDir, reflectDir), 0, 1);
                    phongTerm = cosAngIncidence != 0.0 ? phongTerm : 0.0;
                    phongTerm = pow(phongTerm, _GlobalShininess);

                    float lightIntensity = GetLightIntensity(lightDir);

                    colour += (_GlobalDiffuseColour * lightIntensity * cosAngIncidence) +
                        (_GlobalSpecularColour * lightIntensity * phongTerm);
                }

                    return fixed4(colour.xyz, 1.0);
                }
            ENDCG
        }
    }
}
