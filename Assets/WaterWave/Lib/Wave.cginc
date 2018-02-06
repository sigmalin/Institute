#ifndef _WAVE_
#define _WAVE_

float WaveHeight(sampler2D waveTex, float2 uv)
{
#if SHADER_TARGET < 30
	return tex2D(waveTex, uv).r * 2 - 1;
#else
	return tex2Dlod(waveTex, float4(uv, 0, 0)).r * 2 - 1;
#endif
}

float3 WaveNormal(sampler2D waveTex, float2 uv, float2 texelSize)
{
	float2 shiftX = { texelSize.x,  0 };
	float2 shiftZ = { 0, texelSize.y };
	float3 texX = WaveHeight(waveTex, uv.xy + shiftX);
	float3 texx = WaveHeight(waveTex, uv.xy - shiftX);
	float3 texZ = WaveHeight(waveTex, uv.xy + shiftZ);
	float3 texz = WaveHeight(waveTex, uv.xy - shiftZ);
	float3 du = { 1, 0, (texX.x - texx.x) };
	float3 dv = { 0, 1, (texZ.x - texz.x) };
	//return normalize(cross(du, dv)) * 0.5 + 0.5;
	return normalize(cross(du, dv));
}

#endif