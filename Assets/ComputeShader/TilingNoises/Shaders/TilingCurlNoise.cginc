#ifndef __TilingCurlNoise_
#define __TilingCurlNoise_

#include "TilingNoiseCommon.cginc"
#include "TilingPerlinNoise.cginc"

float LayeredPerlinNoise(float3 st, float period)
{
	if(Octaves == 0) return 0.0;

	float final = 0.0;
	float amplitude = 1.0;
	float maxAmplitude = 0.0;

	for (int i = 0; i < Octaves; ++i)
	{
		final += TiliedPerlinNoise3D(st, period) * amplitude;
		maxAmplitude += amplitude;
		period *= Scale;		
		amplitude *= Persistence;
	}

	return final / maxAmplitude;
}

float3 PerlinNoiseVec3(float3 st, float period)
{
	float x = LayeredPerlinNoise(st, period);

	float y = LayeredPerlinNoise(st.yzx + float3(31.416, -47.853, 12.793), period);
	
	float z = LayeredPerlinNoise(st.zxy + float3(-233.145, -113.408, -185.31), period);

	return float3(x,y,z);
}

float3 TilingCurlNoise(float3 st, float period)
{
	const float e = 0.0009765625;
	const float e2 = 2.0 * e;
	const float invE2 = 1.0 / e2;

	const float3 dx = float3(e, 0.0, 0.0);
	const float3 dy = float3(0.0, e, 0.0);
	const float3 dz = float3(0.0, 0.0, e);

	float3 p_x0 = PerlinNoiseVec3(st - dx, period);
	float3 p_x1 = PerlinNoiseVec3(st + dx, period);
	float3 p_y0 = PerlinNoiseVec3(st - dy, period);
	float3 p_y1 = PerlinNoiseVec3(st + dy, period);
	float3 p_z0 = PerlinNoiseVec3(st - dz, period);
	float3 p_z1 = PerlinNoiseVec3(st + dz, period);

	float x = (p_y1.z - p_y0.z) - (p_z1.y - p_z0.y);
	float y = (p_z1.x - p_z0.x) - (p_x1.z - p_x0.z);
	float z = (p_x1.y - p_x0.y) - (p_y1.x - p_y0.x);

	return float3(x, y, z) * invE2;
}


#endif // __TilingCurlNoise_
