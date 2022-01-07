using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TerrainQuadTree
{
    QuadTreeSetting Setting;

    QuadTreeTraverser Traverser;
    QuadTreeBuildPatches PatchesBuilder;
    QuadTreeLodMap LodMapBuilder;

    GraphicsBuffer RenderPatchesBuffer;

    public TerrainQuadTree(QuadTreeSetting setting)
    {
        Setting = setting;
    }

    public void Initialize()
    {
        Release();

        Traverser = new QuadTreeTraverser(Setting);
        Traverser.Initialize();

        PatchesBuilder = new QuadTreeBuildPatches(Setting);
        PatchesBuilder.Initialize();

        LodMapBuilder = new QuadTreeLodMap(Setting);
        LodMapBuilder.Initialize();
    }

    public void Release()
    {
        if (Traverser != null)
        {
            Traverser.Release();
            Traverser = null;
        }

        if (PatchesBuilder != null)
        {
            PatchesBuilder.Release();
            PatchesBuilder = null;
        }

        if (LodMapBuilder != null)
        {
            LodMapBuilder.Release();
            LodMapBuilder = null;
        }

        RenderPatchesBuffer = null;
    }

    bool isValid()
    {
        return SystemInfo.supportsComputeShaders == true &&
                Traverser != null &&
                PatchesBuilder != null &&
                LodMapBuilder != null;
    }

    public void Process(out GraphicsBuffer RenderPatchesBuffer)
    {
        RenderPatchesBuffer = null;

        if (isValid() == false) return;

        GraphicsBuffer buffer;
        int size = Traverser.Traverse(out buffer);
        if (size == 0) return;

        RenderTexture LodMap;
        LodMapBuilder.Build(Traverser.GetNodeDescriptors(), out LodMap);

        PatchesBuilder.BuildBatch(buffer, size, out RenderPatchesBuffer);
    }
}
