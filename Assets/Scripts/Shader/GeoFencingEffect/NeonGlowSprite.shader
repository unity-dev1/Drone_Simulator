Shader "Custom/URP_NeonGlowSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _CoreIntensity ("Core Intensity", Float) = 3
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Float) = 3
        _GlossColor ("Gloss Color", Color) = (1,1,1,1)
        _GlossIntensity ("Gloss Intensity", Float) = 1
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.01
        _GlowSize ("Glow Size", Range(0,1)) = 0.2
        _Origin ("Arc Origin (UV)", Vector) = (0.5,0,0,0)
        _ArcAngle ("Arc Center Angle (deg)", Float) = 90
        _ArcWidth ("Arc Width (deg)", Float) = 60
        _RippleSpeed ("Ripple Speed", Float) = 1
        _RippleWidth ("Ripple Width", Range(0,1)) = 0.1
        _RippleIntensity ("Ripple Intensity", Float) = 2
        _RippleColor ("Ripple Color", Color) = (1,1,1,1)
        _MinAlpha ("Minimum Alpha", Range(0,1)) = 0.15
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Name "ForwardUnlit"
            Blend SrcAlpha OneMinusSrcAlpha
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
            float _CoreIntensity;
            float4 _GlowColor;
            float _GlowIntensity;
            float4 _GlossColor;
            float _GlossIntensity;
            float _AlphaCutoff;
            float _GlowSize;
            float2 _Origin;
            float _ArcAngle;
            float _ArcWidth;
            float _RippleSpeed;
            float _RippleWidth;
            float _RippleIntensity;
            float4 _RippleColor;
            float _MinAlpha;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float4 tex = tex2D(_MainTex, IN.uv);
                float alpha = tex.a * _Color.a;
                if (alpha < _AlphaCutoff) discard;

                // Arc mask
                float2 dir = normalize(IN.uv - _Origin);
                float angle = atan2(dir.y, dir.x) * 57.2958; // radians to degrees
                float arcCenter = _ArcAngle;
                float arcHalf = _ArcWidth * 0.5;
                float arcMin = arcCenter - arcHalf;
                float arcMax = arcCenter + arcHalf;
                float inArc = step(arcMin, angle) * step(angle, arcMax);

                // Distance from origin for fade and ripple
                float dist = length(IN.uv - _Origin);
                float arcFade = 1.0 - saturate(dist / 0.7); // fade out as it gets farther from origin
                float arcEdge = saturate(1.0 - dist / 0.7); // 1 at origin, 0 at arc edge

                // Ripple (moving light band, clamped to arc area)
                float rippleCenter = frac(_Time.y * _RippleSpeed); // 0 to 1
                float rippleEdgeMask = step(dist, rippleCenter + _RippleWidth * 1.5) * step(rippleCenter - _RippleWidth * 1.5, dist);
                float ripple = exp(-pow((dist - rippleCenter) / _RippleWidth, 2.0)) * _RippleIntensity * inArc * arcEdge * rippleEdgeMask;
                float3 rippleColor = _RippleColor.rgb * ripple * _CoreIntensity;

                // Main color (core intensity boost)
                float3 baseColor = tex.rgb * _Color.rgb * inArc * arcFade * _CoreIntensity;

                // Glow: expand the alpha and blur for outer glow, only in arc
                float glowAlpha = smoothstep(_AlphaCutoff, _AlphaCutoff + _GlowSize, alpha) * inArc * arcFade;
                float3 glow = _GlowColor.rgb * glowAlpha * _GlowIntensity * _CoreIntensity;

                // Gloss: strong highlight at top (y > 0.7 in UV space), only in arc
                float glossMask = pow(saturate(IN.uv.y), 32.0) * _GlossIntensity * alpha * inArc * arcFade;
                float3 gloss = _GlossColor.rgb * glossMask * _CoreIntensity;

                // Final color: base + glow + gloss + ripple
                float3 finalColor = baseColor + glow + gloss + rippleColor;
                float finalAlpha = max(saturate(alpha * inArc * arcFade + glowAlpha * 0.5 + glossMask * 0.5 + ripple * 0.7), _MinAlpha);

                return float4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Unlit/Transparent"
} 