#ifndef __Fresnel_
#define __Fresnel_
/// http://graphicrants.blogspot.com/2013/08/specular-brdf-reference.html
/// https://www.jordanstevenstechart.com/physically-based-rendering
/// Fresnel Function

float SchlickPow5(float _value)
{
	float p = clamp(1 - _value, 0, 1);
	float p2 = p * p;
	return p2 * p2 * _value;
}

float3 FresnelLerp(float3 x, float3 y, float d)
{
	float t = SchlickPow5(d);	
	return lerp (x, y, t);
}

// Fresnel Function

float3 FresnelSchlick(fixed3 _specular, float _cosThelta)
{
	return _specular + (1 - _specular) * SchlickPow5(_cosThelta);
}

float3 SphericalGaussian(fixed3 _specular, float _cosThelta)
{
	float p = ((-5.55473 * _cosThelta) - 6.98316) * _cosThelta;
	return _specular + (1 - _specular) * pow(2, p);
}

float3 UnrealFresnel(fixed3 _specular, float _cosThelta)
{
	float Fc = SchlickPow5(_cosThelta);

	return saturate(50 * _specular.g) * Fc + (1 - Fc) * _specular;
}

float3 FresnelIOR(float _ior, float _cosThelta)
{
	//float term = pow(_ior-1, 2) / pow(_ior+1, 2);
	float f0 = pow((_ior-1) / (_ior+1), 2);

	return f0 + (1 - f0) * SchlickPow5(_cosThelta);
}

// F0

float3 UnityApproximation(fixed3 _albedo, float _metalic, out float3 _diffuse)
{
#ifdef UNITY_COLORSPACE_GAMMA
	float oneMinusDielectricSpec = 1 - 0.220916301;
	_diffuse = _albedo * (oneMinusDielectricSpec - _metalic * oneMinusDielectricSpec);
	return lerp(float3(0.220916301, 0.220916301, 0.220916301), _albedo, _metalic);
#else
	float oneMinusDielectricSpec = 1 - 0.04;
	_diffuse = _albedo * (oneMinusDielectricSpec - _metalic * oneMinusDielectricSpec);
	return lerp(float3(0.04, 0.04, 0.04), _albedo, _metalic);
#endif
}

// KD

float3 NormalIncidenceReflection(float NdotV, float NdotL, float LdotH, float roughness)
{
	float fresnelLight = SchlickPow5(NdotL);
	float fresnelView = SchlickPow5(NdotV);
	float fresnelDiffuse90 = 0.5 + 2 * LdotH * LdotH * roughness;

	return lerp(1, fresnelDiffuse90, fresnelLight) * lerp(1, fresnelDiffuse90, fresnelView);
}

#endif // __Fresnel_
