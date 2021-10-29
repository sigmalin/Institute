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
        protected RenderTargetHandle lowResDepthHandle;
        protected RenderTargetHandle compositingHandle;

        protected int[] dualKawaseIds;

        protected Material volumetricLightMaterial;

        protected Setting passSetting;

        public VolumetricLightRenderPass(Setting setting)
        {
            volumetricHandle.Init("_Volumetric");
            lowResDepthHandle.Init("_LowResDepth");
            compositingHandle.Init("_Compositing");
            shaderTagId = new ShaderTagId("UniversalForward");
            dualKawaseIds = null;
            passSetting = setting;

            Shader volumetricLightShader = Shader.Find("Urp/UrpVolumetricLightShader");
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
                    volumetricLightMaterial.SetFloat("_Extinction", passSetting.extinction * passSetting.extinction);
                    volumetricLightMaterial.SetFloat("_Absorbtion", passSetting.absorbtion);
                    volumetricLightMaterial.SetFloat("_Offset", passSetting.dualKawase.offset);

                    // ray marching
                    cmd.Blit(currentTarget, volumetricHandle.Identifier(), volumetricLightMaterial, 0);
                    // dual kaease
                    if (dualKawaseIds != null)
                    {
                        RenderTargetIdentifier cur = volumetricHandle.Identifier();
                        // down sample
                        for (int i = 0; i < dualKawaseIds.Length; ++i)
                        {
                            cmd.Blit(cur, dualKawaseIds[i], volumetricLightMaterial, 1);
                            cur = dualKawaseIds[i];
                        }
                        // up sample
                        for (int i = dualKawaseIds.Length - 2; 0 <= i; --i)
                        {
                            cmd.Blit(cur, dualKawaseIds[i], volumetricLightMaterial, 2);
                            cur = dualKawaseIds[i];
                        }
                        cmd.Blit(dualKawaseIds[0], volumetricHandle.Identifier(), volumetricLightMaterial, 2);
                    }

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

            createDualKawase(cmd, descriptor1.width, descriptor1.height);

            RenderTextureDescriptor descriptor2 = cameraTextureDescriptor;
            descriptor2.msaaSamples = 1;

            cmd.GetTemporaryRT(volumetricHandle.id, descriptor1, FilterMode.Bilinear);
            ConfigureTarget(volumetricHandle.Identifier());
            cmd.GetTemporaryRT(lowResDepthHandle.id, descriptor1, FilterMode.Bilinear);
            ConfigureTarget(lowResDepthHandle.Identifier());
            cmd.GetTemporaryRT(compositingHandle.id, descriptor2, FilterMode.Bilinear);
            ConfigureTarget(compositingHandle.Identifier());

            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(volumetricHandle.id);
            cmd.ReleaseTemporaryRT(lowResDepthHandle.id);
            cmd.ReleaseTemporaryRT(compositingHandle.id);

            releaseDualKawase(cmd);
        }
        
        private void createDualKawase(CommandBuffer cmd, int width, int height)
        {
            int dualKawaseCount = 0;

            const int BOUND = 128;

            int w = Mathf.CeilToInt(width * passSetting.dualKawase.resolutionScale);
            int h = Mathf.CeilToInt(height * passSetting.dualKawase.resolutionScale);

            while (BOUND <= w && BOUND <= h)
            {
                ++dualKawaseCount;

                w = Mathf.CeilToInt(w * passSetting.dualKawase.resolutionScale);
                h = Mathf.CeilToInt(h * passSetting.dualKawase.resolutionScale);
            }

            dualKawaseCount = Mathf.Min(dualKawaseCount, passSetting.dualKawase.maxSample);

            if (0 < dualKawaseCount)
            {
                dualKawaseIds = new int[dualKawaseCount];
                for(int i = 0; i < dualKawaseCount; ++i)
                {
                    width = Mathf.CeilToInt(width * passSetting.dualKawase.resolutionScale);
                    height = Mathf.CeilToInt(height * passSetting.dualKawase.resolutionScale);
                    dualKawaseIds[i] = Shader.PropertyToID(string.Format("_dualKawase{0}",i));
                    cmd.GetTemporaryRT(dualKawaseIds[i], width, height, 0, FilterMode.Bilinear, RenderTextureFormat.R16);
                }
            }
        }

        private void releaseDualKawase(CommandBuffer cmd)
        {
            if (dualKawaseIds == null) return;

            for (int i = 0; i < dualKawaseIds.Length; ++i)
            {
                cmd.ReleaseTemporaryRT(dualKawaseIds[i]);
            }

            dualKawaseIds = null;
        }
    }

    VolumetricLightRenderPass m_ScriptablePass;

    [System.Serializable]
    public class Setting
    {
        [Range(-1, 1)]
        public float scatter = -0.4f;

        [Range(1, 50)]
        public int step = 25;

        public float maxDistance = 75f;

        public float jitter = 250f;

        [Range(0.1f, 3f)]
        public float intensity = 2.53f;

        [Range(0, 1)]
        public float extinction = 0.5f;

        [Range(0, 1)]
        public float absorbtion = 0.25f;

        public enum DownSample { off = 1, half = 2, third = 3, quarter = 4 };
        public DownSample downsampling = DownSample.off;

        [System.Serializable]
        public class DualKawase
        {
            [Range(0.4f, 0.9f)]
            public float resolutionScale;

            [Range(1, 8)]
            public int offset;

            [Range(1, 8)]
            public int maxSample;

            public DualKawase()
            {
                resolutionScale = 0.5f;
                offset = 1;
                maxSample = 2;
            }
        }
        public DualKawase dualKawase = new DualKawase();
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


