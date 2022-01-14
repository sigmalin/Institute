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
        protected RenderTargetHandle mipDepthHandle;

        RenderTextureDescriptor descHiZ;
        RenderTextureDescriptor descDepth;

        protected Setting passSetting;
        protected int maxMipLevel;

        public HiZRenderPass(Setting setting)
        {
            cameraDepthHandle.Init("_CameraDepthTexture");
            HiZHandle.Init("_HiZTexture");
            mipDepthHandle.Init("_mipDepthHandle");

            passSetting = setting;

            maxMipLevel = 0;
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
                                        
                    int width = descHiZ.width;
                    int height = descHiZ.height;

                    maxMipLevel = 0;

                    while (8 < width && 8 < height && maxMipLevel < passSetting.MaxMipCount)
                    {
                        CreateMipDepthTexture(cmd, passSetting.computeShader, maxMipLevel + 1);

                        CopyMipLevel(cmd, passSetting.computeShader, maxMipLevel);

                        CopyTexture(cmd, passSetting.computeShader, maxMipLevel + 1);

                        ReleaseMipDepthTexture(cmd);

                        maxMipLevel += 1;
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
            descHiZ = cameraTextureDescriptor;
            descHiZ.depthBufferBits = 0;
            descHiZ.colorFormat = RenderTextureFormat.RHalf;
            descHiZ.autoGenerateMips = false;
            descHiZ.useMipMap = true;
            descHiZ.msaaSamples = 1;
            descHiZ.enableRandomWrite = true;
            descHiZ.mipCount = passSetting.MaxMipCount + 1;
            cmd.GetTemporaryRT(HiZHandle.id, descHiZ, FilterMode.Point);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(HiZHandle.id);
        }

        private void CreateMipDepthTexture(CommandBuffer cmd, ComputeShader cs, int mipLevel)
        {
            descDepth = descHiZ;
            descDepth.width >>= mipLevel;
            descDepth.height >>= mipLevel;

            cmd.GetTemporaryRT(mipDepthHandle.id, descDepth, FilterMode.Point);

            Vector2 texSize = new Vector2(descDepth.width, descDepth.height);
            cmd.SetComputeVectorParam(cs, Shader.PropertyToID("_RT_Size"), texSize);
        }

        private void ReleaseMipDepthTexture(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(mipDepthHandle.id);
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

            cmd.SetComputeIntParam(cs, Shader.PropertyToID("_mipLevel"), level);

            cmd.SetComputeTextureParam(cs, 0, HiZHandle.id, HiZHandle.Identifier());
            cmd.SetComputeTextureParam(cs, 0, Shader.PropertyToID("_mipDepthTexture"), mipDepthHandle.Identifier());

            cmd.DispatchCompute(cs, kanel, Mathf.CeilToInt((descDepth.width + sizeX - 1) / sizeX), Mathf.CeilToInt((descDepth.height + sizeY - 1) / sizeY), 1);
        }

        private void CopyTexture(CommandBuffer cmd, ComputeShader cs, int level)
        {
            int kanel = cs.FindKernel("CSCopyTexture");

            uint sizeX, sizeY;
            cs.GetKernelThreadGroupSizes(
                kanel,
                out sizeX,
                out sizeY,
                out _
            );

            cmd.SetComputeTextureParam(cs, kanel, Shader.PropertyToID("_DestTexture"), HiZHandle.Identifier(), level);
            cmd.SetComputeTextureParam(cs, kanel, Shader.PropertyToID("_SrcTexture"), mipDepthHandle.Identifier());

            cmd.DispatchCompute(cs, kanel, Mathf.CeilToInt((descDepth.width + sizeX - 1) / sizeX), Mathf.CeilToInt((descDepth.height + sizeY - 1) / sizeY), 1);
        }
    }

    HiZRenderPass m_ScriptablePass;

    [System.Serializable]
    public class Setting
    {
        public ComputeShader computeShader;

        [Range(6, 8)]
        public int MaxMipCount = 6;
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


