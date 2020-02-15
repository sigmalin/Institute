#ifndef __HASH_
#define __HASH_
// https://shadertoy.fandom.com/wiki/Noise
// https://gist.github.com/patriciogonzalezvivo/670c22f3966e662d2f83

float hash11(float n)
{
	return frac(sin(n) * 114514.4545);
}

float hash12(float2 n)
{
	//return frac(sin(dot(n, float2(12.9898, 4.1414))) * 114514.4545);
	return frac(sin(dot(n ,float2(12.9898,78.233))) * 43758.5453);
}

float hash13(float3 n)
{
	//return frac(sin(dot(n, float3(12.9898, 4.1414, 19.19))) * 114514.4545);
	return frac(sin(dot(n ,float3(12.9898,78.233,128.852))) * 43758.5453)*2.0-1.0;
}

float2 hash22(float2 n)
{
	float2 p = float2(dot(n,float2(127.1,311.7)),
					 dot(n,float2(269.5,183.3)));

	return -1.0+2.0*frac(sin(p) * 114514.4545);
}

float2 hash23(float3 n)
{
	float2 p	=	float2( dot(n,float3(127.1,311.7, 74.7)),
						dot(n,float3(269.5,183.3,246.1)));

	return -1.0+2.0*frac(sin(p) * 114514.4545);
}

float3 hash33(float3 n)
{
	float3 p	=	float3( dot(n,float3(127.1,311.7, 74.7)),
						dot(n,float3(269.5,183.3,246.1)),
						dot(n,float3(113.5,271.9,124.6)));

	return -1.0+2.0*frac(sin(p) * 114514.4545);
}

#endif // __HASH_
