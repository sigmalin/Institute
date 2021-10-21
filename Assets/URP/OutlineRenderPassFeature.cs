using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineRenderPassFeature : ScriptableRendererFeature
{
    class OutlineRenderPass : ScriptableRenderPass
    {
        static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
        static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        static readonly int AspectId = Shader.PropertyToID("_Aspect");

        protected ShaderTagId shaderTagId;
        protected Setting passSetting;
        protected Material outlineMaterial;

        public OutlineRenderPass(Setting setting)
        {
            shaderTagId = new ShaderTagId("UniversalForward");
            passSetting = setting;

            Shader shader = Shader.Find("MyUrp/UrpOutline");
            if (shader)
            {
                outlineMaterial = new Material(shader);
            }
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
            if (!outlineMaterial) return;

            SortingSettings sortingSettings = new SortingSettings(renderingData.cameraData.camera);
            sortingSettings.criteria = SortingCriteria.OptimizeStateChanges;       // Better for SRP Batcher.

            DrawingSettings drawingSettings = new DrawingSettings();
            drawingSettings.SetShaderPassName(0, shaderTagId);
            drawingSettings.sortingSettings = sortingSettings;
            drawingSettings.enableInstancing = true;

            drawingSettings.overrideMaterial = outlineMaterial;
            drawingSettings.overrideMaterialPassIndex = 0;

            RenderQueueRange range = new RenderQueueRange();
            range.lowerBound = passSetting.queueMin;
            range.upperBound = passSetting.queueMax;

            FilteringSettings filteringSettings = new FilteringSettings();
            filteringSettings.renderQueueRange = range;
            filteringSettings.renderingLayerMask = 1;
            filteringSettings.layerMask = passSetting.layer;

            outlineMaterial.SetFloat(OutlineWidthId, passSetting.outlineWidth);
            outlineMaterial.SetColor(OutlineColorId, passSetting.outlineColor);
            outlineMaterial.SetFloat(AspectId, renderingData.cameraData.camera.aspect);

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    OutlineRenderPass m_ScriptablePass;

    [System.Serializable]
    public class Setting
    {
        [Range(1000, 5000)]
        public int queueMin = 1000;
        [Range(1000, 5000)]
        public int queueMax = 2500;
        [Range(0.01f, 1f)]
        public float outlineWidth = 0.24f;

        public Color outlineColor;

        public LayerMask layer;
    }

    public Setting setting = new Setting();

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new OutlineRenderPass(setting);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


