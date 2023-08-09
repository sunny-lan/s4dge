Shader "ShadowMapGenerator4D"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "VertexShaderUtils.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float4 vertexWorld: TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float depth: DEPTH1;
            };

            v2f vert (appdata v)
            {
                v2f o;

                // we piggyback the w-coordinate into z to leverage hardware depth-testing
                o.vertex = UnityObjectToClipPos(v.vertex.xyw);
                o.depth = o.vertex.z / o.vertex.w;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(i.depth, 0, 0, 1);
            }
            ENDCG
        }
    }
}
