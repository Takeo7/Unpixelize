Shader "UI/FrostedGlassDistortion"
{
    Properties
    {
        _MainTex("Background", 2D) = "white" {}
        _DistortionMap("Distortion Map", 2D) = "gray" {}
        _DistortionStrength("Distortion Strength", Float) = 0.02
        _Alpha("Alpha", Range(0,1)) = 0.6
    }

        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
            LOD 100

            Pass
            {
                Name "DistortionPass"
                Blend SrcAlpha OneMinusSrcAlpha
                Cull Off
                ZWrite Off

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                sampler2D _MainTex;
                sampler2D _DistortionMap;
                float _DistortionStrength;
                float _Alpha;
                float4 _MainTex_TexelSize;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = TransformObjectToHClip(v.vertex.xyz);
                    o.uv = v.uv;
                    return o;
                }

                float4 frag(v2f i) : SV_Target
                {
                    float2 noise = tex2D(_DistortionMap, i.uv).rg;
                    noise = noise * 2.0 - 1.0; // Convertir de [0,1] a [-1,1]
                    float2 offset = noise * _DistortionStrength;

                    float4 color = tex2D(_MainTex, i.uv + offset);
                    color.a = _Alpha;
                    return color;
                }
                ENDHLSL
            }
        }
}
