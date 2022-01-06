using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TerrainQuadTree
{
    QuadTreeSetting Setting;

    QuadTreeTraverser Traverser;
    QuadTreeBuildPatches Builder;

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

        Builder = new QuadTreeBuildPatches(Setting);
        Builder.Initialize();
    }

    public void Release()
    {
        if (Traverser != null)
        {
            Traverser.Release();
            Traverser = null;
        }

        if (Builder != null)
        {
            Builder.Initialize();
            Builder = null;
        }

        RenderPatchesBuffer = null;
    }

    bool isValid()
    {
        return SystemInfo.supportsComputeShaders == true &&
                Traverser != null &&
                Builder != null;
    }

    public void Process(out GraphicsBuffer RenderPatchesBuffer)
    {
        RenderPatchesBuffer = null;

        if (isValid() == false) return;

        GraphicsBuffer buffer;
        int size = Traverser.Traverse(out buffer);
        if (size == 0) return;

        Builder.BuildBatch(buffer, size, out RenderPatchesBuffer);
    }
}
