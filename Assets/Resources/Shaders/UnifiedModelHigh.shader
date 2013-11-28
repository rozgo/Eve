Shader "BeyondGames/UnifiedModelHigh" {

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
            #pragma surface surf Custom
            #pragma target 3.0

            struct Input
            {
                float2 uv_MainTex;
                float3 viewDir;
                float3 worldRefl;
                float3 worldNormal;
                INTERNAL_DATA
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
            float3 h = normalize (lightDir + viewDir);
            float NdotL = dot(s.Normal, lightDir) * 0.5 + 0.5;

            float3 ramp = tex2D(_RampTex, float2(NdotL * atten)).rgb * _RampTint.rgb * _RampTint.a;

            float nh = max (0, dot (s.Normal, h));
            //float spec = pow (nh, s.Gloss * 128) * s.Specular * saturate(NdotL);
            nh = min(nh,0.99);
            float exp_gloss = max(0.01, s.Gloss * 128);
            float gloss = pow (nh, exp_gloss);
            //gloss = clamp(gloss, 0, 1);
            float specular = s.Specular;
            //specular = clamp(s.Specular, 0, 1);
            float sx = NdotL;
            //sx = clamp(NdotL,0,0.5);
            float spec = gloss * specular * sx;
            //spec = spec;
    
            float4 c;
            c.rgb = ((s.Albedo * (ramp) * _LightColor0.rgb + _LightColor0.rgb * spec * _SpecularTint.rgb * _SpecularTint.a) * (atten * 2));
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
            o.Albedo = mc;

            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

            float3 worldNormal = WorldNormalVector(IN, o.Normal);
            float3 worldReflect = WorldReflectionVector(IN, o.Normal);

            float3 ambientColor = texCUBE(_AmbientMap, worldNormal) * _AmbientColor.rgb * _AmbientColor.a;
            
            
            float4 specGloss = tex2D(_SpecularTex, IN.uv_MainTex);
            o.Specular = specGloss.r;
            o.Gloss = specGloss.g;

            o.Albedo = mc.rgb * ambientColor * 2;
            
            float rim = 1.0 - saturate( dot ( normalize( IN.viewDir ), o.Normal ) );
            o.Emission = pow( rim, _RimPower ) 
                * _RimColor.rgb * _RimColor.a * specGloss.b + mc.rgb * specGloss.a * _IllumPower * _IllumTint;

            float3 reflectionColor = texCUBE(_ReflectionMap, worldReflect) * _ReflectionTint.rgb;
            o.Emission += reflectionColor * o.Gloss * _ReflectionTint.a;
        }
        
        ENDCG
    }

    Fallback "Diffuse"
}
