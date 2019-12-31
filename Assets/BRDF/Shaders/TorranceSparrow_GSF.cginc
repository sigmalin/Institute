#ifndef __TorranceSparrow_GSF_
#define __TorranceSparrow_GSF_
/// http://graphicrants.blogspot.com/2013/08/specular-brdf-reference.html
/// https://www.jordanstevenstechart.com/physically-based-rendering
/// Geometric Shadowing Function

float Implicit(float NdotV, float NdotL)
{
	return NdotL * NdotV;
}

float AshikhminShirley(float NdotV, float NdotL, float LdotH)
{
	return (NdotL * NdotV) / (LdotH * max(NdotL, NdotV));
}

float AshikhminPremoze(float NdotV, float NdotL)
{
	float NdotLV = NdotL * NdotV;
	return NdotLV / (NdotL + NdotV - NdotLV);
}

float Duer(float3 lightDirection, float3 viewDirection, float3 normalDirection, float NdotV, float NdotL)
{
	float3 LpV = lightDirection + viewDirection;
	float Gs = dot(LpV,LpV) * pow(dot(LpV,normalDirection),-4);
	return  (Gs);
}

float Neumann(float NdotV, float NdotL)
{
	return (NdotL * NdotV) / max(NdotL, NdotV);
}

float Kelemen(float NdotV, float NdotL, float LdotH, float VdotH)
{
	//return (NdotL * NdotV) / (LdotH * LdotH);
	return (NdotL * NdotV) / (VdotH * VdotH);
}

float ModifiedKelemen(float NdotV, float NdotL, float roughness)
{
	float c = 0.797884560802865;    // c = sqrt(2 / Pi)
	float k = roughness * roughness * c;
	float gH = NdotV  * k +(1-k);
	return (gH * gH * NdotL);
}

float CookTorrence(float NdotV, float NdotL, float VdotH, float NdotH)
{
	return min(1.0, min(2*NdotH*NdotV / VdotH, 2*NdotH*NdotL / VdotH));
}

float Ward(float NdotV, float NdotL)
{
	return pow( NdotL * NdotV, 0.5);
}

float Kurt(float NdotV, float NdotL, float VdotH, float roughness)
{
	float nom   = NdotL * NdotV;
	float denom = VdotH * pow(NdotL * NdotV, roughness);

	return nom / denom;
}

#endif // __TorranceSparrow_GSF_
