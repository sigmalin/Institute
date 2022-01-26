#ifndef _TERRAIN_NOISE_H
#define _TERRAIN_NOISE_H

#pragma multi_compile_local __ IQ Billowed Ridged Swiss Jordan

#include "PerlinNoise.hlsl"

float TurbulenceSeed;
int TurbulenceOctaves;
float TurbulenceAmplitude;
float TurbulenceFrequence;
float TurbulenceLacunarity;
float TurbulenceGain;

#if defined(IQ)

#define NOISE(X) iqTurbulence(X, TurbulenceSeed, TurbulenceOctaves, TurbulenceAmplitude, TurbulenceFrequence, TurbulenceLacunarity, TurbulenceGain)

#elif defined(Billowed)

#define NOISE(X) billowedTurbulence(X, TurbulenceSeed, TurbulenceOctaves, TurbulenceAmplitude, TurbulenceFrequence, TurbulenceLacunarity, TurbulenceGain)

#elif defined(Ridged)

#define NOISE(X) RidgedTurbulence(X, TurbulenceSeed, TurbulenceOctaves, TurbulenceAmplitude, TurbulenceFrequence, TurbulenceLacunarity, TurbulenceGain)

#elif defined(Swiss)

#define NOISE(X) swissTurbulence(X, TurbulenceSeed, TurbulenceOctaves, TurbulenceAmplitude, TurbulenceFrequence, TurbulenceLacunarity, TurbulenceGain)

#elif defined(Jordan)

#define NOISE(X) jordanTurbulence(X, TurbulenceSeed, TurbulenceOctaves, TurbulenceAmplitude, TurbulenceFrequence, TurbulenceLacunarity, TurbulenceGain, 0.2, 0.35, 1.0, 0.05, 1)

#else

#define NOISE(X) turbulence(X, TurbulenceSeed, TurbulenceOctaves, TurbulenceAmplitude, TurbulenceFrequence, TurbulenceLacunarity, TurbulenceGain)

#endif

//#define NOISE(X) turbulence(X, 32, 8, 500, 0.0005, 2, 0.5)
//#define NOISE(X) iqTurbulence(X, 32, 12, 1, 0.05, 2, 0.5)
//#define NOISE(X) billowedTurbulence(X, 32, 8, 5, 0.01, 2, 0.5)
//#define NOISE(X) RidgedTurbulence(X, 32, 8, 50, 0.004, 2, 0.5)
//#define NOISE(X) swissTurbulence(X, 32, 8, 500, 0.0005, 2, 0.5)
//#define NOISE(X) jordanTurbulence(X, 32, 12, 4, 0.05, 2, 0.00625, 0.2, 0.35, 1.0, 0.05, 1)

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
