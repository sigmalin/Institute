#ifndef __RAY_MARCHING_
#define __RAY_MARCHING_
//https://wgld.org/
//http://iquilezles.org/www/articles/distfunctions/distfunctions.htm

float Radians(float angle)
{
	return UNITY_PI * angle / 180.0;
}

float3 mod(float3 a, float3 b)   
{   
	return a - b*floor(a / b);
}

float3 rotate(float3 p, float angle, float3 axis)
{
	float3 a = normalize(axis);
	float s = sin(angle);
	float c = cos(angle);
	float r = 1 - c;

	float3x3 m = float3x3(
		a.x * a.x * r + c,
		a.y * a.x * r + a.z * s,
		a.z * a.x * r - a.y * s,

		a.x * a.y * r - a.z * s,
		a.y * a.y * r + c,
		a.z * a.y * r + a.x * s,

		a.x * a.z * r + a.y * s,
		a.y * a.z * r - a.x * s,
		a.z * a.z * r + c
	);

	return mul(m, p);
}

float3 twist_X( float3 p, float power )
{
	float s = sin(power * p.x);
	float c = cos(power * p.x);

	float3x3 m = float3x3(
		1.0, 0.0, 0.0,
		0.0,   c,   s,
		0.0,  -s,   c
	);

	return mul(m, p);
}

float3 twist_Y( float3 p, float power )
{
	float s = sin(power * p.y);
	float c = cos(power * p.y);

	float3x3 m = float3x3(
		  c, 0.0,  -s,
		0.0, 1.0, 0.0,
		  s, 0.0,   c
	);

	return mul(m, p);
}

float3 twist_Z( float3 p, float power )
{
	float s = sin(power * p.z);
	float c = cos(power * p.z);

	float3x3 m = float3x3(
		  c,  -s, 0.0,
		 -s,   c, 0.0,
		0.0, 0.0, 1.0
	);

	return mul(m, p);
}

float smoothMin(float d1, float d2, float k){
    //float h = exp(-k * d1) + exp(-k * d2);
    //return -log(h) / k;

	float h = max(k - abs(d1-d2), 0.0) / k;
	return min(d1, d2) - h*h*h*k/6.0;
}

float sdSphere( float3 p, float s )
{
	return length(p)-s;
}

float sdBox( float3 p, float3 b )
{
  float3 q = abs(p) - b;
  return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
}

float sdTorus_XZ( float3 p, float2 t )
{
	float2 q = float2(length(p.xz)-t.x,p.y);
	return length(q)-t.y;
}

float sdTorus_XY( float3 p, float2 t )
{
	float2 q = float2(length(p.xy)-t.x,p.z);
	return length(q)-t.y;
}

float sdFloor( float3 p )
{
	return dot(p, float3(0,1,0)) + 1;
}

float sdCylinder( float3 p, float2 r )
{
	float2 d = abs(float2(length(p.xy), p.z)) - r;
	return min(max(d.x, d.y), 0) + length(max(d, 0));
}

#endif // __RAY_MARCHING_
