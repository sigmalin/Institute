//https://64.github.io/tonemapping/
#ifndef __ToneMapping_
#define __ToneMapping_

float3 Reinhard_tone_mapping(float3 hdrColor)
{
	return hdrColor / (hdrColor + float3(1.0,1.0,1.0));
}

float3 Inv_Reinhard_tone_mapping(float3 ldrColor)
{
	float3 denominator = 1 - min(0.66,ldrColor);
	//return ldrColor / (float3(1.0,1.0,1.0) - ldrColor);
	return ldrColor / denominator;
}

float3 Filmic_tone_mapping(float3 linearCol)
{
	float3 x = max(0, linearCol - 0.004);
	float3 x1 = x * 6.2;

	return ((x1 + 0.5) * x ) / ((x1 + 1.7) * x + 0.06);
}

float3 Approx_ACES(float3 linearCol)
{
	float a = 2.51;
    float b = 0.03;
    float c = 2.43;
    float d = 0.59;
    float e = 0.14;
    return clamp((linearCol*(a*linearCol+b))/(linearCol*(c*linearCol+d)+e), 0.0, 1.0);
}

float3 ACES_tone_mapping(float3 linearCol, float exposureBias)
{
	return Approx_ACES(linearCol * exposureBias); 
}

#endif // __ToneMapping_
