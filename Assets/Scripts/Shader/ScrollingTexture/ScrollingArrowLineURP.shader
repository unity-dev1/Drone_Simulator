Shader "Custom/ScrollingArrowLineURP"
{
    Properties
    {
        _MainTex ("Arrow Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.2, 0.6, 1, 1)
        _Speed ("Scroll Speed", Float) = 1
        _Tiling ("Tiling", Vector) = (3, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Speed;
            float4 _Tiling;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float scroll = frac(_Time.y * _Speed);
                int maxArrows = 10;
                float N = clamp(_Tiling.x, 1.0, maxArrows);
                float arrowWidth = 1.0 / N;
                half4 col = half4(0,0,0,0);
                for (int i = 0; i < maxArrows; i++) {
                    if (i >= int(N)) break;
                    float left = fmod(arrowWidth * i + scroll, 1.0);
                    float right = left + arrowWidth;
                    // Only draw if fully inside [0,1]
                    float mask = step(0.0, left) * step(right, 1.0);
                    float localX = (uv.x - left) / arrowWidth;
                    float2 arrowUV = float2(localX, uv.y * _Tiling.y);
                    float inArrow = step(0.0, localX) * step(localX, 1.0);
                    half4 tex = tex2D(_MainTex, arrowUV);
                    col += tex * _Color * (tex.a * mask * inArrow);
                }
                return col;
            }
            ENDHLSL
        }
    }
} 