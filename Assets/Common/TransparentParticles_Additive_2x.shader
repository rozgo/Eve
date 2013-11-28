Shader "LUMA/Transparent/Particles/Additive-2x" {
	
Properties {
	_MainTex ("Base", 2D) = "white" {}
	_Color ("Color Tint", Color) = (1,1,1,1)
}

CGINCLUDE		

struct v2f 
{
	half4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
	fixed4 color : TEXCOORD1;
};
	
#include "LUMA_Include.cginc"

sampler2D _MainTex;
fixed4 _Color;
				
ENDCG 

SubShader {
	Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
	Blend SrcAlpha One
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	 
	Pass {
		CGPROGRAM

		half4 _MainTex_ST;		
				
		v2f vert (appdata_full v) 
		{
			v2f o;
			o.pos = ProjectedPosShortcut(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.color = v.color * _Color;
			o.color.rgb *= 2.0;
			return o; 
		}		
		
		fixed4 frag (v2f i) : COLOR0 
		{
			fixed4 tex = tex2D (_MainTex, i.uv);	
			return tex * i.color;		
		}	
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
	 	#pragma glsl_no_auto_normalization
	
		ENDCG
	}
} 
}