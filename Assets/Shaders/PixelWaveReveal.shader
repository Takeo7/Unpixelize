Shader "UI/PixelDiagonalReveal"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _PixelSize("Max Pixel Size", Float) = 20
        _RevealProgress("Reveal Progress", Range(0, 1)) = 0.0
        _Direction("Direction (XY)", Vector) = (1, -1, 0, 0)
    }

        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 100

            Pass
            {
                ZWrite Off
                Cull Off
                Lighting Off
                Blend SrcAlpha OneMinusSrcAlpha

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"


                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _PixelSize;
                float _RevealProgress;
                float4 _Direction;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 normDir = normalize(_Direction.xy);
                    float proj = dot(i.uv, normDir);

                    float t = smoothstep(_RevealProgress - 0.02, _RevealProgress + 0.02, proj);
                    float pixelSize = lerp(_PixelSize, 1.0, t);

                    float2 pixelUV = floor(i.uv * pixelSize) / pixelSize;
                    return tex2D(_MainTex, pixelUV);
                }
                ENDCG
            }
        }
}
