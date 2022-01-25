#ifndef _TERRAIN_NOISE_H
#define _TERRAIN_NOISE_H

//#include "..\OpenSimplexNoise\OpenSimplexNoise.hlsl"
//#include "..\VoronoiNoise\VoronoiNoise.hlsl"
#include "PerlinNoise.hlsl"


//#define NOISE(X) turbulence(X, 32, 8, 500, 0.0005, 2, 0.5)
//#define NOISE(X) iqTurbulence(X, 32, 12, 500, 0.0008, 2, 0.5)
//#define NOISE(X) billowedTurbulence(X, 32, 8, 500, 0.0005, 2, 0.5)
#define NOISE(X) RidgedTurbulence(X, 32, 12, 500, 0.0004, 2, 0.5)

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
	return NOISE(pos);
}

float3 getNormal(float2 pos, float h)
{
	const float ep = 0.01;
	float diffX = getHeight(pos + float2(ep, 0)) - h;
	float diffZ = getHeight(pos + float2(0, ep)) - h;
	return normalize(float3(diffX / ep, 1, diffZ / ep));
}

#endif
