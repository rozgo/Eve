Shader "LUMA/BoW/Mobile-DetailMap-VCol" {
	
Properties {
	_Color ("Main Colour", Color) = (1,1,1,1)
	_MainTex ("Base", 2D) = "white" {}
}

CGINCLUDE		

struct v2f 
{
	half4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
	fixed4 vcol : TEXCOORD1;
#ifdef LIGHTMAP_ON
	half2 uv2 : TEXCOORD2;
#endif
};
	
#include "LUMA_Include.cginc"

sampler2D _MainTex;
fixed4 _Color;
						
ENDCG 

SubShader {
	LOD 300
	
	Tags { "Queue"="Geometry" "RenderType"="Opaque" }
	 
	Pass {
		CGPROGRAM

		half4 _MainTex_ST;
				
		v2f vert (appdata_full v) 
		{
			v2f o;
			o.pos = ProjectedPosShortcut(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.vcol = v.color;
#ifdef LIGHTMAP_ON
	  		o.uv2 = CalculateLightmapUVs(v.texcoord1);
#endif
			return o; 
		}		
		
		fixed4 frag (v2f i) : COLOR0 
		{
			fixed4 tex = tex2D(_MainTex, i.uv);	
			tex = i.vcol + (tex - 0.5) * 2.0 * _Color.a;
			//tex *= _Color;
			
#ifdef LIGHTMAP_ON
			tex = ApplyLightmap(tex, i.uv2);
#endif
			return tex;
		}	
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
		#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
		#pragma glsl_no_auto_normalization
	
		ENDCG
	}
} 
	
	///Fallback to simple shader if lower LOD is set to be used!
	Fallback "LUMA/Optimal/Fastest-Opaque-Dual"

}