//https://zhuanlan.zhihu.com/p/388844386
//https://zhuanlan.zhihu.com/p/352850047
//https://www.decarpentier.nl/scape-render
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GpuDrivenTerrain : MonoBehaviour, IGpuDrivenTerrain
{
    public class TerrainRenderPass
    {
        public const int Opaques = 0;
        public const int Wireframe = 1;
    };

    public QuadTreeSetting Setting = new QuadTreeSetting();

    public bool isShowWireframe;

    Mesh meshTerrain;

    TerrainQuadTree quadTree;

    ComputeBuffer renderPatchesBuffer;
    GraphicsBuffer argBuffer;

    // Start is called before the first frame update
    void Start()
    {
        LodMeshCreator.Generate(Setting.LodMeshStep, Setting.LodMeshRadius, out meshTerrain, out argBuffer);

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

        Setting.matTerrain.SetFloat(Shader.PropertyToID("offsetLOD"), Setting.OffsetLOD);

        Debug.LogFormat("四元樹處理時間 : {0} ms", Time.time - startTime);

        if (GpuDrivenRenderPassFeature.Instance != null)
        {
            GpuDrivenRenderPassFeature.Instance.RegisterTerrain(this);
        }
    }

    public bool onCulling(CommandBuffer _cmd, Camera _cam)
    {
        if (renderPatchesBuffer == null) return false;

        ComputeBuffer culledPatchesBuffer;
        quadTree.onCulling(_cmd, Camera.main, renderPatchesBuffer, out culledPatchesBuffer);

        renderPatchesBuffer = culledPatchesBuffer;

        return true;
    }

    public bool onRender(CommandBuffer _cmd)
    {
        if (renderPatchesBuffer == null) return false;

        Setting.matTerrain.SetBuffer(Shader.PropertyToID("CulledPatchList"), renderPatchesBuffer);

        _cmd.CopyCounterValue(renderPatchesBuffer, argBuffer, sizeof(uint));

        _cmd.DrawMeshInstancedIndirect(
            meshTerrain,
            0,
            Setting.matTerrain,
            TerrainRenderPass.Opaques,
            argBuffer
            );

        if (isShowWireframe)
        {
            _cmd.DrawMeshInstancedIndirect(
            meshTerrain,
            0,
            Setting.matTerrain,
            TerrainRenderPass.Wireframe,
            argBuffer
            );
        }

        return true;
    }
}
