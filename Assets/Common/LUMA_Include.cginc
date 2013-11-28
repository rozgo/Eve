
#ifndef LUMA_CG_INCLUDED
#define LUMA_CG_INCLUDED

#include "UnityCG.cginc"
#include "Lighting.cginc"


///enable if we need to clamp reflection sampling, due to reflections not being in a 0-1 clamped texture (ie. in a shared Render Target)
//#define REFLECTION_SAMPLE_CLAMPING

///enable if we need to clamp shadow sampling, due to shadows not being in a 0-1 clamped texture (ie. in a shared Render Target)
//#define SHADOW_SAMPLE_CLAMPING

///enable if we want Luma controlled single directional lighting per scene
//#define CUSTOM_DIRECTIONAL_LIGHT

///enable if we want Luma controlled specular stuff
//#define CUSTOM_SPECULAR


///these are items that can be lower quality on some platforms, but not others!
///TODO: nice way to auto set these to half values for some Android builds that can handle them
///Androids that work as half values:	Tegra3 | Mali 400 | ????
///
#define PLATFORM_FLOAT2		float2
#define PLATFORM_FLOAT4x4	float4x4


#define DETAIL_TEX_TILE(tex, name) 	(tex.xy * name##_ST.xy)
#define VERTEX_NORMAL				(v.normal * unity_Scale.w)

uniform PLATFORM_FLOAT4x4 _LmMatrix;
uniform PLATFORM_FLOAT4x4 _LmMatrixRed;
uniform PLATFORM_FLOAT4x4 _LmMatrixGreen;
uniform PLATFORM_FLOAT4x4 _LmMatrixBlue;
uniform PLATFORM_FLOAT4x4 _LmMatrixAlpha;

uniform half4 unity_LightmapST;
uniform sampler2D unity_Lightmap;

uniform sampler2D _LM;

uniform fixed _ShadowIntensity;			
uniform fixed3	_WorldLightDirection;
uniform	half3 _SpecLightPos;
uniform	half3 _SpecLightPosFlipped;
uniform half _SpecStrengthScale;
uniform fixed3 _Specular_LightColor;

uniform float4	_ElapsedTime;	///follows Unity Time format (t/20, t, t*2, t*3)... MUST be float4 for precision or you get jumpy stuff on mobile after time
uniform fixed3	_WindDirection;
uniform fixed	_WindWave;

uniform half4	_ReflectionRenderTargetOffsetScale;
uniform half4 	_ReflectionUVMinMax;
uniform half4	_ShadowUVMinMax;
 
  
 
 
    
inline float4 ComputeScreenPosShortcut(half4 projectedPos, half4 centerOffsetScale) 
{
	half4 o = projectedPos;
	
	///apply centering scale
	half2 centerScale = centerOffsetScale.zw;
	centerScale.y *= _ProjectionParams.x;
	o.xy *= centerScale;

	///apply centering offset	
	half2 centerOffset = projectedPos.w * centerOffsetScale.xy;
#if defined(UNITY_HALF_TEXEL_OFFSET)
	centerOffset *= _ScreenParams.zw;
#endif
	o.xy += centerOffset;
	return o;
}


inline half4 ProjectedPosShortcut(float4 objSpaceVert)
{
	return mul((half4x4)UNITY_MATRIX_MVP, objSpaceVert);
}


inline half3 ObjectToWorldTransform(half4 vert)
{
	return mul((half4x4)_Object2World, vert).xyz;
}


inline fixed3 ObjectToWorldRotation(half3 vec)
{
	return fixed3(mul((half3x3)_Object2World, vec).xyz);
}


inline fixed3x3 CalculateTBN(appdata_full v)
{
	half3 binormal = cross(v.normal, v.tangent.xyz) * v.tangent.w;
	return fixed3x3(v.tangent.xyz, binormal, v.normal);
}


inline fixed3 WorldLightDir(half4 objPos)
{
#if CUSTOM_DIRECTIONAL_LIGHT
	return _WorldLightDirection;
#else
	return WorldSpaceLightDir(half4(ObjectToWorldTransform(objPos), 1.0f));
#endif
}


inline fixed3 ObjectLightDir(half4 objPos)
{
#if CUSTOM_DIRECTIONAL_LIGHT
	return mul((half3x3)_World2Object, WorldLightDir()).xyz;
#else
	return mul((half3x3)_World2Object, WorldLightDir(objPos)).xyz;
#endif
}


