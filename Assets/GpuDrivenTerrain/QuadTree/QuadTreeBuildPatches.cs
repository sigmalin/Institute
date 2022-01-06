using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class QuadTreeBuildPatches
{
    int kernelBuildPatches;

    int FinalNodeListShaderID;
    int CulledPatchListShaderID;

    int LengthOfLod0ShaderID;
    int MaxLODShaderID;

    GraphicsBuffer CulledPatchBuffer;

    QuadTreeSetting Setting;

    public QuadTreeBuildPatches(QuadTreeSetting setting)
    {
        Setting = setting;

        if (Setting != null && Setting.BuildPatchesCS != null)
        {
            kernelBuildPatches = Setting.BuildPatchesCS.FindKernel("BuildPatches");

            FinalNodeListShaderID = Shader.PropertyToID("FinalNodeList");
            CulledPatchListShaderID = Shader.PropertyToID("CulledPatchList");

            LengthOfLod0ShaderID = Shader.PropertyToID("LengthOfLod0");
            MaxLODShaderID = Shader.PropertyToID("MaxLOD");
        }
    }

    public void Initialize()
    {
        if (Setting != null)
        {
            InitGraphicsBuffer();
        }
    }

    public void Release()
    {
        ReleaseGraphicsBuffer();
    }

    bool isValid()
    {
        return Setting != null && Setting.BuildPatchesCS != null &&
                CulledPatchBuffer != null;
    }

    void InitGraphicsBuffer()
    {
        ReleaseGraphicsBuffer();

        int maxPatchCount = Setting.MaxPatchCount;

        CulledPatchBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, maxPatchCount, sizeof(float) * 2 + sizeof(uint));
    }

    void ReleaseGraphicsBuffer()
    {
        if (CulledPatchBuffer != null)
        {
            CulledPatchBuffer.Release();
            CulledPatchBuffer.Dispose();
            CulledPatchBuffer = null;
        }
    }

    void BuildRenderBatches(GraphicsBuffer srcBuffer, int srcSize)
    {
        CulledPatchBuffer.SetCounterValue(0);

        Setting.BuildPatchesCS.SetBuffer(kernelBuildPatches, FinalNodeListShaderID, srcBuffer);
        Setting.BuildPatchesCS.SetBuffer(kernelBuildPatches, CulledPatchListShaderID, CulledPatchBuffer);

        Setting.BuildPatchesCS.SetInt(LengthOfLod0ShaderID, Setting.LengthOfLod0);
        Setting.BuildPatchesCS.SetInt(MaxLODShaderID, Setting.MaxLOD);

        Setting.BuildPatchesCS.Dispatch(kernelBuildPatches, srcSize, 1, 1);
    }

    public bool BuildBatch(GraphicsBuffer srcBuffer, int srcSize, out GraphicsBuffer buffer)
    {        
        buffer = null;

        if (srcBuffer == null || srcSize == 0) return false;

        if (isValid() == false) return false;

        BuildRenderBatches(srcBuffer, srcSize);

        buffer = CulledPatchBuffer;

        return true;
    }
}
