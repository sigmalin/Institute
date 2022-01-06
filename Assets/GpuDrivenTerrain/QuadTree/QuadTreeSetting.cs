using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuadTreeSetting
{
    public ComputeShader TraverserCS;
    public ComputeShader BuildPatchesCS;

    public Material matTerrain;

    [Range(4, 8)]
    public int NodeSizeAtMaxLOD = 5;

    [Range(3, 6)]
    public int MaxLOD = 5;

    [HideInInspector]
    public readonly int PatchCountInNode = 8;

    [Range(1, 8)]
    public int LodMeshRadius = 4;

    public int LengthOfLod0
    {
        get { return PatchCountInNode * LodMeshRadius;  }
    }

    public int MaxNodeCount
    {
        get { return Pow4(MaxLOD - 1) * NodeSizeAtMaxLOD * NodeSizeAtMaxLOD;  }
    }

    public int MaxPatchCount
    {
        get { return MaxNodeCount * PatchCountInNode * PatchCountInNode; }
    }

    public int GetDescriptorCount()
    {
        int count = 0;

        for (int i = 0; i <= MaxLOD; ++i)
        {
            int countInLevel = (NodeSizeAtMaxLOD << i);
            count += countInLevel * countInLevel;
        }

        return count;
    }


    int Pow4(int level)
    {
        int res = 1;

        for (int i = 0; i < level; ++i)
        {
            res <<= 2;
        }

        return res;
    }

}
