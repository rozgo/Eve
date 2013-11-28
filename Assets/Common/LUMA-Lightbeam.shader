Shader "LUMA/BoW/LightBeam" {
	
Properties {
	_Color ("Color Tint", Color) = (1,1,1,1)
	_RimPower("Fresnel Power", Range(0.1,8.0)) = 1.0
}

CGINCLUDE		

struct v2f 
{
	half4 pos : SV_POSITION;
	fixed4 color : TEXCOORD0;
	fixed3 fres: TEXCOORD1;
};
	
#include "LUMA_Include.cginc"

fixed4 _Color;
half _RimPower;
				
ENDCG 

SubShader {
	Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
	//Blend One OneMinusSrcAlpha
	Blend SrcAlpha One
	Cull Back Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
//	Tags { "RenderType"="Opaque" "Queue"="Transparent" }
//	Blend Off
//	Cull Off Lighting Off
	 
	Pass {
		CGPROGRAM

		v2f vert (appdata_full v)
		{
			v2f o;
			o.pos = ProjectedPosShortcut(v.vertex);
			o.color = v.color * _Color;
			
			half3 worldNormal = normalize(mul((float3x3)_Object2World, v.normal));
			half3 viewDirNorm = normalize(_WorldSpaceCameraPos.xyz - mul(_Object2World, v.vertex).xyz);
			fixed3 fres = saturate(dot(worldNormal, viewDirNorm));
			o.fres = pow(fres, _RimPower);
			
			//o.color = fixed4(worldNormal, 1);
			//o.color = fixed4(o.fres, 1);
			
			return o; 
		}		
		
		fixed4 frag (v2f i) : COLOR0 
		{
			//return i.color;
			return fixed4(i.fres,1) * i.color;
		}	
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
	 	#pragma glsl_no_auto_normalization
	
		ENDCG
	}
} 
}