Shader "UI/PixelationMasked"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _PixelSize("Pixel Size", Float) = 10.0
    }

        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "CanUseSpriteAtlas" = "False" }
            ZWrite Off
            Cull Off
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                Name "Default"
                Tags { "LightMode" = "Always" }

            // 💡 Este stencil es el que espera Unity para UI Masks
            Stencil
            {
                Ref 1
                Comp Equal
                Pass Keep
            }

            ColorMask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _PixelSize;

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
                float2 pixelUV = floor(i.uv * _PixelSize) / _PixelSize;
                return tex2D(_MainTex, pixelUV);
            }
            ENDCG
        }
        }
}
