Shader "Viva/Surface/WallStencil"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,1)) = 1.0
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _AlphaMult ("Alpha Multiply", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="AlphaTest" }

        LOD 200
        Blend One OneMinusDstAlpha

        Pass
        {
            Name "ForwardBase"
            
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _Smoothness;
            float _Cutoff;
            float _AlphaMult;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                clip(c.a - _Cutoff); // Discard the fragment if alpha is less than _Cutoff
                c.a *= _AlphaMult; // Apply alpha multiplication
                return half4(c.rgb, c.a * _Smoothness); // Return color with modified alpha
            }
            ENDHLSL
        }
    }
    // FallBack "Universal Render Pipeline/Lit"
}