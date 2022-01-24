#ifndef _NODE_DESCRIPTOR_H
#define _NODE_DESCRIPTOR_H

struct NodeDescriptor
{
	uint branch;
};

RWStructuredBuffer<NodeDescriptor> NodeDescriptors;

uint NodeSizeAtMaxLOD;

uint GetNodeCount(uint curLOD, uint maxLOD)
{
	return NodeSizeAtMaxLOD << (maxLOD - curLOD);
}

uint GetNodeIdOffset(uint curLOD, uint maxLOD)
{
	uint offset = 0;

	for (uint i = maxLOD; curLOD < i; --i)
	{
		uint count = GetNodeCount(i, maxLOD);
		offset += count * count;
	}

	return offset;
}

uint GetNodeID(uint2 node, uint curLOD, uint maxLOD)
{
	return GetNodeIdOffset(curLOD, maxLOD) + node.y * GetNodeCount(curLOD, maxLOD) + node.x;
}

void SetNodeBranch(uint2 node, uint curLOD, uint maxLOD, uint branch)
{
	uint id = GetNodeID(node, curLOD, maxLOD);
	NodeDescriptors[id].branch = branch;
}

int GetNodeBranch(uint2 node, uint curLOD, uint maxLOD)
{
	uint id = GetNodeID(node, curLOD, maxLOD);
	return NodeDescriptors[id].branch;
}

#endif
