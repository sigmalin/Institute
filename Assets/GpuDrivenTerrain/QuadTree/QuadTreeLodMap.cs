using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class QuadTreeLodMap
{
    int kernelBuildLodMap;

    int LodMapShaderID;
    int LodMapSizeShaderID;
    int MaxLODShaderID;

    int NodeSizeAtMaxLodID;
    int NodeDescriptorsShaderID;

    RenderTexture rtLodMap;

    QuadTreeSetting Setting;

    public QuadTreeLodMap(QuadTreeSetting setting)
    {
        Setting = setting;

        if (Setting != null && Setting.BuildLodMapCS != null)
        {
            kernelBuildLodMap = Setting.BuildLodMapCS.FindKernel("BuildLodMap");

            LodMapShaderID = Shader.PropertyToID("LodMap");
            LodMapSizeShaderID = Shader.PropertyToID("LodMapSize");
            MaxLODShaderID = Shader.PropertyToID("MaxLOD");

            NodeSizeAtMaxLodID = Shader.PropertyToID("NodeSizeAtMaxLOD");
            NodeDescriptorsShaderID = Shader.PropertyToID("NodeDescriptors");
        }
    }

    public void Initialize()
    {
        if (Setting != null)
        {
            InitRenderTexture();
        }
    }

    public void Release()
    {
        ReleaseRenderTexture();
    }

    bool isValid()
    {
        return Setting != null && Setting.BuildLodMapCS != null &&
                rtLodMap != null;
    }

    void InitRenderTexture()
    {
        ReleaseRenderTexture();

        int size = Setting.NodeSizeAtMaxLOD << Setting.MaxLOD;
        rtLodMap = new RenderTexture(size, size, 0, RenderTextureFormat.R8);
        rtLodMap.enableRandomWrite = true;
        rtLodMap.filterMode = FilterMode.Point;
        rtLodMap.Create();
    }

    void ReleaseRenderTexture()
    {
        if(rtLodMap != null)
        {
            rtLodMap.Release();
            GameObject.Destroy(rtLodMap);
            rtLodMap = null;
        }
    }

    void BuildLodMap (GraphicsBuffer descBuffer)
    {
        if (isValid() == false) return;

        Setting.BuildLodMapCS.SetBuffer(kernelBuildLodMap, NodeDescriptorsShaderID, descBuffer);
        Setting.BuildLodMapCS.SetTexture(kernelBuildLodMap, LodMapShaderID, rtLodMap);

        Setting.BuildLodMapCS.SetVector(LodMapSizeShaderID, new Vector2(rtLodMap.width, rtLodMap.height));
        Setting.BuildLodMapCS.SetInt(NodeSizeAtMaxLodID, Setting.NodeSizeAtMaxLOD);
        Setting.BuildLodMapCS.SetInt(MaxLODShaderID, Setting.MaxLOD);

        uint sizeX, sizeY;
        Setting.BuildLodMapCS.GetKernelThreadGroupSizes(
            kernelBuildLodMap,
            out sizeX,
            out sizeY,
            out _
        );

        Setting.BuildLodMapCS.Dispatch(kernelBuildLodMap, 
                    Mathf.CeilToInt((rtLodMap.width + sizeX - 1) / sizeX),
                    Mathf.CeilToInt((rtLodMap.height + sizeY - 1) / sizeY), 1);
    }

    public bool Build (GraphicsBuffer descBuffer, out RenderTexture lodMap)
    {
        lodMap = null;

        if (descBuffer == null) return false;

        BuildLodMap(descBuffer);

        lodMap = rtLodMap;

        return true;
    }
}
