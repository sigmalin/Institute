#ifndef _MY_UNLIT_H
#define _MY_UNLIT_H

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

#ifndef UNITY_MATRIX_M
#define UNITY_MATRIX_M unity_ObjectToWorld
#endif // UNITY_MATRIX_M

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

CBUFFER_START(UnityPerFrame)
	float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
	float4x4 unity_WorldToObject;
	float4 unity_LODFade;
	real4 unity_WorldTransformParams;
CBUFFER_END

CBUFFER_START(_LightBuffer)
	float4 _LightColor;
	float4 _LightDirection;
CBUFFER_END

CBUFFER_START(_CameraBuffer)
	float4 _WorldSpaceCameraPos;
CBUFFER_END


TEXTURE2D_SHADOW(_ShadowMap);
SAMPLER_CMP(sampler_linear_clamp_compare);

CBUFFER_START(_CustomShadows)
	float4x4 _ShadowMatrixVP;
	float _ShadowStrength;
	float4 _ShadowMapSize;
CBUFFER_END

float ShadowAttenuation (float3 worldPos) 
{
	float4 shadowProj = mul(_ShadowMatrixVP, float4(worldPos,1));
	shadowProj.xyz /= shadowProj.w;

	float attenuation = 0;

#if defined(_SHADOWS_SOFT)
	float tentWeights[9];
	float2 tentUVs[9];
	SampleShadow_ComputeSamples_Tent_5x5(
		_ShadowMapSize, shadowProj.xy, tentWeights, tentUVs
	);
		
	for (int i = 0; i < 9; i++) 
	{
		attenuation += tentWeights[i] * SAMPLE_TEXTURE2D_SHADOW(
				_ShadowMap, sampler_linear_clamp_compare, float3(tentUVs[i].xy, shadowProj.z)
		);
	}
#else

	attenuation = SAMPLE_TEXTURE2D_SHADOW(_ShadowMap, sampler_linear_clamp_compare, shadowProj.xyz);
	
#endif

	return lerp(1, attenuation, _ShadowStrength);
}

#endif // _MY_UNLIT_H