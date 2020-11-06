#include "SphericalGaussSpecular.cginc"

#ifndef _ANISOTROPIC_SPHERICAL_GAUSS_SPECULAR_H
#define _ANISOTROPIC_SPHERICAL_GAUSS_SPECULAR_H

ASG WarpDistributionASG(in SG _ndf, in float3 _view)
{
	ASG warp;

	warp.BasisZ = reflect(-_view, _ndf.Axis);
	warp.BasisX = normalize(cross(_ndf.Axis, warp.BasisZ));
	warp.BasisY = normalize(cross(warp.BasisZ, warp.BasisX));

	float VdotL = max(dot(_view, _ndf.Axis), 0.1);

	warp.SharpnessX = _ndf.Sharpness / (8 * VdotL * VdotL);
	warp.SharpnessY = _ndf.Sharpness / 8;

	warp.Amplitude = _ndf.Amplitude;

	return warp;
}

float3 FresnelTerm(in float3 _specular, in ASG _warpD, in float3 _view)
{
	float3 h = normalize(_warpD.BasisZ + _view);
	float LdotH = saturate(dot(_warpD.BasisZ, h));

	return _specular + (1 - _specular) * Schlick(LdotH);
}

float3 ASGSpecular(in SG _lightingLobe, in float3 _normal, in float3 _view, in float3 _specular, in float _roughness)
{
	// Create an SG that approximates the NDF. Note that a single SG lobe is a poor fit for
    // the GGX NDF, since the GGX distribution has a longer tail. A sum of 3 SG's can more
    // closely match the shape of a GGX distribution, but it would also increase the cost
    // computing specular by a factor of 3.
	SG ndf = DistributionTerm(_normal, _roughness);

	ASG warpNDF = WarpDistributionASG(ndf, _view);

	float NdotL = saturate(dot(_normal, warpNDF.BasisZ));
	float NdotV = saturate(dot(_normal, _view));

	float3 Ds_Integral = ConvolveASG_SG(warpNDF, _lightingLobe);
	float Vs = VisibilityTerm(NdotL, NdotV, _roughness);
	float3 Fs = FresnelTerm(_specular, warpNDF, _view);

	// Fade out spec entirely when lower than 0.1% albedo
    float3 FadeOut = saturate(dot(_specular, 333.0f));

	return max(Ds_Integral * Fs * Vs * FadeOut * NdotL, 0);
}

#endif // _ANISOTROPIC_SPHERICAL_GAUSS_DIFFUSE_H