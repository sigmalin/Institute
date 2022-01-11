//https://zhuanlan.zhihu.com/p/388844386
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GpuDrivenTerrain : MonoBehaviour, IGpuDrivenUnit
{
    public QuadTreeSetting Setting = new QuadTreeSetting();

    public bool isShowWireframe;

    Mesh meshTerrain;

    TerrainQuadTree quadTree;

    GraphicsBuffer renderPatchesBuffer;
    GraphicsBuffer argBuffer;

    // Start is called before the first frame update
    void Start()
    {
        LodMeshCreator.Generate(4, Setting.LodMeshRadius, out meshTerrain, out argBuffer);

        quadTree = new TerrainQuadTree(Setting);
        quadTree.Initialize();

        renderPatchesBuffer = null;
    }

    private void OnDestroy()
    {
        if (quadTree != null)
        {
            quadTree.Release();
            quadTree = null;
        }

        if (meshTerrain != null)
        {
            meshTerrain.Clear();
            meshTerrain = null;
        }

        if (argBuffer != null)
        {
            argBuffer.Release();
            argBuffer.Dispose();
            argBuffer = null;
        }

        renderPatchesBuffer = null;
    }

    bool isValid()
    {
        return Setting.matTerrain != null &&
                meshTerrain != null &&
                argBuffer != null &&
                quadTree != null;
    }

    void LateUpdate()
    {
        if (isValid() == false) return;

        renderPatchesBuffer = null;

        float startTime = Time.time;

        quadTree.Process(out renderPatchesBuffer);

        Setting.matTerrain.SetBuffer(Shader.PropertyToID("CulledPatchList"), renderPatchesBuffer);
        Setting.matTerrain.SetFloat(Shader.PropertyToID("offsetLOD"), 1f);

        Debug.LogFormat("四元樹處理時間 : {0} ms", Time.time - startTime);

        if (GpuDrivenRenderPassFeature.Instance != null)
        {
            GpuDrivenRenderPassFeature.Instance.Register(this);
        }
    }

    public bool onRender(CommandBuffer _cmd)
    {
        if (renderPatchesBuffer == null) return false;

        _cmd.CopyCounterValue(renderPatchesBuffer, argBuffer, sizeof(uint));

        _cmd.DrawMeshInstancedIndirect(
            meshTerrain,
            0,
            Setting.matTerrain,
            isShowWireframe ? -1 : 0,
            argBuffer
            );

        return true;
    }
}
