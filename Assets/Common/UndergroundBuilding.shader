Shader "Custom/UndergroundBuilding" {
	Properties {
        _MainTex ("Diffuse (RGB) Alpha (A)", 2D) = "white" {}
    }
    
    CGINCLUDE
    
    struct v2f 
	{
		half4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
	};	
	
	fixed4 _MainTex_ST;
	sampler _MainTex;
	
	#include "Luma_Include.cginc"
	
	ENDCG
	
	SubShader 
	{
		ZWrite On
		ZTest Off
		Tags {  "Queue"="Geometry-25" "RenderType"="Opaque" }
		LOD 200	
		
		Pass
		{		
			CGPROGRAM
		
			v2f vert (appdata_full v) 
			{
				v2f o;
				o.pos = ProjectedPosShortcut(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
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
			
			ENDCG			
		}		
	} 
	FallBack "Diffuse"
}
