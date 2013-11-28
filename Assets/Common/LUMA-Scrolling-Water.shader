// - Unlit
// - Scroll Base texture
// - SUPPORTS lightmap

Shader "LUMA/Scrolling/Opaque/Water" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_Cube ("Cubemap", CUBE) = "" {}
	_MainTex ("Base (RGBA)", 2D) = "white" {}
	_Normal ("Normal map", 2D) = "bump" {}
	_BumpStrength ("Bump Strength", Range(0, 2.0)) = 1.0
	_ScrollX ("Scroll speed X", Float) = 1.0
	_ScrollY ("Scroll speed Y", Float) = 0.0
}

SubShader {
	Tags {"Queue"="Geometry" "RenderType"="Opaque"}
	
	CGINCLUDE
	#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
	#include "LUMA_Include.cginc"
	
	sampler2D _MainTex;
	sampler2D _Normal;
	fixed4 _Color;
	fixed _BumpStrength;
	samplerCUBE _Cube;
	half _ScrollX;
	half _ScrollY;
	half4 _MainTex_ST;
	
	struct v2f {
		half4 pos : 		SV_POSITION;
		half4 uv : 			TEXCOORD0;
		fixed4 vertexColor:	TEXCOORD1;
		fixed3 worldRefl:	TEXCOORD2;
#ifndef LIGHTMAP_OFF
		half2 lmap : 		TEXCOORD3;
#endif
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;
		
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex) + frac(_Time.y * half2(_ScrollX, _ScrollY));
		o.uv.zw = TRANSFORM_TEX(v.texcoord, _MainTex) + frac(_Time.y * half2(_ScrollX, _ScrollY) * 0.25);
		o.uv.zw *= half2(2,2);
#ifndef LIGHTMAP_OFF
	  	o.lmap = CalculateLightmapUVs(v.texcoord1);
#endif
		o.vertexColor = _Color * v.color;
		
		fixed3 worldNormal = mul((float3x3)_Object2World, v.normal);
		o.worldRefl = CalculateReflectedNormal(v);
		
		return o;
	}
	ENDCG

	Pass {
		CGPROGRAM
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest		
		#pragma glsl_no_auto_normalization
				
		fixed4 frag (v2f i) : COLOR
		{
			half4 texColor = tex2D (_MainTex, i.uv.xy);
			half4 texColor2 = tex2D (_MainTex, i.uv.zw);
			half3 norm = UnpackNormal(tex2D(_Normal, i.uv.xy));
			
			i.vertexColor *= (texColor + texColor2) * 0.5;
			
			half3 cubeRefl = texCUBE(_Cube, i.worldRefl + norm * _BumpStrength).rgb * (texColor.a + texColor2.a) * 0.5 * _Color.a;
			
			
#ifndef LIGHTMAP_OFF
			i.vertexColor = ApplyLightmap(i.vertexColor, i.lmap);
#endif

			i.vertexColor.rgb += cubeRefl;
			return i.vertexColor;
		}
		ENDCG 
	}	
}
}
