Shader "BeyondGames/ProjectedShadow" {
	Properties {
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
	}

	Category {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Geometry" }

		Blend DstColor Zero
		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }

		SubShader {
			Pass {
				
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				fixed4 _TintColor;
				
				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};
				
				float4 _MainTex_ST;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}

				sampler2D _CameraDepthTexture;
				
				fixed4 frag (v2f i) : COLOR
				{
					float4 albedo = tex2D(_MainTex, i.texcoord);
					float4 tint = float4(_TintColor.rgb, 1);
					albedo += float4(1,1,1,1) *_TintColor.a + tint;
					return albedo;
				}
				ENDCG 
			}
		}
	}
}
