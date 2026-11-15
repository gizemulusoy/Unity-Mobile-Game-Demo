Shader "Hidden/WinBackdropBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurDirection ("Blur Direction", Vector) = (1, 0, 0, 0)
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _BlurDirection;

            fixed4 frag(v2f_img input) : SV_Target
            {
                float2 uv = input.uv;
                float2 stepUV = _MainTex_TexelSize.xy * _BlurDirection.xy;

                fixed4 color = tex2D(_MainTex, uv) * 0.40;
                color += tex2D(_MainTex, uv + stepUV) * 0.24;
                color += tex2D(_MainTex, uv - stepUV) * 0.24;
                color += tex2D(_MainTex, uv + stepUV * 2.0) * 0.06;
                color += tex2D(_MainTex, uv - stepUV * 2.0) * 0.06;

                return color;
            }
            ENDCG
        }
    }

    Fallback Off
}