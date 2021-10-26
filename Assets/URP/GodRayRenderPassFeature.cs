using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GodRayRenderPassFeature : ScriptableRendererFeature
{
    class GodRayRenderPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "GodRayRenderPass";

        RenderTargetIdentifier currentTarget;

        protected ShaderTagId shaderTagId;

        protected RenderTargetHandle occluderTexHandle;

        protected Material godRayMaterial;

        protected Setting passSetting;

        public GodRayRenderPass(Setting setting)
        {
            occluderTexHandle.Init("_OccluderTexture");
            shaderTagId = new ShaderTagId("UniversalForward");
            passSetting = setting;

            Shader godRayShader = Shader.Find("Urp/UrpGodRayShader");
            if (godRayShader)
            {
                godRayMaterial = new Material(godRayShader);
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
            if (!godRayMaterial)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);

            using (new ProfilingScope(cmd, new ProfilingSampler(k_RenderTag)))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                RenderOcculder(context, ref renderingData);

                RenderRadiusBlur(cmd, context, ref renderingData);
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
            descriptor.msaaSamples = 1;

            descriptor.width = Mathf.RoundToInt(descriptor.width * passSetting.resolutionScale);
            descriptor.height = Mathf.RoundToInt(descriptor.height * passSetting.resolutionScale);

            cmd.GetTemporaryRT(occluderTexHandle.id, descriptor, FilterMode.Bilinear);
            ConfigureTarget(occluderTexHandle.Identifier());
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(occluderTexHandle.id);
        }

        private void RenderOcculder(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Camera camera = renderingData.cameraData.camera;

            SortingSettings sortingSettings = new SortingSettings(renderingData.cameraData.camera);
            sortingSettings.criteria = SortingCriteria.CommonOpaque;

            DrawingSettings drawingSettings = new DrawingSettings();
            drawingSettings.SetShaderPassName(0, shaderTagId);
            drawingSettings.sortingSettings = sortingSettings;
            drawingSettings.enableInstancing = true;

            drawingSettings.overrideMaterial = godRayMaterial;
            drawingSettings.overrideMaterialPassIndex = 0;

            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            filteringSettings.layerMask = passSetting.layer;

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            context.DrawSkybox(camera);
        }

        private void RenderRadiusBlur(CommandBuffer cmd, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Camera camera = renderingData.cameraData.camera;

            Vector3 sunDirectionWorldSpace = RenderSettings.sun.transform.forward;
            
            Vector3 cameraPositionWorldSpace = camera.transform.position;
            
            Vector3 sunPositionWorldSpace = cameraPositionWorldSpace - sunDirectionWorldSpace;
            
            Vector3 sunPositionViewportSpace = camera.WorldToViewportPoint(sunPositionWorldSpace);

            godRayMaterial.SetVector("_Center", new Vector4(sunPositionViewportSpace.x, sunPositionViewportSpace.y, 0, 0));
            godRayMaterial.SetFloat("_Intensity", passSetting.intensity);
            godRayMaterial.SetFloat("_BlurWidth", passSetting.blurWidth);

            cmd.Blit(occluderTexHandle.Identifier(), currentTarget, godRayMaterial, 1);
        }
    }

    GodRayRenderPass m_ScriptablePass;

    [System.Serializable]
    public class Setting
    {
        public LayerMask layer;

        [Range(0.1f, 1f)]
        public float resolutionScale = 0.5f;

        [Range(0.0f, 1.0f)]
        public float intensity = 1.0f;

        [Range(0.0f, 1.0f)]
        public float blurWidth = 0.85f;
    }

    public Setting setting = new Setting();

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new GodRayRenderPass(setting);

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


