#ifndef __DualParaboloid_
#define __DualParaboloid_

float4 SAMPLE_DUALPARBOLOID_LOD(Texture2D tex, SamplerState state, float3 r, float lod)
{
	float2 uvF = r.xy / (r.z + 1);
	uvF = uvF * 0.5 + 0.5;
	uvF.x *= 0.5;

	float2 uvR = r.xy / (1 - r.z);
	uvR = uvR * 0.5 + 0.5;
	uvR.x = (1 - uvR.x) * 0.5 + 0.5;

	float2 uv = lerp(uvF, uvR, r.z < 0);

	return SAMPLE_TEXTURE2D_LOD(tex, state, uv, lod);
}

#endif // __DualParaboloid_
