#ifndef __MyIBL_DualParaboloid
#define __MyIBL_DualParaboloid

#include "ToneMapping.hlsl"
#include "DualParaboloid.hlsl"

#ifndef PI
#define PI 3.1415926536898
#endif

TEXTURE2D(_IrradianceD);
SAMPLER(sampler_IrradianceD);

TEXTURE2D(_IntegrateBRDF);
SAMPLER(sampler_IntegrateBRDF);

TEXTURE2D(_PrefiliterEnv);
SAMPLER(sampler_PrefiliterEnv);

struct IBL_Data
{
	float3 albedo;
	float3 F0;
	float3 normalDirection;
	float3 reflectDirection;
	float NdotV;
	float alphaRoughness;
	float metallic;
};

float3 DiffuseIrradiance(IBL_Data ibl)
{
	float3 F90 = max(1.0 - ibl.alphaRoughness, ibl.F0);
	float3 fresnel = FresnelSchlick(ibl.F0, F90, ibl.NdotV);

	float3 refract = 1.0 - fresnel;
	float3 kD = refract * (1.0 - ibl.metallic);

	float3 irradiance = SAMPLE_DUALPARBOLOID_LOD (_IrradianceD, sampler_IrradianceD, ibl.normalDirection, 0).rgb;
	irradiance = Inv_Reinhard_tone_mapping(irradiance);

	return kD * irradiance * ibl.albedo;
}

float3 SpecularIBL(IBL_Data ibl)
{
	float3 PrefilteredColor = SAMPLE_DUALPARBOLOID_LOD(_PrefiliterEnv, sampler_PrefiliterEnv, ibl.reflectDirection, ibl.alphaRoughness * 5).rgb;
	PrefilteredColor = Inv_Reinhard_tone_mapping(PrefilteredColor);

	float2 EnvBRDF = SAMPLE_TEXTURE2D_LOD(_IntegrateBRDF, sampler_IntegrateBRDF, float2(ibl.NdotV, ibl.alphaRoughness), 0).rg;
	return PrefilteredColor * (ibl.F0 * EnvBRDF.x + EnvBRDF.y);
}

float3 IBL(IBL_Data ibl)
{
	return DiffuseIrradiance(ibl) + SpecularIBL(ibl);
}



#endif // __MyIBL_DualParaboloid
