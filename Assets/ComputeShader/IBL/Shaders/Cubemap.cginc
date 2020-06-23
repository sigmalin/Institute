#ifndef __Cubemap_
#define __Cubemap_

float4 decodeInstructions;
float colorSpace;

float4 filtering_cube_map(TextureCube<float4> cubemap, SamplerState state, float3 n) 
{
    n.yz = -n.yz;

	float4 col = cubemap.SampleLevel(state,n,0);
	col.rgb = pow(col.rgb, colorSpace);

	float alpha = decodeInstructions.w * (col.a - 1) + 1;
	col.rgb *= (decodeInstructions.x * pow(abs(alpha), decodeInstructions.y));

    return min(col,4);
}

float3 calc_normal(int face, float2 uv) {
    // 6 Face(+X,-X,+Y,-Y,+Z,-Z) for [0,5]
    uv = (uv - 0.5) * 2.0;  // Convert range [0, 1] to [-1, 1]

    float3 n = float3(0.0, 0.0, 0.0);
    if (face == 0) {
        // +X face for Unity
        n.x = 1.0;
        n.zy = uv;
    } else if (face == 1) {
        // -X face for Unity
        n.x = -1.0;
        n.z = -uv.x;
        n.y = uv.y;
    } else if (face == 2) {
        // +Y face for Unity
		n.y = -1.0;
        n.x = uv.x;
        n.z = -uv.y;        
    } else if (face == 3) {
        // -Y face for Unity
        n.y = 1.0;
        n.xz = uv;
    } else if (face == 4) {
        // +Z face for Unity
		n.z = -1.0;
        n.xy = uv;        
    } else if (face == 5) {
        // -Z face for Unity
        n.z = 1.0;
        n.x = -uv.x;
        n.y = uv.y;
    }

    return n;
}

#endif // __Cubemap_
