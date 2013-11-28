Shader "BeyondGames/UnifiedModelVert" {
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
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _TintColor;
                
                struct appdata_t {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                };
                
                float4 _MainTex_ST;

                v2f vert (appdata_t v)
                {
                    v2f o;
                    o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                    o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                    return o;
                }
                
                fixed4 frag (v2f i) : COLOR
                {
                    //return _TintColor * tex2D(_MainTex, i.texcoord);
                    return tex2D(_MainTex, i.texcoord);
                }
                ENDCG 
            }
        }
    }
}
