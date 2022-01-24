#ifndef _QUAD_TREE_CORE_H
#define _QUAD_TREE_CORE_H

#define PATCH_COUNT_IN_NODE 8

uniform uint LengthOfLod0;
uniform uint MaxLOD;

uint GetLodSize(uint lod)
{
	return (LengthOfLod0 << lod);
}

uint2 CalcNodePos(uint2 node, uint lod)
{
	uint2 pos = uint2(0, 0);
	
	for (uint curLod = lod; curLod < MaxLOD; curLod += 1)
	{
		uint2 offset = node & 0x01;
		node >>= 1;
		offset *= uint2(LengthOfLod0, LengthOfLod0) << curLod;
		pos += offset;
	}

	pos += node * GetLodSize(MaxLOD);

	return pos;
}

float2 CalcPatchPos(uint2 node, uint2 offset, uint lod)
{
	uint2 nodePos = CalcNodePos(node, lod);

	uint nodeSize = GetLodSize(lod);

	float patchSize = ((float)nodeSize) / PATCH_COUNT_IN_NODE;

	float2 patchPos = float2(nodePos.x + patchSize * offset.x, nodePos.y + patchSize * offset.y);

	return patchPos;
}

float3 CalcNodeCenterPos(uint2 node, uint lod)
{
	uint2 centerPos = uint2(LengthOfLod0, LengthOfLod0);
	if (lod == 0) {
		centerPos >>= 1;
	}
	else {
		centerPos <<= (lod - 1);
	}

	centerPos += CalcNodePos(node, lod);

	return float3(centerPos.x, 0, centerPos.y);
}

#endif