inline half3 ObjSpaceViewDirShortcut(in half3 vInObj)
{
	half3 objSpaceCameraPos = mul((half4x4)_World2Object, half4(_WorldSpaceCameraPos.xyz, 1)).xyz * unity_Scale.w;
	return objSpaceCameraPos - vInObj;
}


inline half3 WorldSpaceViewDirShortcut(in half4 vInObj)
{
	half3 worldPos = ObjectToWorldTransform(vInObj);	
	return _WorldSpaceCameraPos.xyz - worldPos;
}


inline fixed3 GetSpecularColor(in half amount)
{
#if CUSTOM_SPECULAR
	return _Specular_LightColor * (_SpecStrengthScale * amount);	
#else
	return _LightColor0.rgb * (_SpecStrengthScale * amount);	
#endif
}


inline fixed3 EthansFakeReflection (half4 vtx) 
{
	half4 reflection;
	reflection.xyz = ObjectToWorldTransform(vtx);
	reflection.xy = (-_WorldSpaceCameraPos.xz * 0.6 + reflection.xz) * 0.07;
	return fixed3(reflection.xyz);
}


inline void WorldSpaceNormalAndViewDirShortcut(appdata_full v, out fixed3 worldNormal, out half3 viewDir)
{
	worldNormal = ObjectToWorldRotation(VERTEX_NORMAL);
	viewDir = WorldSpaceViewDirShortcut(v.vertex);
}


inline fixed3 CalculateReflectedNormal(appdata_full v)
{
	fixed3 worldNormal;
	half3 viewDir;
	WorldSpaceNormalAndViewDirShortcut(v, worldNormal, viewDir);
	fixed3 viewDirNorm = normalize(viewDir);
	return reflect(-viewDirNorm, worldNormal);
}


inline fixed3 CalculateReflectedNormalWorldOut(appdata_full v, out fixed3 worldNormal)
{
	half3 viewDir;
	WorldSpaceNormalAndViewDirShortcut(v, worldNormal, viewDir);
	fixed3 viewDirNorm = normalize(viewDir);
	return reflect(-viewDirNorm, worldNormal);
}


inline fixed3 CalculateReflectedNormalWorldAndViewOut(appdata_full v, out fixed3 worldNormal, out fixed3 viewDirNorm)
{
	half3 viewDir;
	WorldSpaceNormalAndViewDirShortcut(v, worldNormal, viewDir);
	viewDirNorm = normalize(viewDir);
	return reflect(-viewDirNorm, worldNormal);
}


inline half CalculateSpecularNH(appdata_full v, fixed3 lightDir)
{
	fixed3 worldNormal;
	half3 viewDir;
	WorldSpaceNormalAndViewDirShortcut(v, worldNormal, viewDir);
	fixed3 viewDirNorm = normalize(viewDir);
	fixed3 halfDir = normalize(lightDir + viewDirNorm);
	return saturate(dot(worldNormal, halfDir));
}


inline half CalculateSpecularAmount(appdata_full v, fixed3 lightDir, half falloff)
{
	half nh = CalculateSpecularNH(v, lightDir);
	
	///NOTE: If you try to return the following line straight, Unity or the Cg compiler goes BOOOM>!>!>!
	half res = pow(nh, falloff);
	return res;
}


inline fixed3 CalculateSpecularColor(appdata_full v, fixed3 lightDir, half falloff)
{
	half amount = CalculateSpecularAmount(v, lightDir, falloff);
	return GetSpecularColor(amount);
}


inline fixed3 CalculateSpecularColorFlat(appdata_full v, half specStr)
{
#if CUSTOM_SPECULAR
	fixed3 specDir = normalize(_SpecLightPosFlipped - v.vertex);
#else
	fixed3 specDir = normalize(WorldLightDir(v.vertex));
#endif
	half amount = CalculateSpecularNH(v, specDir) * specStr;
	return GetSpecularColor(amount);
}

inline fixed4 ApplySpecularUsingDetail(fixed4 tex, fixed3 spec, fixed detail)
{
	///apply detail map based specular, using the upper half of the detail map
	tex.rgb += spec * saturate(detail - 0.5);
	return tex;
}


