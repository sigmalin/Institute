using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostEffectRenderPassFeature : ScriptableRendererFeature
{
    class CustomPostEffectRenderPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "CustomPostEffectRenderPass";

        static readonly int TempTargetId = Shader.PropertyToID("_TempTargetTex");

        Material customMaterial;
        RenderTargetIdentifier currentTarget;

        public CustomPostEffectRenderPass()
        {
            Shader shader = Shader.Find("Urp/CustomPostEffect");
            if(shader)
            {
                customMaterial = new Material(shader);
            }           
        }

        public void Setup(in RenderTargetIdentifier renderTarget)
        {
            currentTarget = renderTarget;
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
            if (!customMaterial) return;
            if (!renderingData.cameraData.postProcessEnabled) return;

            ProfilingSampler sampler = new ProfilingSampler("CustomPostEffectRenderPass");

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);

            using(new ProfilingScope(cmd, sampler))
            {
                Render(cmd, ref renderingData);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;

            RenderTargetIdentifier source = currentTarget;
            int destination = TempTargetId;

            int width = cameraData.camera.scaledPixelWidth;
            int height = cameraData.camera.scaledPixelHeight;

            cmd.GetTemporaryRT(destination, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.Default);            
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, customMaterial);
        }
    }

    CustomPostEffectRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomPostEffectRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


