#ifndef __MyPBR_
#define __MyPBR_

#include "MyBRDF.hlsl"

#ifdef _DUAL_PARABOLOID_
#include "MyIBL_DualParaboloid.hlsl"
#else
#include "MyIBL.hlsl"
#endif

#define AlbedoAndFresnelFromMetallic(baseColor, metallic, Albedo, F0, F90)\
	float3 Albedo = baseColor  * (1.0 - 0.04) * (1.0 - metallic);\
	float3 F0 = lerp(0.04, baseColor, metallic);\
	float reflectance = max(max(F0.r, F0.g), F0.b);\
	float3 F90 = clamp(reflectance*25.0, 0.0, 1.0); // == reflectance / 0.04


#define AlbedoAndFresnelFromIOR(baseColor, ior1, ior2, Albedo, F0)\
	float reflectance = sqr(ior1-ior2) / sqr(ior1+ior2);\
	float3 Albedo = baseColor  * (1.0 - reflectance);\
	float3 F0 = baseColor * reflectance;\
	


#endif // __MyPBR_
