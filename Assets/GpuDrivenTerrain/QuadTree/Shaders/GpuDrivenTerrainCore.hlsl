#ifndef _GPU_DRIVEN_TERRAIN_CORE_H
#define _GPU_DRIVEN_TERRAIN_CORE_H

#include "RenderPatch.hlsl"

float offsetLOD;

struct appdata
{
    float4 vertex : POSITION;
    half4 color : COLOR;
    uint uv : TEXCOORD0;
};

inline float3 TransformLocalToWorld(appdata v, RenderPatch data)
{
    uint row = (v.uv & 0xffff) + (data.coordinate & 0xffff);
    uint col = ((v.uv >> 16) & 0xffff) + ((data.coordinate >> 16) & 0xffff);

    uint2 coordinate = uint2(row, col);
    uint4 neighbor = uint4(data.neighbor, (data.neighbor >> 8), (data.neighbor >> 16), (data.neighbor >> 24)) & 0xff;
    neighbor = uint4(1 << neighbor.r, 1 << neighbor.g, 1 << neighbor.b, 1 << neighbor.a);
    neighbor = neighbor.rgba - 1;

    float2 offset = float2(0, 0);
    offset -= v.color.rg * (coordinate & neighbor.rg);
    offset -= v.color.ab * (coordinate & neighbor.ab);

    float3 position = v.vertex.xyz;
    position.xz += offset * offsetLOD;
    position.xz *= (1 << data.lod);
    position.xz += data.position;

    return position;
}

#endif
