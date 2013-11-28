// - Unlit
// - Scroll Base texture
// - SUPPORTS lightmap

Shader "LUMA/Scrolling/Opaque/1-Layer-LP" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGBA)", 2D) = "white" {}
	_SpecTex ("Spec (RGB)", 2D) = "black" {}
	_FresnelCol ("Fresnel Colour", Color) = (0,0,0,1)
	_FresnelPower("Fresnel Power", Range(0.01, 8.0)) = 3.0
	_ScrollX ("Scroll speed X", Float) = 1.0
	_ScrollY ("Scroll speed Y", Float) = 0.0
}

SubShader {
	Tags {"Queue"="Geometry" "RenderType"="Opaque" "LightMode" = "ForwardBase"}
	
	CGINCLUDE
	#include "Luma_Include.cginc"
	
	sampler2D _MainTex;
	sampler2D _SpecTex;
	fixed4 _Color;
	fixed4 _FresnelCol;
	half _FresnelPower;
	half _ScrollX;
	half _ScrollY;
	
	struct v2f {
		half4 pos : SV_POSITION;
		half4 uv : TEXCOORD0;
		fixed4 vertexColor : TEXCOORD1;
		fixed3 SHLighting : TEXCOORD2;
		fixed3 fres:	TEXCOORD3;
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = ProjectedPosShortcut(v.vertex);
		o.uv.xy = v.texcoord + frac(half2(_ScrollX, _ScrollY) * _Time.y);
		
		fixed3 worldNormal = mul((float3x3)_Object2World, v.normal);
		half3 viewDirNorm = normalize(_WorldSpaceCameraPos.xyz - mul(_Object2World, v.vertex).xyz);
			
		fixed3 fres = 1 - saturate(dot(normalize(worldNormal), viewDirNorm));
		o.fres = pow(fres, _FresnelPower) * _FresnelCol.rgb;
		
		o.SHLighting = ShadeSH9(half4(worldNormal,1));
		
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
			half4 specColor = tex2D(_SpecTex, i.uv.xy);
			i.vertexColor *= texColor;
			
			i.vertexColor.rgb *= i.SHLighting;
			i.vertexColor.rgb += i.fres * specColor;

			return i.vertexColor;
		}
		ENDCG 
	}	
}
}
