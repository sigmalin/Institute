#ifndef __TilingNoiseCommon_
#define __TilingNoiseCommon_

float RandomSeed;

int Octaves;
float Persistence;
float Scale;

float Period;

int TexWidth;
int TexHeight;
int TexDepth;

float mod(float a, float b)
{
	return a - b * floor(a / b);
}

float modulo(float divident, float divisor)
{
	float positiveDivident = mod(divident, divisor) + divisor;
	return mod(positiveDivident, divisor);
}

int FloorToInt(float x)
{
	return (int)floor(x);
}


float2 random2(float2 p)
{
	float2 v = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
	v = sin(v) * RandomSeed;
	return frac(v);
}

float3 random3(float3 p)
{
	float3 v = float3(dot(p, float3(127.1, 311.7, 74.7)),
                      dot(p, float3(269.5, 183.3, 246.1)),
                      dot(p, float3(113.5, 271.9, 124.6)));

	v = sin(v) * RandomSeed;
	return frac(v);
}

#endif // __TilingNoiseCommon_
