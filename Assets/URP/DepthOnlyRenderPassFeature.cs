using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthOnlyRenderPassFeature : ScriptableRendererFeature
{
    class DepthOnlyRenderPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "DepthOnlyRenderPass";

        protected RenderTargetHandle depthTexHandle;

        protected ShaderTagId shaderTagId;
        protected Setting passSetting;

        public DepthOnlyRenderPass(Setting setting)
        {
            depthTexHandle.Init("_CameraDepthTexture");
            shaderTagId = new ShaderTagId("DepthOnly");
            passSetting = setting;
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);
            using (new ProfilingScope(cmd, new ProfilingSampler("DepthOnlyRenderPass")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                var drawSettings = CreateDrawingSettings(shaderTagId, ref renderingData, sortFlags);
                drawSettings.perObjectData = PerObjectData.None;

                RenderQueueRange range = new RenderQueueRange();
                range.lowerBound = passSetting.queueMin;
                range.upperBound = passSetting.queueMax;

                FilteringSettings filteringSettings = new FilteringSettings();
                filteringSettings.renderQueueRange = range;
                filteringSettings.renderingLayerMask = 1;
                filteringSettings.layerMask = passSetting.layer;

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);

                cmd.SetGlobalTexture(depthTexHandle.id, depthTexHandle.Identifier());
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor descriptor = cameraTextureDescriptor;
            descriptor.colorFormat = RenderTextureFormat.Depth;
            descriptor.depthBufferBits = 16;

            // Depth-Only pass don't use MSAA
            descriptor.msaaSamples = 1;

            cmd.GetTemporaryRT(depthTexHandle.id, descriptor, FilterMode.Point);
            ConfigureTarget(depthTexHandle.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(depthTexHandle.id);
        }
    }

    DepthOnlyRenderPass m_ScriptablePass;

    [System.Serializable]
    public class Setting
    {
        [Range(1000, 5000)]
        public int queueMin = 1000;
        [Range(1000, 5000)]
        public int queueMax = 2500;

        public LayerMask layer;
    }

    public Setting setting = new Setting();

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new DepthOnlyRenderPass(setting);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPrepasses;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


