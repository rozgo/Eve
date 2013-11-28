// - Unlit
// - SUPPORTS lightmap

Shader "LUMA/Transparent/Unlit/Default-2Sided-LM-Trans-10" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Transparent-10" "LightMode" = "ForwardBase" "IgnoreProjector"="True" "RenderType"="Transparent"}
	Blend SrcAlpha OneMinusSrcAlpha 
	ZWrite Off
	Cull Off
	
		
	CGINCLUDE
	#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
	#include "LUMA_Include.cginc"
	
	
	sampler2D _MainTex;
	half4 _MainTex_ST;
	
	struct v2f {
		half4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
#ifndef LIGHTMAP_OFF
		half2 lmap : TEXCOORD1;
#endif
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = ProjectedPosShortcut(v.vertex);
		o.uv = v.texcoord;
		
#ifndef LIGHTMAP_OFF
	  	o.lmap = CalculateLightmapUVs(v.texcoord1);
#endif
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
			fixed4 tex = tex2D (_MainTex, i.uv);
			
#ifndef LIGHTMAP_OFF
			tex = ApplyLightmap(tex, i.lmap);
#endif
			return tex;
		}
		ENDCG 
	}	
}
}
