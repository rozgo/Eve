Shader "BeyondGames/UnifiedModel" {

    Properties {
        //_RimColor ("Rim Color", Color) = (0.97,0.88,1,0.75)
        //_RimPower ("RimPower", Range(0.1,3) ) = 3
        _MainTex ("Diffuse (RGB) Alpha (A)", 2D) = "white" {}
        _Tint ("Tint", Color) = (1,1,1,1)
        _BumpMap ("Normal (Normal)", 2D) = "bump" {}
        _SpecularTex ("Specular Level (R) Gloss (G) Rim Mask (B) Illum (A)", 2D) = "black" {}
        //_SpecularTint ("SpecularTint", Color) = (1,1,1,1)
        _IllumTint ("IllumTint", Color) = (1,1,1,1)
        _IllumPower ("IllumPower", Range(0,25) ) = 0
        //_RampTex ("Toon Ramp (RGB)", 2D) = "white" {}
        //_ReflectionMap ("IBL Tex", Cube) = "_Skybox" { TexGen CubeNormal }
        //_AmbientTex ("IBL Tex", Cube) = "_Skybox" { TexGen CubeNormal }
        //_AmbientColor ("IBL Color", Color) = (1,1,1,0)
    }

    SubShader{
        Tags { "RenderType" = "Opaque" }
        
        CGPROGRAM
            #pragma surface surf Lambert
            #pragma target 3.0

            struct Input
            {
                float2 uv_MainTex;
                float3 worldNormal;
            };

            uniform sampler2D _RampTex;
            uniform samplerCUBE _AmbientMap;
            uniform samplerCUBE _ReflectionMap;
            uniform float4 _ReflectionTint;
            uniform float4 _AmbientColor;
            uniform float _AmbientPower;
            uniform float4 _RimColor;
            uniform float _RimPower;
            uniform float4 _SpecularTint;
            uniform float4 _RampTint;
            
            sampler2D _MainTex, _SpecularTex, _BumpMap;
            float4 _IllumTint;
            float _IllumPower;
            float4 _Tint;

        inline float4 LightingCustom (SurfaceOutput s, float3 lightDir, float3 viewDir, float atten)
        {
    
            float4 c;
            c.rgb = ((s.Albedo * _LightColor0.rgb) * (atten));
            c.a = 1;
            //c.a = s.Alpha;
            //c.rgb = s.Albedo;
            return c;
        }
      
        void surf (Input IN, inout SurfaceOutput o)
        {
            //float4 ma = tex2Dbias( _MainTex, float4(IN.uv_MainTex.x, IN.uv_MainTex.y, 0.0, -1.0) );
            float4 ma = tex2D( _MainTex, IN.uv_MainTex.xy);
            float4 mc = _Tint * ma;
            mc.a = 1;
            //o.Albedo = float4(1,0,0,1);

            //o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

            float3 worldNormal = WorldNormalVector(IN, o.Normal);
            //float3 worldReflect = WorldReflectionVector(IN, o.Normal);

            float3 ambientColor = texCUBE(_AmbientMap, worldNormal) * _AmbientColor.rgb * _AmbientColor.a;


            //o.Albedo = mc.rgb * pow(ambientColor, _AmbientPower) * 2;
            o.Albedo = mc.rgb * ambientColor * 2 + float4(0.1,0,0,0);
            //o.Albedo = float4(1,0,0,1);
        }
        
        ENDCG
    }

    Fallback "Diffuse"
}
