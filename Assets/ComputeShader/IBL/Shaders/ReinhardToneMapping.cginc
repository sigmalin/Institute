#ifndef __ReinhardToneMapping_
#define __ReinhardToneMapping_

float3 Reinhard_tone_mapping(float3 hdrColor)
{
	return hdrColor / (hdrColor + float3(1.0,1.0,1.0));
}

#endif // __ReinhardToneMapping_
