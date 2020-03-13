// https://www.shadertoy.com/view/3dVXDc
#ifndef __TilingPerlinWorleyNoise_
#define __TilingPerlinWorleyNoise_

#include "TilingNoiseCommon.cginc"
#include "TilingPerlinNoise.cginc"
#include "TiliedCellularNoise.cginc"

float remap(float x, float a, float b, float c, float d)
{
    return (((x - a) / (b - a)) * (d - c)) + c;
}

float PerlinFbm(float3 st, float period)
{
    if(Octaves == 0) return 0.0;

	float final = 0.0;
	float amplitude = 1.0;
	float maxAmplitude = 0.0;

	for (int i = 0; i < Octaves; ++i)
	{
		float v = (TiliedPerlinNoise3D(st, period) + 1.0) * 0.5;
		final += v * amplitude;
		maxAmplitude += amplitude;
		period *= Scale;		
		amplitude *= Persistence;
	}

	return final / maxAmplitude;
}

float WorleyFbm(float3 st, float period)
{
    if(Octaves == 0) return 0.0;

	float final = 0.0;
	float amplitude = 1.0;
	float maxAmplitude = 0.0;

	for (int i = 0; i < Octaves; ++i)
	{
		float v = 1.0 - TiliedCellularNoise3D(st, period);
		final += v * amplitude;
		maxAmplitude += amplitude;
		period *= Scale;		
		amplitude *= Persistence;
	}

	return final / maxAmplitude;
}

float TilingPerlinWorley(float3 st, float period)
{	
	float pFbm = PerlinFbm(st, period);
	pFbm = abs(pFbm * 2.0 - 1.0);

	float wFbm = WorleyFbm(st, period);

	return remap(pFbm, 0.0, 1.0, wFbm, 1.0);
}

#endif // __TilingPerlinWorleyNoise_
