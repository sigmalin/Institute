#ifndef _PROJECTION_GRID 
#define _PROJECTION_GRID

float4x4 _Interpolation;

#define PROJECTION_TO_WORLD(uv) Projection2World(uv);

inline float4 Projection2World(float2 uv)
{
    //Interpolate between frustums world space projection points. p is in world space.
	float4 p = lerp(lerp(_Interpolation[0], _Interpolation[1], uv.x), lerp(_Interpolation[3], _Interpolation[2], uv.x), uv.y);
	p = p / p.w;
    return p;
}

#endif