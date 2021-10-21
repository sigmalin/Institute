using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RayMarchingRenderPassFeature : ScriptableRendererFeature
{
    class RayMarchingRenderPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "RayMarchingRenderPass";

        static readonly int ClipToWorldId = Shader.PropertyToID("_ClipToWorld");

        RenderTargetIdentifier currentTarget;

        Material material;

        public void Setup(Material _material, in RenderTargetIdentifier renderTarget)
        {
            material = _material;
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
            if (!material) return;

            ProfilingSampler sampler = new ProfilingSampler("RayMarchingRenderPass");

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);

            using (new ProfilingScope(cmd, sampler))
            {
                SetClipMatrix(ref renderingData);
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
            cmd.Blit(null, currentTarget, material);
        }

        void SetClipMatrix(ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;

            Matrix4x4 proj = GL.GetGPUProjectionMatrix(cameraData.camera.projectionMatrix, false);

            proj[2, 3] = proj[3, 2] = 0.0f;
            proj[3, 3] = 1.0f;

            Matrix4x4 view = cameraData.camera.worldToCameraMatrix;

            Matrix4x4 clip = Matrix4x4.Inverse(proj * view)
             * Matrix4x4.TRS(new Vector3(0, 0, -proj[2, 2]), Quaternion.identity, Vector3.one);

            material.SetMatrix(ClipToWorldId, clip);
        }
    }

    RayMarchingRenderPass m_ScriptablePass;

    public Material matRayMarching;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new RayMarchingRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(matRayMarching, renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


