using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLightRenderPassFeature : ScriptableRendererFeature
{
    class VolumetricLightRenderPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "VolumetricLightRenderPass";

        RenderTargetIdentifier currentTarget;

        protected ShaderTagId shaderTagId;

        protected RenderTargetHandle volumetricHandle;
        protected RenderTargetHandle gaussianBlurHandle;
        protected RenderTargetHandle lowResDepthHandle;
        protected RenderTargetHandle compositingHandle;

        protected Material volumetricLightMaterial;

        protected Setting passSetting;

        public VolumetricLightRenderPass(Setting setting)
        {
            volumetricHandle.Init("_Volumetric");
            gaussianBlurHandle.Init("_GaussianBlur");
            lowResDepthHandle.Init("_LowResDepth");
            compositingHandle.Init("_Compositing");
            shaderTagId = new ShaderTagId("UniversalForward");
            passSetting = setting;

            Shader volumetricLightShader = Shader.Find("Urp/VolumetricLightShader");
            if (volumetricLightShader)
            {
                volumetricLightMaterial = new Material(volumetricLightShader);
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
            if (!volumetricLightMaterial || !renderingData.cameraData.requiresDepthTexture)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);

            using (new ProfilingScope(cmd, new ProfilingSampler(k_RenderTag)))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                try
                {
                    volumetricLightMaterial.SetFloat("_Scattering", passSetting.scatter);
                    volumetricLightMaterial.SetFloat("_Steps", passSetting.step);
                    volumetricLightMaterial.SetFloat("_MaxDistance", passSetting.maxDistance);
                    volumetricLightMaterial.SetFloat("_JitterVolumetric", passSetting.jitter);
                    volumetricLightMaterial.SetFloat("_Intensity", passSetting.intensity);
                    volumetricLightMaterial.SetFloat("_GaussSamples", passSetting.gaussBlur.samples);
                    volumetricLightMaterial.SetFloat("_GaussAmount", passSetting.gaussBlur.amount);

                    // ray marching
                    cmd.Blit(currentTarget, volumetricHandle.Identifier(), volumetricLightMaterial, 0);
                    // blur
                    cmd.Blit(volumetricHandle.Identifier(), gaussianBlurHandle.Identifier(), volumetricLightMaterial, 1);
                    cmd.Blit(gaussianBlurHandle.Identifier(), volumetricHandle.Identifier(), volumetricLightMaterial, 2);

                    cmd.SetGlobalTexture(volumetricHandle.id, volumetricHandle.Identifier());

                    // down sample depth
                    cmd.Blit(currentTarget, lowResDepthHandle.Identifier(), volumetricLightMaterial, 3);

                    cmd.SetGlobalTexture(lowResDepthHandle.id, lowResDepthHandle.Identifier());

                    //upsample and composite
                    cmd.Blit(currentTarget, compositingHandle.Identifier(), volumetricLightMaterial, 4);
                    cmd.Blit(compositingHandle.Identifier(), currentTarget);
                }
                catch
                {

                }
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
            int divider = (int)passSetting.downsampling;

            RenderTextureDescriptor descriptor1 = cameraTextureDescriptor;
            descriptor1.colorFormat = RenderTextureFormat.R16;
            descriptor1.msaaSamples = 1;
            descriptor1.width /= divider;
            descriptor1.height /= divider;

            RenderTextureDescriptor descriptor2 = cameraTextureDescriptor;
            descriptor2.msaaSamples = 1;

            cmd.GetTemporaryRT(volumetricHandle.id, descriptor1, FilterMode.Bilinear);
            ConfigureTarget(volumetricHandle.Identifier());
            cmd.GetTemporaryRT(gaussianBlurHandle.id, descriptor1, FilterMode.Bilinear);
            ConfigureTarget(gaussianBlurHandle.Identifier());
            cmd.GetTemporaryRT(lowResDepthHandle.id, descriptor1, FilterMode.Bilinear);
            ConfigureTarget(lowResDepthHandle.Identifier());
            cmd.GetTemporaryRT(compositingHandle.id, descriptor2, FilterMode.Bilinear);
            ConfigureTarget(compositingHandle.Identifier());

            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(volumetricHandle.id);
            cmd.ReleaseTemporaryRT(gaussianBlurHandle.id);
            cmd.ReleaseTemporaryRT(lowResDepthHandle.id);
            cmd.ReleaseTemporaryRT(compositingHandle.id);
        }
    }

    VolumetricLightRenderPass m_ScriptablePass;

    [System.Serializable]
    public class Setting
    {
        public float scatter = -0.4f;

        public float step = 25f;

        public float maxDistance = 75f;

        public float jitter = 250f;

        public float intensity = 2.53f;

        public enum DownSample { off = 1, half = 2, third = 3, quarter = 4 };
        public DownSample downsampling = DownSample.off;

        [System.Serializable]
        public class GaussBlur
        {
            public float amount;
            public float samples;

            public GaussBlur()
            {
                amount = 100f;
                samples = 2f;
            }
        }
        public GaussBlur gaussBlur = new GaussBlur();
    }

    public Setting setting = new Setting();

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new VolumetricLightRenderPass(setting);

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


