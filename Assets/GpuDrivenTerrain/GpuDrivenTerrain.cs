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

    public TerrainSetting Setting = new TerrainSetting();

    public bool isShowWireframe;

    public bool isCulling;

    public bool isDisableQuadTreeTraver = false;
    private bool _DisableQuadTreeTraver;

    Mesh meshTerrain;

    TerrainQuadTree quadTree;

    ComputeBuffer renderPatchesBuffer;
    GraphicsBuffer argBuffer;

    // Start is called before the first frame update
    void Start()
    {
        LodMeshCreator.Generate(Setting.QuadTree.LodMeshStep, Setting.QuadTree.LodMeshRadius, out meshTerrain, out argBuffer);

        quadTree = new TerrainQuadTree(Setting.QuadTree);
        quadTree.Initialize();

        renderPatchesBuffer = null;

        setQuadTreeTraverDisable(false);
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

    void setQuadTreeTraverDisable(bool isDisable)
    {
        _DisableQuadTreeTraver = isDisable;

        if (Setting.QuadTree.TraverserCS == null) return;

        const string DISABLE_KEY_WORD = "QUAD_TREE_TRAVERSE_DISABLE";

        if (_DisableQuadTreeTraver)
        {
            Setting.QuadTree.TraverserCS.EnableKeyword(DISABLE_KEY_WORD);
        }
        else
        {
            Setting.QuadTree.TraverserCS.DisableKeyword(DISABLE_KEY_WORD);
        }
    }

    void Update()
    {
        if(isDisableQuadTreeTraver != _DisableQuadTreeTraver)
        {
            setQuadTreeTraverDisable(isDisableQuadTreeTraver);
        }        
    }

    void LateUpdate()
    {
        if (isValid() == false) return;

        renderPatchesBuffer = null;

        float startTime = Time.time;

        quadTree.Process(out renderPatchesBuffer);

        Setting.matTerrain.SetFloat(Shader.PropertyToID("offsetLOD"), Setting.QuadTree.OffsetLOD);

        Debug.LogFormat("四元樹處理時間 : {0} ms", Time.time - startTime);

        if (GpuDrivenRenderPassFeature.Instance != null)
        {
            GpuDrivenRenderPassFeature.Instance.RegisterTerrain(this);
        }
    }

    public bool onCulling(CommandBuffer _cmd, Camera _cam)
    {
        if (renderPatchesBuffer == null) return false;

        if (isCulling == true)
        {
            ComputeBuffer culledPatchesBuffer;
            quadTree.Culling(_cmd, Camera.main, renderPatchesBuffer, out culledPatchesBuffer);

            renderPatchesBuffer = culledPatchesBuffer;
        }        

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
