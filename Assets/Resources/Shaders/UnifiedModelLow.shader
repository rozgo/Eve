Shader "BeyondGames/UnifiedModelLow" {
    Properties {
        _MainTex ("Diffuse (RGB) Alpha (A)", 2D) = "white" {}
        _Tint ("Tint", Color) = (1,1,1,1)
        _BumpMap ("Normal (Normal)", 2D) = "bump" {}
        _SpecularTex ("Specular Level (R) Gloss (G) Rim Mask (B) Illum (A)", 2D) = "black" {}
        _IllumTint ("IllumTint", Color) = (1,1,1,1)
        _IllumPower ("IllumPower", Range(0,25) ) = 0
    }

    Category {

        Tags { "RenderType" = "Opaque" }

        SubShader {
            Pass {
                
                CGPROGRAM
                #pragma target 3.0
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                uniform samplerCUBE _AmbientMap;
                uniform sampler2D _AmbientRamp;
                fixed4 _Tint;
                
                struct appdata_t {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL0;
                    float2 texcoord0 : TEXCOORD0;
                    float2 texcoord1 : TEXCOORD1;
                };

                struct v2f {
                    float4 vertex : POSITION;
                    float2 texcoord0 : TEXCOORD0;
                    float2 texcoord1 : TEXCOORD1;
                    float4 color: COLOR;
                };
                
                float4 _MainTex_ST;
                float4 _AmbientRamp_ST;

                v2f vert (appdata_t v)
                {

                    v2f o;
                    o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                    o.texcoord0 = TRANSFORM_TEX(v.texcoord0, _MainTex);

                    float4 worldNormal = normalize(mul(_Object2World, float4(v.normal, 0)));
                    float NdotL = dot(worldNormal, float4(0,0,1,0)) * 0.5 + 0.5;
                    o.texcoord1 = float2(1-NdotL,0);

                    

                    float4 baseColor = float4(1,1,1,1);// * worldVertex.y;

                    //float4 color = baseColor + (topColor * dot(normal,up));
                    //o.color = color;// * max(worldVertex.y/10, 1);

                    //float2 sideCoord = TRANSFORM_TEX(v.texcoord, _WorldTex);
                    //float sideColor = tex2D(_WorldTex, sideCoord);

                    //o.normal = v.normal;

                    o.color = baseColor;//float4(v.normal,1);
                    return o;

                }
                
                fixed4 frag (v2f i) : COLOR
                {
                    //return _TintColor * tex2D(_MainTex, i.texcoord);
                    //float4 ambientColor = texCUBE(_AmbientMap, i.texcoord1);
                    //return tex2D(_MainTex, i.texcoord0) * _Tint * i.color * ambientColor;
                    float4 ambientRamp = tex2D(_AmbientRamp, i.texcoord1);
                    return tex2D(_MainTex, i.texcoord0) * _Tint * i.color * ambientRamp * 2;
                }

                ENDCG 
            }
        }
    }
}
