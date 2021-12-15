using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HiZRenderPassFeature : ScriptableRendererFeature
{
    class HiZRenderPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "HiZRenderPass";

        protected RenderTargetHandle cameraDepthHandle;
        protected RenderTargetHandle HiZHandle;

        RenderTextureDescriptor descriptor;

        protected Setting passSetting;

        public HiZRenderPass(Setting setting)
        {
            cameraDepthHandle.Init("_CameraDepthTexture");
            HiZHandle.Init("_HiZTexture");

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
            if (!passSetting.computeShader || !renderingData.cameraData.requiresDepthTexture) return;
            
            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);

            using (new ProfilingScope(cmd, new ProfilingSampler(k_RenderTag)))
            {
                try
                {
                    cmd.Blit(cameraDepthHandle.Identifier(), HiZHandle.Identifier());

                    int level = 0;
                    int width = descriptor.width;
                    int height = descriptor.height;
                    while (8 < width && 8 < height)
                    {
                        CopyMipLevel(cmd, passSetting.computeShader, level);

                        level += 1;
                        width >>= 1;
                        height >>= 1;
                    }

                    context.ExecuteCommandBuffer(cmd);
                }
                catch
                {
                    Debug.LogError("Some Thing Error!");
                }

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
            descriptor = cameraTextureDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.colorFormat = RenderTextureFormat.RHalf;
            descriptor.autoGenerateMips = false;
            descriptor.useMipMap = true;
            descriptor.msaaSamples = 1;
            descriptor.enableRandomWrite = true;
            cmd.GetTemporaryRT(HiZHandle.id, descriptor, FilterMode.Point);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(HiZHandle.id);
        }

        private void CopyMipLevel(CommandBuffer cmd, ComputeShader cs, int level)
        {
            int kanel = cs.FindKernel("CSDownSample");

            uint sizeX, sizeY;
            cs.GetKernelThreadGroupSizes(
                kanel,
                out sizeX,
                out sizeY,
                out _
            );

            int nextLevel = level + 1;

            RenderTextureDescriptor desc = descriptor;
            desc.width >>= nextLevel;
            desc.height >>= nextLevel;

            int mipDepthTextureID = Shader.PropertyToID("_mipDepthTexture");
            cmd.GetTemporaryRT(mipDepthTextureID, desc, FilterMode.Point);

            Vector4 texSize = new Vector4(desc.width, desc.height, 1f / desc.width, 1f / desc.height);
            cmd.SetComputeVectorParam(cs, Shader.PropertyToID("_RT_Size"), texSize);

            cmd.SetComputeTextureParam(cs, 0, HiZHandle.id, HiZHandle.Identifier(), level);
            cmd.SetComputeTextureParam(cs, 0, mipDepthTextureID, mipDepthTextureID);

            cmd.DispatchCompute(cs, kanel, Mathf.CeilToInt((desc.width + sizeX - 1) / sizeX), Mathf.CeilToInt((desc.height + sizeY - 1) / sizeY), 1);
            cmd.CopyTexture(mipDepthTextureID, 0, 0, HiZHandle.Identifier(), 0, nextLevel);

            cmd.ReleaseTemporaryRT(mipDepthTextureID);
        }
    }

    HiZRenderPass m_ScriptablePass;

    [System.Serializable]
    public class Setting
    {
        public ComputeShader computeShader;
    }

    public Setting setting = new Setting();

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new HiZRenderPass(setting);

        // Configures where the render pass should be injected.
        //m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(renderingData.cameraData.camera == Camera.main)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }        
    }
}


