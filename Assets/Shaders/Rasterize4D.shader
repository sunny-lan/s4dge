// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Rasterize4D"
{
    Properties
    {
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

            float4x4 projectionMatrix;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 normal : NORMAL;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(v.normal);
                //o.vertex = mul(UNITY_MATRIX_VP, float4(v.vertex.xyz, 1.0));
                //o.vertex = mul(projectionMatrix, float4(v.vertex.xyz, 1.0));
                //o.vertex = float4(v.vertex.xyz, 1.0);
                //o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(0, 1, 0, 0.5);
                return col;
            }
            ENDCG
        }
    }
}
