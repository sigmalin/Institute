#ifndef __MyIBL_
#define __MyIBL_

#include "ToneMapping.cginc"

#ifndef PI
#define PI 3.1415926536898
#endif

samplerCUBE _IrradianceD;
sampler2D _IntegrateBRDF;

UNITY_DECLARE_TEXCUBE(_PrefiliterEnv);
#ifndef UNITY_SPECCUBE_LOD_STEPS
	#define UNITY_SPECCUBE_LOD_STEPS (5)
#endif

struct IBL_Data
{
	float3 albedo;
	float3 F0;
	float3 normalDirection;
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

	float3 irradiance = texCUBE (_IrradianceD, ibl.normalDirection).rgb;
	irradiance = Inv_Reinhard_tone_mapping(irradiance);

	return kD * irradiance * ibl.albedo;
}

float3 SpecularIBL(IBL_Data ibl)
{
	float3 PrefilteredColor = UNITY_SAMPLE_TEXCUBE_LOD(_PrefiliterEnv, ibl.normalDirection, ibl.alphaRoughness * UNITY_SPECCUBE_LOD_STEPS).rgb;
	PrefilteredColor = Inv_Reinhard_tone_mapping(PrefilteredColor);

	float2 EnvBRDF = tex2Dlod(_IntegrateBRDF, float4(ibl.NdotV, ibl.alphaRoughness, 0.0, 0.0)).rg;
	return PrefilteredColor * (ibl.F0 * EnvBRDF.x + EnvBRDF.y);
}

float3 IBL(IBL_Data ibl)
{
	return DiffuseIrradiance(ibl) + SpecularIBL(ibl);
}



#endif // __MyIBL_
