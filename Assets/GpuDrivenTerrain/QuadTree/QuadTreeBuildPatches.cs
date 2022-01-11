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

    int LodMapShaderID;
    int NodeSizeAtMaxLodID;

    GraphicsBuffer RenderPatchBuffer;

    QuadTreeSetting Setting;

    public QuadTreeBuildPatches(QuadTreeSetting setting)
    {
        Setting = setting;

        if (Setting != null && Setting.BuildPatchesCS != null)
        {
            kernelBuildPatches = Setting.BuildPatchesCS.FindKernel("BuildPatches");

            FinalNodeListShaderID = Shader.PropertyToID("FinalNodeList");
            CulledPatchListShaderID = Shader.PropertyToID("RenderPatchList");

            LengthOfLod0ShaderID = Shader.PropertyToID("LengthOfLod0");
            MaxLODShaderID = Shader.PropertyToID("MaxLOD");

            LodMapShaderID = Shader.PropertyToID("LodMap");
            NodeSizeAtMaxLodID = Shader.PropertyToID("NodeSizeAtMaxLOD");
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
                RenderPatchBuffer != null;
    }

    void InitGraphicsBuffer()
    {
        ReleaseGraphicsBuffer();

        int maxPatchCount = Setting.MaxPatchCount;

        RenderPatchBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, maxPatchCount, sizeof(float) * 2 + sizeof(uint) + sizeof(uint));
    }

    void ReleaseGraphicsBuffer()
    {
        if (RenderPatchBuffer != null)
        {
            RenderPatchBuffer.Release();
            RenderPatchBuffer.Dispose();
            RenderPatchBuffer = null;
        }
    }

    void BuildRenderBatches(GraphicsBuffer srcBuffer, int srcSize, RenderTexture rtLodMap)
    {
        RenderPatchBuffer.SetCounterValue(0);

        Setting.BuildPatchesCS.SetBuffer(kernelBuildPatches, FinalNodeListShaderID, srcBuffer);
        Setting.BuildPatchesCS.SetBuffer(kernelBuildPatches, CulledPatchListShaderID, RenderPatchBuffer);

        Setting.BuildPatchesCS.SetTexture(kernelBuildPatches, LodMapShaderID, rtLodMap);

        Setting.BuildPatchesCS.SetInt(LengthOfLod0ShaderID, Setting.LengthOfLod0);
        Setting.BuildPatchesCS.SetInt(MaxLODShaderID, Setting.MaxLOD);

        Setting.BuildPatchesCS.SetInt(NodeSizeAtMaxLodID, Setting.NodeSizeAtMaxLOD);

        Setting.BuildPatchesCS.Dispatch(kernelBuildPatches, srcSize, 1, 1);
    }

    public bool BuildBatch(GraphicsBuffer srcBuffer, int srcSize, RenderTexture rtLodMap, out GraphicsBuffer buffer)
    {        
        buffer = null;

        if (srcBuffer == null || srcSize == 0 || rtLodMap == null) return false;

        if (isValid() == false) return false;

        BuildRenderBatches(srcBuffer, srcSize, rtLodMap);

        buffer = RenderPatchBuffer;

        return true;
    }
}
