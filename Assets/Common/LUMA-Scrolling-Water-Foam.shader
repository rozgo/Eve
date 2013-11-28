// - Unlit
// - Scroll Base texture
// - SUPPORTS lightmap

Shader "LUMA/Scrolling/Transparent/WaterFoam" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGBA)", 2D) = "white" {}
	_ScrollX ("Scroll speed X", Float) = 1.0
	_ScrollY ("Scroll speed Y", Float) = 0.0
}

SubShader {
	Tags {"Queue"="Transparent-50" "RenderType"="Transparent"}
	ZWrite Off
	Blend SrcAlpha One
	
	CGINCLUDE
	#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
	#include "LUMA_Include.cginc"
	
	sampler2D _MainTex;
	fixed4 _Color;
	half _ScrollX;
	half _ScrollY;
	half4 _MainTex_ST;
	
	struct v2f {
		half4 pos : 		SV_POSITION;
		half4 uv : 			TEXCOORD0;
		fixed4 vertexColor:	TEXCOORD1;
#ifndef LIGHTMAP_OFF
		half2 lmap : 		TEXCOORD2;
#endif
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;
		
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex) + frac(_Time.y * half2(_ScrollX, _ScrollY));
		//o.uv.zw = TRANSFORM_TEX(v.texcoord, _MainTex) + frac(_Time.y * half2(_ScrollX, _ScrollY) * 0.25);
		//o.uv.zw *= half2(2,2);
#ifndef LIGHTMAP_OFF
	  	o.lmap = CalculateLightmapUVs(v.texcoord1);
#endif
		o.vertexColor = _Color * v.color;
				
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
			//half4 texColor2 = tex2D (_MainTex, i.uv.zw);
			
			//i.vertexColor *= (texColor + texColor2) * 0.5;
			i.vertexColor *= texColor;
			
#ifndef LIGHTMAP_OFF
			i.vertexColor = ApplyLightmap(i.vertexColor, i.lmap);
#endif
			return i.vertexColor;
		}
		ENDCG 
	}	
}
}
