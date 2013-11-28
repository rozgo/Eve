Shader "BeyondGames/CamoBuilder" {
	Properties {
		_MainTex ("Main (RGB)", 2D) = "white" {}
		_MainTint ("MainTint", Color) = (1,1,1,1)
		_MainSaturation ("MainSaturation", Range(0,2) ) = 1
		_MainBrightness ("MainBrightness", Range(0,2) ) = 1

		_MaskTex ("Mask (RGB)", 2D) = "white" {}
		
		_CamoTex ("Camo (RGB)", 2D) = "white" {}
		_CamoTint ("CamoTint", Color) = (1,1,1,1)
		_CamoOpacity ("CamoOpacity", Range(0,1) ) = 1
		_CamoSaturation ("CamoSaturation", Range(0,2) ) = 1
		_CamoBrightness ("CamoBrightness", Range(0,2) ) = 1

		_DirtTex ("DirtTex (RGB)", 2D) = "white" {}


	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Custom

		sampler2D _MainTex;
		half3 _MainTint;
		//half _MainHue;
		half _MainSaturation;
		half _MainBrightness;

		sampler2D _MaskTex;

		sampler2D _CamoTex;
		half3 _CamoTint;
		half _CamoOpacity;
		half _CamoSaturation;
		half _CamoBrightness;

		sampler2D _DirtTex;

		struct Input {
			half2 uv_MainTex;
		};

		fixed4 LightingCustom (SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
		    fixed4 c;
		    c.rgb = s.Albedo;
		    c.a = s.Alpha;
		    return c;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			half3 one = half3(1,1,1);

			half3 mainRGB = tex2D (_MainTex, IN.uv_MainTex) * _MainTint;
			half3 mainGray = half3((mainRGB.r + mainRGB.g + mainRGB.b)/3);
			half3 mainFinal = (2 * mainRGB * _MainSaturation + 2 * mainGray * (1 - _MainSaturation)) / 2 * _MainBrightness;
			//half3 mainFinal = mainRGB;

			half3 maskColor = tex2D (_MaskTex, IN.uv_MainTex);

			half3 camoRGB = tex2D (_CamoTex, IN.uv_MainTex) * _CamoTint;
			half3 camoGray = half3((camoRGB.r + camoRGB.g + camoRGB.b)/3);
			half3 camoFinal = (2 * camoRGB * _CamoSaturation + 2 * camoGray * (1 - _CamoSaturation)) / 2 * _CamoBrightness;
			//half3 camoFinal = camoRGB;

			half3 mainCamoMultiply = 2 * mainFinal * camoFinal;
			half3 mainCamoScreen = one - 2 * (one - mainFinal) * (one - camoFinal);

			half3 finalRGB = mainCamoScreen;
			if (mainFinal.r < 0.5)
			{
				finalRGB.r = mainCamoMultiply.r;
			}
			if (mainFinal.g < 0.5)
			{
				finalRGB.g = mainCamoMultiply.g;
			}
			if (mainFinal.b < 0.5)
			{
				finalRGB.b = mainCamoMultiply.b;
			}

			finalRGB = ( mainFinal * (1 - _CamoOpacity) + finalRGB * _CamoOpacity);

			finalRGB = finalRGB * tex2D (_DirtTex, IN.uv_MainTex);

			o.Albedo = finalRGB;
			o.Alpha = 1;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
