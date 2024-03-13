Shader "URP/Custom/DoubleSidedTexture"
{
    Properties
    {
        _FrontTex ("Base Front (RGB) Trans (A)", 2D) = "white" {}
        _BackTex ("Base Back (RGB) Trans (A)", 2D) = "white" {}
        _Normal ("Normal", 2D) = "bump" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.0
        _PhotoDataColor ("Photo Data Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        // Enable double-sided rendering
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            // Include required URP libraries
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;
                float3 positionWS : TEXCOORD1;
            };

            // URP material properties
            CBUFFER_START(UnityPerMaterial)
                float4 _PhotoDataColor;
                float _Metallic;
                float _Smoothness;
            CBUFFER_END

            // Texture and sampler declarations
            TEXTURE2D(_FrontTex);
            TEXTURE2D(_BackTex);
            TEXTURE2D(_Normal);
            SAMPLER(sampler_FrontTex);
            SAMPLER(sampler_BackTex);
            SAMPLER(sampler_Normal);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS);
                return OUT;
            }

            float Facing(float3 positionWS, float3 normalWS)
            {
                float3 viewDirectionWS = normalize(_WorldSpaceCameraPos - positionWS);
                return dot(viewDirectionWS, normalWS) > 0 ? 1.0 : -1.0;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float side = Facing(IN.positionWS, IN.normalWS);

                half4 frontColor = SAMPLE_TEXTURE2D(_FrontTex, sampler_FrontTex, IN.uv);
                half4 backColor = SAMPLE_TEXTURE2D(_BackTex, sampler_BackTex, IN.uv);
                half4 color = side > 0 ? frontColor : backColor;

                half3 normal = UnpackNormal(SAMPLE_TEXTURE2D(_Normal, sampler_Normal, IN.uv));
                half metallic = _Metallic;
                half smoothness = _Smoothness;

                // Lighting calculations here, if custom lighting is desired
                // Otherwise, use URP's integrated lighting

                return half4(color.rgb, 1); // Assume fully opaque
            }
            ENDHLSL
        }
    }
}
