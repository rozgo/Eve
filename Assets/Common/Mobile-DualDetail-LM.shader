Shader "LUMA/Detail/Mobile-ShoreBlend-LM" {
	
Properties {
	_MainTex ("Base", 2D) = "white" {}
	_DetailTex ("Detail (RG [565])", 2D) = "white" {}
}

CGINCLUDE		

struct v2f 
{
	half4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
	half2 uvDetail : TEXCOORD1;
	half2 uvDetail2: TEXCOORD2;
	fixed4 vcol : TEXCOORD3;
#ifdef LIGHTMAP_ON
	half2 uv2 : TEXCOORD4;
#endif
};
	
#include "LUMA_Include.cginc"


sampler2D _MainTex;
sampler2D _DetailTex;
						
ENDCG 

SubShader {
	LOD 300
	
	Tags { "Queue"="Geometry" "RenderType"="Opaque" }
	 
	Pass {
		CGPROGRAM

		half4 _MainTex_ST;		
		half4 _DetailTex_ST;
				
		v2f vert (appdata_full v) 
		{
			v2f o;
			o.pos = ProjectedPosShortcut(v.vertex);
			o.uv = v.texcoord;
			o.uvDetail = DETAIL_TEX_TILE(v.texcoord,_DetailTex);
			o.uvDetail2 = v.texcoord * _DetailTex_ST.zw;
			o.vcol = v.color;
#ifdef LIGHTMAP_ON
	  		o.uv2 = CalculateLightmapUVs(v.texcoord1);
#endif
			return o; 
		}		
		
		fixed4 frag (v2f i) : COLOR0 
		{
			fixed4 maintex = tex2D (_MainTex, i.uv);	
			
			fixed detail = tex2D (_DetailTex, i.uvDetail).r;
			fixed detail2 = tex2D (_DetailTex, i.uvDetail2).g;
			fixed blendedDetail = lerp(detail, detail2, i.vcol.a);
			
			//fixed4 tex = lerp(maintex, i.vcol, i.vcol.a);
			fixed4 tex = lerp(maintex, i.vcol, /* saturate(6 * detail2 - 2) * */ i.vcol.a);
			
			tex = ApplyDetailMap(tex, blendedDetail);
			//tex.rgb *= i.vcol.rgb;
			
#ifdef LIGHTMAP_ON
			tex = ApplyLightmap(tex, i.uv2);
#endif	
			//return fixed4(i.vcol.a, i.vcol.a, i.vcol.a, i.vcol.a);
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