inline fixed3 LightingLambertVS (fixed3 norm, half3 lightDir)
{
	fixed3 col = UNITY_LIGHTMODEL_AMBIENT.xyz;
	fixed diff = max (0, dot (norm, lightDir));
	return col + fixed3(_LightColor0.rgb * (diff * 2));
}


inline fixed3 GetSphericalHarmonics(appdata_full v)
{
	fixed3 worldNormal = ObjectToWorldRotation(VERTEX_NORMAL);
	return saturate(fixed3(ShadeSH9(half4(worldNormal, 1))));
}


inline fixed3 GetSphericalHarmonicsNormalIn(appdata_full v, fixed3 worldNormal)
{
	return saturate(fixed3(ShadeSH9(half4(worldNormal, 1))));
}


inline fixed4 GetSphericalHarmonicsWithIntensity(appdata_full v)
{
	fixed3 worldNormal = ObjectToWorldRotation(VERTEX_NORMAL);
	fixed4 sh;
	sh.rgb = saturate(fixed3(ShadeSH9(half4(worldNormal, 1))));
	sh.a = dot(fixed3(1, 1, 1), sh.rgb) * 0.3333;
	
	return sh;
}


inline fixed3 GetSphericalHarmonicsNormalOut(appdata_full v, out fixed3 worldNormal)
{
	worldNormal = ObjectToWorldRotation(VERTEX_NORMAL);
	return saturate(fixed3(ShadeSH9(half4(worldNormal, 1))));
}


inline fixed4 GetSphericalHarmonicsWithIntensityNormalOut(appdata_full v, out fixed3 worldNormal)
{
	worldNormal = ObjectToWorldRotation(VERTEX_NORMAL);
	fixed4 sh;
	sh.rgb = saturate(fixed3(ShadeSH9(half4(worldNormal, 1))));
	sh.a = dot(fixed3(1, 1, 1), sh.rgb) * 0.3333;
	
	return sh;
}


inline fixed GetFresnelTerm(appdata_full v)
{
	fixed3 worldNormal;
	half3 viewDir;
	WorldSpaceNormalAndViewDirShortcut(v, worldNormal, viewDir);
	fixed3 viewDirNorm = fixed3(normalize(viewDir));

	return saturate(dot(viewDirNorm, worldNormal));
}


inline fixed4 CalculateLightingSpecularLookup(fixed3 albedo, fixed gloss, sampler2D specLUT, fixed3 nlAndSh, fixed nh, fixed3 sh)
{
	fixed spec = tex2D(specLUT, fixed2(nh, 0)).a * gloss;
	fixed4 c = fixed4(albedo * nlAndSh, gloss);
	c.rgb += GetSpecularColor(spec * sh.x);
	
	return c;
}

		
inline fixed4 LightingMobileBumpSpecPixelLit (fixed3 albedo, fixed gloss, fixed3 norm, sampler2D specLUT, fixed3 lightDir, fixed3 halfDir, fixed3 sh)
{
	fixed nl = saturate(dot(norm, lightDir));
	fixed nh = dot(norm, halfDir);
	return CalculateLightingSpecularLookup(albedo, gloss, specLUT, fixed3(nl, nl, nl) + sh, nh, sh);
}


inline fixed4 LightingMobileBumpSpecPixelLitNLOut (fixed3 albedo, fixed gloss, fixed3 norm, sampler2D specLUT, fixed3 lightDir, fixed3 halfDir, fixed3 sh, out fixed nl)
{
	nl = saturate(dot(norm, lightDir));
	fixed nh = dot(norm, halfDir);
	return CalculateLightingSpecularLookup(albedo, gloss, specLUT, fixed3(nl, nl, nl) + sh, nh, sh);
}


inline fixed3 TransformObjectSpaceToTangent(appdata_full v, fixed3 vec)
{
	///calculate TBN Matrix
	fixed3x3 TBNMatrix = CalculateTBN(v);
	
	///transform vector into tangent space
	return mul(TBNMatrix, vec);
}


inline void TransformLightAndHalfToTangent(appdata_full v, inout fixed3 lightDirObj, inout fixed3 halfDirObj)
{
	///calculate TBN Matrix
	fixed3x3 TBNMatrix = CalculateTBN(v);
	
	///transform vectors into tangent space
	lightDirObj = mul(TBNMatrix, lightDirObj);
	halfDirObj = mul(TBNMatrix, halfDirObj);
}

