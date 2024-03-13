Shader "Custom/OnsenSurfaceURP"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _EdgeMap ("Edge Map", 2D) = "black" {}
        _PrimaryWaterColor ("Primary Water Color", Color) = (0, 0, 1, 0)
        _EdgeAlphaFalloff ("Edge Alpha Falloff", Range(0, 1)) = 0.25
        _EdgeColorFalloff ("Edge Color Falloff", Range(0, 1)) = 0.25
        _NoiseScale ("Noise Scale", Range(0, 8)) = 1.0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.25
        _NormalStrength ("Normal Strength", Range(0, 1)) = 0.5
        _NoiseSpeed ("Noise Speed", Range(0, 2)) = 2.0
        _FlowSpeed ("Flow Speed", Range(0, 4)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Name "ForwardBase"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            TEXTURE2D(_EdgeMap);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_EdgeMap);
            float4 _PrimaryWaterColor;
            float _EdgeAlphaFalloff;
            float _EdgeColorFalloff;
            float _NoiseScale;
            float _Smoothness;
            float _NormalStrength;
            float _NoiseSpeed;
            float _FlowSpeed;

            float4 ObjectToClipPos (float3 pos)
            {
                return mul (UNITY_MATRIX_VP, mul (UNITY_MATRIX_M, float4 (pos,1)));
            }



            float noiseBlend(float f) {
                float p = step(0.5, f);
                float r = f * 2.0 * (1.0 - p) + (1.0 - (f - 0.5) * 2.0) * p;
                return smoothstep(0.0, 1.0, r);
            }

            float2 pseudoRandomSample(float2 uv, float time) {
                float n1 = noiseBlend(frac(time));
                float n2 = noiseBlend(frac(time + 0.3333));
                float n3 = noiseBlend(frac(time + 0.6666));
                float flowSpeed = _Time.x * _FlowSpeed;
                return (
                    SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(flowSpeed, 0.0) + floor(time) * 0.541).rg * n1 +
                    SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-0.5 * flowSpeed, 0.87 * flowSpeed) - floor(time + 0.3333) * 0.781).rg * n2 +
                    SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-0.5 * flowSpeed, -0.87 * flowSpeed) + floor(time + 0.6666) * 0.367).rg * n3
                ) * 0.6666;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                float waveHeight = sin((IN.uv.x + _Time.y * _FlowSpeed) * _NoiseScale) * _NormalStrength * 0.1;
                float waveHeightY = cos((IN.uv.y + _Time.y * _FlowSpeed) * _NoiseScale) * _NormalStrength * 0.1;

                // Apply the calculated wave height for a subtle vertex displacement
                float3 perturbedPosition = IN.positionOS + float3(0, waveHeight + waveHeightY, 0);

                OUT.positionHCS = ObjectToClipPos(perturbedPosition);
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float edge = SAMPLE_TEXTURE2D(_EdgeMap, sampler_EdgeMap, IN.uv).r;
                float alphaFalloff = _PrimaryWaterColor.a - edge * _EdgeAlphaFalloff;
                float3 colorFalloff = _PrimaryWaterColor.rgb - edge * _EdgeColorFalloff;

                float2 noise = pseudoRandomSample(IN.uv * _NoiseScale, _Time.y * _NoiseSpeed) * _NormalStrength;
                float brightnessVariation = (noise.x + noise.y) * 0.5;
                colorFalloff += brightnessVariation * 0.1; 

                return half4(saturate(colorFalloff), alphaFalloff); // Ensure color stays within valid range using saturate
            }

            ENDHLSL
        }
    }
}
