using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class TestPipeline
{
    void DrawDepthOnly(ScriptableRenderContext context, Camera camera)
    {
        SortingSettings sortingSettings = new SortingSettings(camera);
        sortingSettings.criteria = SortingCriteria.CommonOpaque;       // front to back.

        //https://docs.unity3d.com/ScriptReference/Rendering.DrawingSettings.html
        DrawingSettings drawingSettings = new DrawingSettings();
        drawingSettings.SetShaderPassName(0, shaderTagIds[0]);
        drawingSettings.sortingSettings = sortingSettings;
        drawingSettings.enableInstancing = true;

        //https://docs.unity3d.com/ScriptReference/Rendering.FilteringSettings.html
        FilteringSettings filteringSettings = new FilteringSettings();
        filteringSettings.renderQueueRange = RenderQueueRange.opaque;
        filteringSettings.renderingLayerMask = 1;
        filteringSettings.layerMask = camera.cullingMask;

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    void DrawOpaque(ScriptableRenderContext context, Camera camera)
    {
        SortingSettings sortingSettings = new SortingSettings(camera);
        sortingSettings.criteria = SortingCriteria.OptimizeStateChanges;       // Better for SRP Batcher.

        //https://docs.unity3d.com/ScriptReference/Rendering.DrawingSettings.html
        DrawingSettings drawingSettings = new DrawingSettings();
        drawingSettings.SetShaderPassName(0, shaderTagIds[1]);
        drawingSettings.sortingSettings = sortingSettings;
        drawingSettings.enableInstancing = true;

        for (int i = 2; i < shaderTagIds.Length; ++i)
            drawingSettings.SetShaderPassName(i, shaderTagIds[i]);

        //https://docs.unity3d.com/ScriptReference/Rendering.FilteringSettings.html
        FilteringSettings filteringSettings = new FilteringSettings();
        filteringSettings.renderQueueRange = RenderQueueRange.opaque;
        filteringSettings.renderingLayerMask = 1;
        filteringSettings.layerMask = camera.cullingMask;

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }
}