inline void ObjSpaceViewAndLightDirShortcut(half4 vert, out fixed3 viewDir, out fixed3 lightDir)
{
	viewDir = normalize(ObjSpaceViewDirShortcut(vert.xyz));
	lightDir = ObjectLightDir(vert);
}


inline void WorldSpaceViewAndLightDirShortcut(half4 vert, out fixed3 viewDir, out fixed3 lightDir)
{
	viewDir = normalize(WorldSpaceViewDirShortcut(vert));
	lightDir = WorldLightDir(vert);
}


inline fixed GetTextureBlendFromVertexColor(fixed4 vColor)
{
	///NOTE: Using .r for now, as we've had trouble getting .a from XSI
	return vColor.r;
}


inline half2 CalculateLightmapUVs(half2 lmuv)
{
	return lmuv * unity_LightmapST.xy + unity_LightmapST.zw;
}


inline fixed4 ApplyLightmap(fixed4 tex, half2 lmuv)
{
	fixed3 lm = DecodeLightmap (tex2D(unity_Lightmap, lmuv));
	tex.rgb *= lm;
	
	return tex;
}


inline fixed4 ApplyLightmap2(inout fixed4 tex1, inout fixed4 tex2, half2 lmuv)
{
	fixed3 lm = DecodeLightmap (tex2D(unity_Lightmap, lmuv));
	tex1.rgb *= lm;
	tex2.rgb *= lm;	
	
	return tex1;
}


inline fixed4 ApplyLightmapSaturated(fixed4 tex, half2 lmuv, fixed4 sat)
{
	fixed3 lm = DecodeLightmap (tex2D(unity_Lightmap, lmuv));
	tex.rgb *= saturate(lm + sat.rgb);
	
	return tex;
}


inline fixed DecodeDetailPlusMinus(fixed detail)
{
	return (detail - 0.5) * 2.0;
}


inline fixed4 ApplyDetailMap(fixed4 tex, fixed detail)
{
	fixed f = DecodeDetailPlusMinus(detail);
	tex.rgb += fixed3(f, f, f);
	return tex;
}


inline fixed4 ApplyDetailMapMasked(fixed4 tex, fixed detail, fixed mask)
{
	fixed f = DecodeDetailPlusMinus(detail) * mask;
	tex.rgb += fixed3(f, f, f);
	return tex;
}


inline fixed DetailMapLuminosityMask(fixed4 tex)
{
	///grayscale of the detail map is used for "dark-masking"
	return saturate(dot(fixed4(1, 1, 1, 0), tex));
}


inline fixed4 ApplyBurnEffect(fixed4 tex, fixed4 burn, fixed4 burnMulLookup, fixed4 burnAddLookup, fixed3 emberFactor)
{
	fixed4 texRes = lerp(burn,
   				fixed4((tex.rgb * burnMulLookup.rgb) + (burnAddLookup.rgb * emberFactor), tex.a),
   				burnMulLookup.a);
   
	return texRes;
}


inline PLATFORM_FLOAT2 CalculateShadowSpace(half4 vert)
{
	return mul(_LmMatrix, vert).xy;
}


inline void CalculateShadowSpace1Channel(half4 vert, out half2 lmspace1)
{
	lmspace1 = mul(_LmMatrixRed, vert).xy;
}


inline void CalculateShadowSpace2Channels(half4 vert, out half2 lmspace1, out half2 lmspace2)
{
	CalculateShadowSpace1Channel(vert, lmspace1);
	lmspace2 = mul(_LmMatrixGreen, vert).xy;
}


inline void CalculateShadowSpace3Channels(half4 vert, out half2 lmspace1, out half2 lmspace2, out half2 lmspace3)
{
	CalculateShadowSpace2Channels(vert, lmspace1, lmspace2);
	lmspace3 = mul(_LmMatrixBlue, vert).xy;
}


inline void CalculateShadowSpace4Channels(half4 vert, out half2 lmspace1, out half2 lmspace2, out half2 lmspace3, out half2 lmspace4)
{
	CalculateShadowSpace3Channels(vert, lmspace1, lmspace2, lmspace3);
	lmspace4 = mul(_LmMatrixAlpha, vert).xy;
}


