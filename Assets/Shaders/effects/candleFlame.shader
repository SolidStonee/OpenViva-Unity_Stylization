﻿Shader "Viva/Effects/candleFlame"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _Size ("Size", Range(0.01,0.1)) = 1.0
        _Color ("Color", Color ) = (1.,1.,1.,1.)
	}
	SubShader
	{
		Tags {
			"Queue"="Transparent+1"
			"RenderType"="Transparent"
		}
		LOD 100
		Blend One One
		Zwrite off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
            fixed _Size;
            fixed4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				// billboard mesh towards camera

				float3 baseWorldPos = unity_ObjectToWorld._m30_m31_m32;
				float flickerA = 1.0+(cos(_Time.y*3.7)*.005+cos(_Time.z*2.57)*.005)*v.uv.y;
				float flickerB = 1.0+(cos(_Time.z*4.3)*.01+cos(_Time.z*3.17)*.01)*v.uv.y;
				float3 vpos = baseWorldPos+v.vertex.xyz*_Size;
				vpos.xy = (vpos.xy-.5)*float2(flickerA,flickerB)+.5;
				
				float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
				float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
				
				o.vertex = mul(UNITY_MATRIX_P, viewPos);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i)

                fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb *= _Color.rgb*col.a;
				return col;
			}
			ENDCG
		}
	}
}
