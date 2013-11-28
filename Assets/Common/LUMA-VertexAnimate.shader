Shader "LUMA/BoW/VertexAnimate" {
// Shader authored for waving flags. Requires the flag's pivot axes to be set so that:
// x points down the flag, away from the flag pole
// y points up
// z points out the normal of the flag mesh
// And the UVs should be mapped with in U with the flagpole end at u=0

Properties {
	_MainTex ("Base", 2D) = "white" {}
	_ScrollSpeed ("Scroll speed", Range(0, 8)) = 1
	_Amplitude ("Amplitude", Range(0,2)) = 1
}

CGINCLUDE		

struct v2f 
{
	half4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
};
	
#include "Luma_Include.cginc"

sampler2D _MainTex;
half _ScrollSpeed;
fixed _Amplitude;
						
ENDCG 

SubShader {
	LOD 300
	
	Tags { "Queue"="Geometry" "RenderType"="Opaque" }
	Cull Off
	 
	Pass {
		CGPROGRAM

		v2f vert (appdata_full v) 
		{
			v2f o;
			
			v.vertex.xyz += sin(v.vertex.x + _Time.y * _ScrollSpeed) * v.texcoord.x * fixed3(0.1,0.2,0.7) * _Amplitude;
			
			o.pos = ProjectedPosShortcut(v.vertex);
			o.uv = v.texcoord.xy;
			return o; 
		}		
		
		fixed4 frag (v2f i) : COLOR0 
		{
			fixed4 tex = tex2D (_MainTex, i.uv);
		
			return tex;		
		}	
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
		#pragma glsl_no_auto_normalization
	
		ENDCG
	}
} 
	
	///Fallback to simple shader if lower LOD is set to be used!
	Fallback "VertexLit"

}