inline fixed4 ApplyShadowMap(fixed4 tex, PLATFORM_FLOAT2 lmspace)
{
	fixed3 shadows = tex2D(_LM, lmspace).xyz;
	tex.rgb *= shadows;
	
	return tex;
}

inline fixed4 ApplyShadowMapSaturated(fixed4 tex, PLATFORM_FLOAT2 lmspace, fixed3 sat)
{
	fixed3 shadows = tex2D(_LM, lmspace).xyz;
	tex.rgb *= saturate(shadows + sat);
	
	return tex;
}

inline fixed4 ApplyShadowMapSHClamped(fixed4 tex, PLATFORM_FLOAT2 lmspace, fixed shIntensity)
{
	fixed3 shadows = tex2D(_LM, lmspace).xyz;
	tex.rgb *= shadows + (1 - shIntensity);
	
	return tex;
}		


inline fixed4 ApplyShadowMapNLScaled(fixed4 tex, PLATFORM_FLOAT2 lmspace, fixed nl)
{
	fixed3 shadows = tex2D(_LM, lmspace).xyz;

	///push n.l to consider even larger angles to be direct on lighting hit... only fade out towards 90deg
	nl = saturate(nl * 2.0);
	tex.rgb *= lerp(fixed3(1, 1, 1), shadows.rgb, nl);
	
	return tex;
}			


inline fixed GetShadowAdjustedColor(fixed shadowColor)
{
	return 1.0 - (_ShadowIntensity * shadowColor);
}


inline fixed4 BurnShadowColor(fixed4 tex, fixed shadowColor)
{
	shadowColor = GetShadowAdjustedColor(shadowColor);	
	tex.rgb *= shadowColor;
	return tex;
}


inline half2 ClampShadowUVs(half2 uv)
{
#ifdef SHADOW_SAMPLE_CLAMPING
	return clamp(uv, _ShadowUVMinMax.xy, _ShadowUVMinMax.zw);
#else
	return uv;
#endif
}


inline fixed4 ApplyShadowMap1Channel(fixed4 tex, half2 lmspace1)
{
	fixed shadowColor = tex2D(_LM, ClampShadowUVs(lmspace1)).r;
	return BurnShadowColor(tex, shadowColor);
}


inline fixed ApplyShadowMap1ChannelNoTex(half2 lmspace1)
{
	fixed shadowColor = tex2D(_LM, ClampShadowUVs(lmspace1)).r;
	return GetShadowAdjustedColor(shadowColor);
}


inline fixed4 ApplyShadowMap2Channels(fixed4 tex, half2 lmspace1, half2 lmspace2)
{
	fixed shadowColor = tex2D(_LM, ClampShadowUVs(lmspace1)).r + tex2D(_LM, ClampShadowUVs(lmspace2)).g;
	return BurnShadowColor(tex, saturate(shadowColor));
}


inline fixed ApplyShadowMap2ChannelsNoTex(half2 lmspace1, half2 lmspace2)
{
	fixed shadowColor = tex2D(_LM, ClampShadowUVs(lmspace1)).r + tex2D(_LM, ClampShadowUVs(lmspace2)).g;
	return GetShadowAdjustedColor(saturate(shadowColor));
}


inline fixed4 ApplyShadowMap3Channels(fixed4 tex, half2 lmspace1, half2 lmspace2, half2 lmspace3)
{
	// D3D doesn't like those three tex2Ds thrown into the dot
	// Seems to be a 3.5 issue
#if SHADER_API_D3D9
	fixed shadowColor = tex2D(_LM, ClampShadowUVs(lmspace1)).r + tex2D(_LM, ClampShadowUVs(lmspace2)).g + tex2D(_LM, ClampShadowUVs(lmspace3)).b;
#else
	fixed shadowColor = dot(fixed4(1, 1, 1, 0), fixed4(tex2D(_LM, ClampShadowUVs(lmspace1)).r, tex2D(_LM, ClampShadowUVs(lmspace2)).g, tex2D(_LM, ClampShadowUVs(lmspace3)).b, 0));
#endif
		
	return BurnShadowColor(tex, saturate(shadowColor));
}


