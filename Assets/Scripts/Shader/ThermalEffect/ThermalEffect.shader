Shader "Custom/GrayscaleVolumeEffect"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,0.1)
        _HotColor ("Hot Color", Color) = (1, 0.7, 0, 1)     // Bright Yellow
        _WarmColor ("Warm Color", Color) = (1, 0.2, 0, 1)   // Orange-Red
        _MidColor ("Mid Color", Color) = (1, 0, 0.5, 1)     // Hot Pink
        _ColdColor ("Cold Color", Color) = (0.4, 0, 0.8, 1) // Purple
        _Intensity ("Intensity", Range(0.5, 2.0)) = 1.2
        _Contrast ("Contrast", Range(0.0, 2.0)) = 1.0
        _NoiseScale ("Noise Scale", Range(0, 100)) = 50
        _NoiseStrength ("Noise Strength", Range(0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Universal Forward"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _HotColor;
                float4 _WarmColor;
                float4 _MidColor;
                float4 _ColdColor;
                float _Intensity;
                float _Contrast;
                float _NoiseScale;
                float _NoiseStrength;
            CBUFFER_END

            // Simple noise function
            float2 hash2D(float2 p)
            {
                float2 k = float2(0.3183099, 0.3678794);
                p = p * k + k.yx;
                return -1.0 + 2.0 * frac(16.0 * k * frac(p.x * p.y * (p.x + p.y)));
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(dot(hash2D(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
                               dot(hash2D(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
                          lerp(dot(hash2D(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
                               dot(hash2D(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
            }

            float3 GetThermalColor(float t)
            {
                float3 color;
                if (t < 0.33)
                {
                    color = lerp(_ColdColor.rgb, _MidColor.rgb, t * 3.0);
                }
                else if (t < 0.66)
                {
                    color = lerp(_MidColor.rgb, _WarmColor.rgb, (t - 0.33) * 3.0);
                }
                else
                {
                    color = lerp(_WarmColor.rgb, _HotColor.rgb, (t - 0.66) * 3.0);
                }
                return color;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.uv = input.uv;
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float4 sceneColor = float4(SampleSceneColor(screenUV), 1.0);
                
                // Calculate base intensity
                float luminance = dot(sceneColor.rgb, float3(0.299, 0.587, 0.114));
                
                // Add noise based on world position
                float noiseVal = noise(input.worldPos.xy * _NoiseScale) * _NoiseStrength;
                luminance += noiseVal;
                
                // Apply contrast and intensity
                luminance = pow(saturate(luminance), _Contrast);
                luminance = pow(luminance, _Intensity);
                
                // Get thermal color
                float3 thermalColor = GetThermalColor(luminance);
                
                // Add glow to hot areas
                float3 glow = pow(luminance, 4) * _HotColor.rgb;
                float3 finalColor = thermalColor + glow * 0.3;
                
                return float4(finalColor, _Color.a);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
} 