// Shader path in Unity's shader dropdown menu
Shader "Nature/Canyon"
{
    // Define the properties visible in the Material inspector
    Properties
    {
        _Control ("Control Map", 2D) = "white" {}
        _TextureR ("Red Channel Texture", 2D) = "white" {}
        _TextureG ("Green Channel Texture", 2D) = "white" {}
        _TextureB ("Blue Channel Texture", 2D) = "white" {}
        _TextureA ("Alpha Channel Texture", 2D) = "white" {}
    }

    // Unity uses SubShaders to define a list of passes
    SubShader
    {
        // Set the tags for this Subshader
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }

        // Define a Pass
        Pass
        {
            // Use HLSL program block
            HLSLPROGRAM

            // Include necessary URP definitions
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Define the structure of our vertex input
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Define what we want to pass from the vertex shader to the fragment shader
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Vertex shader: processes each vertex of our geometry
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            // Fragment shader: processes each pixel fragment of the geometry
            half4 frag(Varyings IN) : SV_Target
            {
                // Sample the control map
                fixed4 control = SAMPLE_TEXTURE2D(_Control, sampler_Control, IN.uv);
                // Sample each texture
                fixed4 texR = SAMPLE_TEXTURE2D(_TextureR, sampler_TextureR, IN.uv);
                fixed4 texG = SAMPLE_TEXTURE2D(_TextureG, sampler_TextureG, IN.uv);
                fixed4 texB = SAMPLE_TEXTURE2D(_TextureB, sampler_TextureB, IN.uv);
                fixed4 texA = SAMPLE_TEXTURE2D(_TextureA, sampler_TextureA, IN.uv);

                // Blend the textures based on the control map's channels
                fixed4 blendedColor = texR * control.r + texG * control.g + texB * control.b + texA * control.a;

                // Output the blended color
                return half4(blendedColor.rgb, 1); // Assuming fully opaque
            }

            // End of HLSL program block
            ENDHLSL
        }
    }
}