inline fixed ApplyShadowMap3ChannelsNoTex(half2 lmspace1, half2 lmspace2, half2 lmspace3)
{
	// D3D doesn't like those three tex2Ds thrown into the dot
	// Seems to be a 3.5 issue
#if SHADER_API_D3D9
	fixed shadowColor = tex2D(_LM, ClampShadowUVs(lmspace1)).r + tex2D(_LM, ClampShadowUVs(lmspace2)).g + tex2D(_LM, ClampShadowUVs(lmspace3)).b;
#else
	fixed shadowColor = dot(fixed4(1, 1, 1, 0), fixed4(tex2D(_LM, ClampShadowUVs(lmspace1)).r, tex2D(_LM, ClampShadowUVs(lmspace2)).g, tex2D(_LM, ClampShadowUVs(lmspace3)).b, 0));
#endif	
	
	return GetShadowAdjustedColor(saturate(shadowColor));
}


inline fixed4 ApplyShadowMap4Channels(fixed4 tex, half2 lmspace1, half2 lmspace2, half2 lmspace3, half2 lmspace4)
{
	// D3D doesn't like those three tex2Ds thrown into the dot
	// Seems to be a 3.5 issue
#if SHADER_API_D3D9
	fixed shadowColor = tex2D(_LM, ClampShadowUVs(lmspace1)).r + tex2D(_LM, ClampShadowUVs(lmspace2)).g + tex2D(_LM, ClampShadowUVs(lmspace3)).b + tex2D(_LM, ClampShadowUVs(lmspace4)).a;
#else
	fixed shadowColor = dot(fixed4(1, 1, 1, 1), fixed4(tex2D(_LM, ClampShadowUVs(lmspace1)).r, tex2D(_LM, ClampShadowUVs(lmspace2)).g, tex2D(_LM, ClampShadowUVs(lmspace3)).b, tex2D(_LM, ClampShadowUVs(lmspace4)).a));
#endif
	
	return BurnShadowColor(tex, saturate(shadowColor));
}


inline fixed ApplyShadowMap4ChannelsNoTex(half2 lmspace1, half2 lmspace2, half2 lmspace3, half2 lmspace4)
{
	// D3D doesn't like those three tex2Ds thrown into the dot
	// Seems to be a 3.5 issue
#if SHADER_API_D3D9
	fixed shadowColor = tex2D(_LM, ClampShadowUVs(lmspace1)).r + tex2D(_LM, ClampShadowUVs(lmspace2)).g + tex2D(_LM, ClampShadowUVs(lmspace3)).b + tex2D(_LM, ClampShadowUVs(lmspace4)).a;
#else
	fixed shadowColor = dot(fixed4(1, 1, 1, 1), fixed4(tex2D(_LM, ClampShadowUVs(lmspace1)).r, tex2D(_LM, ClampShadowUVs(lmspace2)).g, tex2D(_LM, ClampShadowUVs(lmspace3)).b, tex2D(_LM, ClampShadowUVs(lmspace4)).a));
#endif
	
	return GetShadowAdjustedColor(saturate(shadowColor));
}


inline half2 UpdateOneChannelScroll(half2 uvs, half2 tiling, half2 direction)
{
	return (uvs * tiling) + frac(direction * _ElapsedTime.x);
}


inline half4 UpdateTwoChannelScroll(half2 uvs, half4 tiling, half4 direction)
{
	///NOTE: Performing texture reads from .zw causes Dependent Reads and can be quite slow on Mobile...
	return (uvs.xyxy * tiling) + frac(direction * _ElapsedTime.x);
}


inline fixed3 UnpackTwoNormalDistortion(sampler2D normalMap, half4 uvs, half2 distortion)
{	
	///NOTE: Performing texture reads from .zw causes Dependent Reads and can be quite slow on Mobile...
	fixed3 nrml = UnpackNormal(tex2D(normalMap, uvs.xy)) + UnpackNormal(tex2D(normalMap, uvs.zw));
	nrml.xy *= distortion;
	return nrml;
}


inline fixed3 SampleReflection(sampler2D reflectionTex, half4 screen, fixed3 norm)
{
	half2 reflUV = (screen.xy / screen.w) + norm.xy;
#ifdef REFLECTION_SAMPLE_CLAMPING
	reflUV = clamp(reflUV, _ReflectionUVMinMax.xy, _ReflectionUVMinMax.zw);
#endif
	return tex2D(reflectionTex, reflUV).rgb;
}


#endif