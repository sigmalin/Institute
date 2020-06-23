#ifndef _MY_DEPTH_ONLY_H
#define _MY_DEPTH_ONLY_H

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

struct appdata
{
	float4 vertex : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	float4 clipPos : SV_POSITION;
};

v2f vertDepthOnly(appdata v)
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);

	float4 worldPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1));
	o.clipPos = mul(unity_MatrixVP, worldPos);
	return o;
}

void fragDepthOnly(v2f i)
{
}

#endif // _MY_SHADOW_H