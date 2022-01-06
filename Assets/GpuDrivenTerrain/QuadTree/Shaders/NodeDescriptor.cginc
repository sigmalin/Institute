#ifndef _NODE_DESCRIPTOR_H
#define _NODE_DESCRIPTOR_H

struct NodeDescriptor
{
	uint branch;
};

RWStructuredBuffer<NodeDescriptor> NodeDescriptors;

uint NodeSizeAtMaxLOD;

uint GetNodeCount(uint lod)
{
	return NodeSizeAtMaxLOD << (lod);
}

uint GetNodeIdOffset(uint lod)
{
	uint offset = 0;

	for (uint i = 0; i < lod; ++i)
	{
		uint count = GetNodeCount(i);
		offset += count * count;
	}

	return offset;
}

uint GetNodeID(uint2 node, uint lod)
{
	return GetNodeIdOffset(lod) + node.y * GetNodeCount(lod) + node.x;
}

void SetNodeBranch(uint2 node, uint lod, uint branch)
{
	uint id = GetNodeID(node, lod);
	NodeDescriptors[id].branch = branch;
}

#endif
