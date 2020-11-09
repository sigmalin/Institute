#include "AnisotropicSphericalGauss.cginc"
#include "AnisotropicSphericalGaussSpecular.cginc"

#ifndef _ASHIKHMIN_H
#define _ASHIKHMIN_H

ASG WarpDistributionASG(in ASG _ndf, in float3 _view)
{
	ASG warp;

	warp.BasisZ = reflect(-_view, _ndf.BasisZ);

	float3 Xi = normalize(cross(warp.BasisZ, _view));
	float3 Yi = normalize(cross(Xi, warp.BasisZ));

	float IdotXi = dot(warp.BasisZ, Xi);
	float IdotYi = dot(warp.BasisZ, Yi);

	float VdotZ  = max(dot(_view, _ndf.BasisZ), 0.1);
	float XdotXi = dot(_ndf.BasisX, Xi);
	float YdotXi = dot(_ndf.BasisY, Xi);

	float U00 = -XdotXi / (2 * VdotZ);
	float U01 = YdotXi / (2 * VdotZ);
	float U10 = YdotXi / 2;
	float U11 = XdotXi / 2;

	warp.BasisX = normalize(U00 * Xi + U01 * Yi);
	warp.BasisY = normalize(U10 * Xi + U11 * Yi);

	warp.SharpnessX = _ndf.SharpnessX;
	warp.SharpnessY = _ndf.SharpnessY;

	warp.Amplitude = _ndf.Amplitude;

	return warp;
}

ASG AshikhminDterm(in float3 _axisZ, in float3 _axisX, in float3 _axisY, in float _lambda, in float _mu)
{
	ASG ndf;
	ndf.BasisZ = _axisZ;
	ndf.BasisX = _axisX;
	ndf.BasisY = _axisY;
	ndf.SharpnessX = _lambda * 0.5;
	ndf.SharpnessY = _mu * 0.5;
	ndf.Amplitude = 1;

	return ndf;
}

float AshikhminMterm(in float _lambda, in float _mu, in float _LdotH, in float _NdotL, in float _NdotV)
{
	float term1 = sqrt((_lambda + 1) * (_mu + 1)) / (8 * PI);
	float term2 = _LdotH * max(_NdotL, _NdotV);
	return term1 / term2;
}

float3 AshikhminSpecular(in SG _lightingLobe, in float3 _normal, in float3 _tangent, in float3 _bitangent, in float3 _view, in float3 _specular, in float _lambda, in float _mu)
{
	ASG ndf = AshikhminDterm(_normal, _tangent, _bitangent, _lambda, _mu);

	ASG warpNDF = WarpDistributionASG(ndf, _view);

	float NdotL = saturate(dot(_normal, warpNDF.BasisZ));
	float NdotV = max(0.1, dot(_normal, _view));

	float3 h = normalize(warpNDF.BasisZ + _view);
	float LdotH = max(0.1, dot(warpNDF.BasisZ, h));

	float3 Ds_Integral = EvaluateASG(warpNDF, _lightingLobe.Axis);

	float Ms = AshikhminMterm(_lambda, _mu, LdotH, NdotL, NdotV);
	float3 Fs = _specular + (1 - _specular) * Schlick(LdotH);
	
	return max(Ds_Integral * Fs * Ms * NdotL, 0);
}

float3 AshikhminSpecularWithIndirect(in SG _lightingLobe, in float3 _normal, in float3 _tangent, in float3 _bitangent, in float3 _view, in float3 _specular, in float _lambda, in float _mu)
{
	ASG ndf = AshikhminDterm(_normal, _tangent, _bitangent, _lambda, _mu);

	ASG warpNDF = WarpDistributionASG(ndf, _view);

	float NdotL = saturate(dot(_normal, warpNDF.BasisZ));
	float NdotV = max(0.1, dot(_normal, _view));

	float3 h = normalize(warpNDF.BasisZ + _view);
	float LdotH = max(0.1, dot(warpNDF.BasisZ, h));

	float3 Ds_Integral = ConvolveASG_SG(warpNDF, _lightingLobe);

	float Ms = AshikhminMterm(_lambda, _mu, LdotH, NdotL, NdotV);
	float3 Fs = _specular + (1 - _specular) * Schlick(LdotH);
	
	return max(Ds_Integral * Fs * Ms * NdotL, 0);
}

#endif // _ASHIKHMIN_H