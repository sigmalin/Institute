#ifndef _MY_SHADOW_H
#define _MY_SHADOW_H

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

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

CBUFFER_START(_CustomShadows)
	float _ShadowBias;
CBUFFER_END

struct appdata
{
	float4 vertex : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	float4 clipPos : SV_POSITION;
};

v2f vertShadowCaster(appdata v)
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);

	float4 worldPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1));
	o.clipPos = mul(unity_MatrixVP, worldPos);
#if UNITY_REVERSED_Z
	o.clipPos.z -= _ShadowBias;
	o.clipPos.z =
			min(o.clipPos.z, o.clipPos.w * UNITY_NEAR_CLIP_VALUE);
#else
	o.clipPos.z += _ShadowBias;
	o.clipPos.z =
			max(o.clipPos.z, o.clipPos.w * UNITY_NEAR_CLIP_VALUE);
#endif
	return o;
}

void fragShadowCaster(v2f i)
{
}

#endif // _MY_SHADOW_H