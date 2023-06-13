Shader "Rasterize4D"
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "VertexShader.cginc"

            VertexData vert (VertexData v)
            {
                VertexData transformed4D = applyTranslation(applyRotation(v, modelViewRotation4D), modelViewTranslation4D);
                transformed4D.pos = applyPerspectiveTransformation(transformed4D.pos);

                return transformed4D;
            }

            fixed4 frag (VertexData i) : SV_Target
            {
                return fixed4(0.0, 0.0, 0.0, 0.0);
            }
            ENDCG
        }
    }
}
