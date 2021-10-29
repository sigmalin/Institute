using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RayMarchingComputeRenderPassFeature : ScriptableRendererFeature
{
    class RayMarchingComputeRenderPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "RayMarchingComputeRenderPass";

        RenderTargetIdentifier currentTarget;

        protected RenderTargetHandle cameraColourHandle;
        protected RenderTargetHandle cameraDepthHandle;
        protected RenderTargetHandle rayMarchingTexHandle;

        protected Vector2 rayMarchingTexSize;

        protected Setting passSetting;

        public RayMarchingComputeRenderPass(Setting setting)
        {
            cameraColourHandle.Init("_CameraColorTexture");
            cameraDepthHandle.Init("_CameraDepthTexture");
            rayMarchingTexHandle.Init("_RayMarchingTexture");
            passSetting = setting;
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
            if (!passSetting.computeShader || !renderingData.cameraData.requiresDepthTexture) return;

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);

            using (new ProfilingScope(cmd, new ProfilingSampler(k_RenderTag)))
            {
                try
                {
                    Render(cmd, passSetting.computeShader, ref renderingData);
                }
                catch
                {
                }
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor descriptor = cameraTextureDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.enableRandomWrite = true;
            descriptor.msaaSamples = 1;

            rayMarchingTexSize.x = descriptor.width;
            rayMarchingTexSize.y = descriptor.height;

            cmd.GetTemporaryRT(rayMarchingTexHandle.id, descriptor, FilterMode.Point);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(rayMarchingTexHandle.id);
        }

        private void Render(CommandBuffer cmd, ComputeShader cs, ref RenderingData renderingData)
        {
            int kanel = cs.FindKernel("CSMain");

            uint sizeX, sizeY, sizeZ;
            cs.GetKernelThreadGroupSizes(
                kanel,
                out sizeX,
                out sizeY,
                out sizeZ
            );

            cmd.SetComputeVectorParam(cs, Shader.PropertyToID("_RT_Size"), rayMarchingTexSize);
            cmd.SetComputeVectorParam(cs, Shader.PropertyToID("_WorldCameraPos"), renderingData.cameraData.camera.transform.position);
            cmd.SetComputeVectorParam(cs, Shader.PropertyToID("_LightDirection"), -RenderSettings.sun.transform.forward);
            cmd.SetComputeVectorParam(cs, Shader.PropertyToID("_Sphere"), passSetting.sphereParam);

            cmd.SetComputeTextureParam(cs, 0, rayMarchingTexHandle.id, rayMarchingTexHandle.Identifier());
            cmd.SetComputeTextureParam(cs, 0, cameraColourHandle.id, cameraColourHandle.Identifier());
            cmd.SetComputeTextureParam(cs, 0, cameraDepthHandle.id, cameraDepthHandle.Identifier());

            cmd.DispatchCompute(cs, 0, Mathf.CeilToInt(rayMarchingTexSize.x / sizeX), Mathf.CeilToInt(rayMarchingTexSize.y / sizeY), 1);
            cmd.Blit(rayMarchingTexHandle.Identifier(), currentTarget);
        }
    }

    RayMarchingComputeRenderPass m_ScriptablePass;

    [System.Serializable]
    public class Setting
    {
        public ComputeShader computeShader;

        public Vector4 sphereParam;
    }

    public Setting setting = new Setting();

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new RayMarchingComputeRenderPass(setting);

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


