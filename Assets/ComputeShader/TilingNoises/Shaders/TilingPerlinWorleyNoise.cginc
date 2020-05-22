// https://www.shadertoy.com/view/3dVXDc
#ifndef __TilingPerlinWorleyNoise_
#define __TilingPerlinWorleyNoise_

#include "TilingNoiseCommon.cginc"
#include "TilingPerlinNoise.cginc"
#include "TiliedCellularNoise.cginc"

float remap(float originalValue, float originalMin, float originalMax, float newMin, float newMax)
{
	return newMin + (((originalValue - originalMin) / (originalMax - originalMin)) * (newMax - newMin));
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

	float wFbm = WorleyFbm(st, period);

	// Perlin Worley is based on description in GPU Pro 7: Real Time Volumetric Cloudscapes.
	// However it is not clear the text and the image are matching: images does not seem to match what the result  from the description in text would give.
	// Also there are a lot of fudge factor in the code, e.g. *0.2, so it is really up to you to fine the formula you like.

	//return remap(wFbm, 0.0, 1.0, 0.0, pFbm); // Matches better what figure 4.7 (not the following up text description p.101). Maps worley between newMin as 0 and 
	//return remap(pFbm, 0.0, 1.0, wFbm, 1.0);   // mapping perlin noise in between worley as minimum and 1.0 as maximum (as described in text of p.101 of GPU Pro 7) 
	return remap(pFbm, wFbm, 1.0, 0.5, 1.0);
}

#endif // __TilingPerlinWorleyNoise_
