#include "SphericalGauss.cginc"

#ifndef _SPHERICAL_GAUSS_SPECULAR_H
#define _SPHERICAL_GAUSS_SPECULAR_H

SG DistributionTerm(in float3 _direction, in float _roughness)
{
	SG distribution;

	distribution.Axis = _direction;
	float m2 = _roughness * _roughness;
	distribution.Sharpness = 2 / m2;
	distribution.Amplitude = 1 / (PI * m2);

	return distribution;
}

SG WarpDistributionSG(in SG _ndf, in float3 _view)
{
	SG warp;

	warp.Axis = reflect(-_view, _ndf.Axis);
	warp.Amplitude = _ndf.Amplitude;
	warp.Sharpness = _ndf.Sharpness / (4 * max(dot(_ndf.Axis, _view), 0.1));

	return warp;
}

float GGX_V1(in float _m2, in float _theta)
{
	return 1 / (_theta + sqrt(_m2 + (1 - _m2) * _theta * _theta));
}

float VisibilityTerm(in float3 _NdotL, in float3 _NdotV, in float _roughness)
{
	float m2 = _roughness * _roughness;
	
	return GGX_V1(m2, _NdotL) * GGX_V1(m2, _NdotV);	
}

float Schlick(float _value)
{
	float p = clamp(1 - _value, 0, 1);
	float p2 = p * p;
	float p5 = p2 * p2 * p;

	return p5;
}

float3 FresnelTerm(in float3 _specular, in SG _warpD, in float3 _view)
{
	float3 h = normalize(_warpD.Axis + _view);
	float LdotH = saturate(dot(_warpD.Axis, h));

	return _specular + (1 - _specular) * Schlick(LdotH);
}

float3 SGSpecular(in SG _lightingLobe, in float3 _normal, in float3 _view, in float3 _specular, in float _roughness)
{
	// Create an SG that approximates the NDF. Note that a single SG lobe is a poor fit for
    // the GGX NDF, since the GGX distribution has a longer tail. A sum of 3 SG's can more
    // closely match the shape of a GGX distribution, but it would also increase the cost
    // computing specular by a factor of 3.
	SG ndf = DistributionTerm(_normal, _roughness);

	SG warpNDF = WarpDistributionSG(ndf, _view);

	float NdotL = saturate(dot(_normal, warpNDF.Axis));
	float NdotV = saturate(dot(_normal, _view));

	float3 Ds_Integral = SGInnerProduct(warpNDF, _lightingLobe);
	float Vs = VisibilityTerm(NdotL, NdotV, _roughness);
	float3 Fs = FresnelTerm(_specular, warpNDF, _view);

	// Fade out spec entirely when lower than 0.1% albedo
    float3 FadeOut = saturate(dot(_specular, 333.0f));

	return max(Ds_Integral * Fs * Vs * FadeOut * NdotL, 0);
}

#endif // _SPHERICAL_GAUSS_DIFFUSE_H