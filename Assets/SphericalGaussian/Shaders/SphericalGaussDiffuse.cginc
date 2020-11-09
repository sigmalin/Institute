#include "SphericalGauss.cginc"

#ifndef _SPHERICAL_GAUSS_DIFFUSE_H
#define _SPHERICAL_GAUSS_DIFFUSE_H

float3 SGIrradianceInnerProduct(in SG _lightingLobe, in float3 _normal)
{
	SG cosineLobe = CosineLobeSG(_normal);
	return max(SGInnerProduct(_lightingLobe, cosineLobe), 0);
}

float3 SGDiffuseInnerProduct(in SG _lightingLobe, in float3 _normal, in float3 _albedo)
{
	float3 brdf = _albedo / PI;
	return SGIrradianceInnerProduct(_lightingLobe, _normal) * brdf;
}

//A CHEAPER APPROXIMATION
float3 SGIrradiancePunctual(in SG _lightingLobe, in float3 _normal)
{
	float cosineTerm = saturate(dot(_lightingLobe.Axis, _normal));
	return cosineTerm * ApproximateSGIntegral(_lightingLobe);
}

float3 SGDiffusePunctual(in SG _lightingLobe, in float3 _normal, in float3 _albedo)
{
	float3 brdf = _albedo / PI;
	return SGIrradiancePunctual(_lightingLobe, _normal) * brdf;
}

//Stephen Hill's APPROXIMATION Cosine Lobe
//Assumes lightingLobe is normalized
float ApproximateCosineLobe(in SG _lightingLobe, in float3 _normal)
{
    const float muDotN = dot(_lightingLobe.Axis, _normal);
    const float lambda = _lightingLobe.Sharpness;

    const float c0 = 0.36f;
    const float c1 = 1.0f / (4.0f * c0);

    float eml  = exp(-lambda);
    float em2l = eml * eml;
    float rl   = rcp(lambda);

    float scale = 1.0f + 2.0f * em2l - rl;
    float bias  = (eml - em2l) * rl - em2l;

    float x  = sqrt(1.0f - scale);
    float x0 = c0 * muDotN;
    float x1 = c1 * x;

    float n = x0 + x1;

    float y = (abs(x0) <= x1) ? n * n / x : saturate(muDotN);
 
    float result = scale * y + bias;
    return result;
}

float3 SGDiffuseFitted(in SG _lightingLobe, in float3 _normal, in float3 _albedo)
{
	float3 brdf = _albedo / PI;
	return ApproximateCosineLobe(_lightingLobe, _normal) * ApproximateSGIntegral(_lightingLobe) * brdf;
}

#endif // _SPHERICAL_GAUSS_DIFFUSE_H