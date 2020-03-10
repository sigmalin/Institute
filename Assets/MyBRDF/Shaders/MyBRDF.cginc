#ifndef __MyBRDF_
#define __MyBRDF_

#ifndef PI
#define PI 3.1415926536898
#endif

float D_GGX(float NdotH, float roughness)
{
	float a = roughness;
	float a2 = a * a;
	float Distribution = NdotH*NdotH * (a2-1.0) + 1.0;

	return a2 / (PI * Distribution * Distribution);
}

float V_GGX(float NdotL, float NdotV, float roughness)
{
	float a = roughness;
	float a2 = a * a;
	float oneMinusA2 = 1 - a2;

	float NdotL2 = NdotL * NdotL;
	float NdotV2 = NdotV * NdotV;

	float lambdaL = NdotV * sqrt(NdotL2 * oneMinusA2 + a2);
	float lambdaV = NdotL * sqrt(NdotV2 * oneMinusA2 + a2);
	return 0.5 / (lambdaL + lambdaV);
}

float Schlick(float _value)
{
	float p = clamp(1 - _value, 0, 1);
	float p2 = p * p;
	float p5 = p2 * p2 * p;

	return p5;
}

float3 FresnelSchlick(float3 F0, float3 F90, float theta)
{
	return F0 + (F90 - F0) * Schlick(theta);
}

float3 Lambert(float3 col)
{
	return col / PI;
}

struct BRDF_Data 
{
	float3 albedo;
	float3 F0;
	float3 F90;
	float NdotL;
	float NdotV;
	float LdotH;
	float NdotH;
	float alphaRoughness;
};

float3 BRDF(BRDF_Data brdf)
{	
	if (brdf.NdotL <= 0) return 0;

	float Ds = D_GGX(brdf.NdotH, brdf.alphaRoughness);
	float Vs = V_GGX(brdf.NdotL, brdf.NdotV, brdf.alphaRoughness);				
	float3 Fs = FresnelSchlick(brdf.F0, brdf.F90, brdf.LdotH);
	
	float3 specular = Ds * Vs * Fs;
	float3 diffuse = (1 - Fs) * Lambert(brdf.albedo);

	return (diffuse + specular) * brdf.NdotL;
}

#endif // __MyBRDF_
