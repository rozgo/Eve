Shader "LUMA/BoW/BrinkOfWarGround" {
	
Properties {
	_MainTex ("Base", 2D) = "white" {}
	_DetailTex ("Detail (Alpha8)", 2D) = "white" {}
	_MicroTex ("Micro (Alpha8)", 2D) = "white" {}
}

CGINCLUDE		

struct v2f 
{
	half4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
	half2 uvDetail : TEXCOORD1;
	half2 uvMicro: TEXCOORD2;
	fixed4 vcol : TEXCOORD3;
#ifdef LIGHTMAP_ON
	half2 uv2 : TEXCOORD4;
#endif
};
	
#include "Luma_Include.cginc"

sampler2D _MainTex;
sampler2D _DetailTex;
sampler2D _MicroTex;
						
ENDCG 

SubShader {
	LOD 300
	
	Tags { "Queue"="Geometry" "RenderType"="Opaque" }
	 
	Pass {
		CGPROGRAM

		half4 _MainTex_ST;	
		half4 _DetailTex_ST;
		half4 _MicroTex_ST;
				
		v2f vert (appdata_full v) 
		{
			v2f o;
			o.pos = ProjectedPosShortcut(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.uvDetail = DETAIL_TEX_TILE(v.texcoord,_DetailTex);
			o.uvMicro = TRANSFORM_TEX(v.texcoord, _MicroTex);
			o.vcol = v.color;
#ifdef LIGHTMAP_ON
	  		o.uv2 = CalculateLightmapUVs(v.texcoord1);
#endif
			return o; 
		}		
		
		fixed4 frag (v2f i) : COLOR0 
		{
			fixed4 tex = tex2D (_MainTex, i.uv);	
			fixed detail = tex2D (_DetailTex, i.uvDetail).a;
			fixed micro = tex2D(_MicroTex, i.uvMicro).a;
			
			tex = ApplyDetailMap(tex, detail);
			tex = ApplyDetailMap(tex, micro);
			tex *= i.vcol;
			
			
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