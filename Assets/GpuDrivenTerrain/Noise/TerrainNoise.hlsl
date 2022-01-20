#ifndef _TERRAIN_NOISE_H
#define _TERRAIN_NOISE_H

//#include "..\OpenSimplexNoise\OpenSimplexNoise.hlsl"
//#include "..\VoronoiNoise\VoronoiNoise.hlsl"
#include "..\PerlinNoise\PerlinNoise.hlsl"


#define NOISE(X) iqTurbulence(X, 0, 8, 4)

float BillowedNoise(float2 pos)
{
	return abs(NOISE(pos));
}

float RidgedNoise(float2 p)
{
	return 1.0f - abs(NOISE(p));
}


float getHeight(float2 pos)
{
	//return VoronoiFbm(pos) * 200;
	//return SimplexFbm(pos) * 200;
	return (NOISE(pos / 1000).x + 1;
}

float3 getNormal(float2 pos, float h)
{
	const float ep = 0.01;
	float diffX = getHeight(pos + float2(ep, 0)) - h;
	float diffZ = getHeight(pos + float2(0, ep)) - h;
	return normalize(float3(diffX / ep, 1, diffZ / ep));
}

#endif
