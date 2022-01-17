using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GpuDrivenRenderPassFeature : ScriptableRendererFeature
{
    GpuDrivenTerrainPass m_TerrainPass;

    public static GpuDrivenRenderPassFeature Instance { private set; get; }

    /// <inheritdoc/>
    public override void Create()
    {
        GpuDrivenRenderPassFeature.Instance = this;

        m_TerrainPass = new GpuDrivenTerrainPass();
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_TerrainPass);
    }

    public void RegisterTerrain(IGpuDrivenTerrain terrain)
    {
        if (m_TerrainPass != null)
        {
            m_TerrainPass.Register(terrain);
        }
    }
}


