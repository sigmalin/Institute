#include "SphericalGauss.cginc"

#ifndef _ANISOTROPIC_SPHERICAL_GAUSS_H
#define _ANISOTROPIC_SPHERICAL_GAUSS_H

// AnisotropicSphericalGaussian(dir) :=
// Amplitude * exp(-SharpnessX * dot(BasisX, dir)^2 - SharpnessY * dot(BasisY, dir)^2)
struct ASG
{
	float3 Amplitude;
	float3 BasisZ;
	float3 BasisX;
	float3 BasisY;
	float SharpnessX;
	float SharpnessY;
};

float3 EvaluateASG(in ASG _asg, in float3 _direction)
{
	float sTerm = saturate(dot(_asg.BasisZ, _direction));

	float XdotL = dot(_direction, _asg.BasisX);
	float lambdaTerm = _asg.SharpnessX * XdotL * XdotL;

	float YdotL = dot(_direction, _asg.BasisY);
	float muTerm = _asg.SharpnessY * YdotL * YdotL;

	return _asg.Amplitude * sTerm * exp(-lambdaTerm-muTerm);
}

float3 ConvolveASG_SG(in ASG _asg, in SG _sg)
{
	// The ASG paper specifes an isotropic SG as exp(2 * nu * (dot(v, axis) - 1)),
    // so we must divide our SG sharpness by 2 in order to get the nup parameter expected by
    // the ASG formulas
	float nu = _sg.Sharpness * 0.5;

	ASG convolveASG;
	convolveASG.BasisX = _asg.BasisX;
	convolveASG.BasisY = _asg.BasisY;
	convolveASG.BasisZ = _asg.BasisZ;

	convolveASG.SharpnessX = (nu * _asg.SharpnessX) / (nu + _asg.SharpnessX);
	convolveASG.SharpnessY = (nu * _asg.SharpnessY) / (nu + _asg.SharpnessY);

	convolveASG.Amplitude = PI / sqrt((nu + _asg.SharpnessX) * (nu + _asg.SharpnessY));

	float3 asgResult = EvaluateASG(convolveASG, _sg.Axis);
	return asgResult * _sg.Amplitude * _asg.Amplitude;
}

#endif // _ANISOTROPIC_SPHERICAL_GAUSS_H