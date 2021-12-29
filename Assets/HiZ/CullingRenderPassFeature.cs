using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CullingRenderPassFeature : ScriptableRendererFeature
{
    class CullingRenderPass : ScriptableRenderPass
    {
        Vector2 texSize;

        ComputeBuffer postionBuffer;
        ComputeBuffer cullingResultBuffer;

        Queue<IHizCullingObject> objectQueue;

        protected Setting passSetting;

        public CullingRenderPass(Queue<IHizCullingObject> _queue, Setting _setting)
        {
            postionBuffer = null;

            cullingResultBuffer = null;

            objectQueue = _queue;

            passSetting = _setting;
        }

        private bool isValid()
        {
            return SystemInfo.supportsComputeShaders && passSetting.computeShader;
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
            if (isValid())
            {
                CommandBuffer cmd = CommandBufferPool.Get();

                var etor = objectQueue.GetEnumerator();

                try
                {
                    while (etor.MoveNext())
                    {
                        if (fillBuffer(passSetting.computeShader, etor.Current) == true)
                        {
                            Process(cmd, passSetting.computeShader, renderingData.cameraData.camera, etor.Current);
                            context.ExecuteCommandBuffer(cmd);
                            cmd.Clear();
                        }

                        releaseBuffer();
                    }
                }
                catch
                {

                }

                etor.Dispose();

                CommandBufferPool.Release(cmd);
            }

            objectQueue.Clear();
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            texSize.x = cameraTextureDescriptor.width;
            texSize.y = cameraTextureDescriptor.height;
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
        }

        bool fillBuffer(ComputeShader _cs, IHizCullingObject _obj)
        {
            if (_obj == null) return false;

            return _obj.getBuffers(out postionBuffer, out cullingResultBuffer);
        }

        void releaseBuffer()
        {
            postionBuffer = null;

            cullingResultBuffer = null;
        }

        void Process(CommandBuffer _cmd, ComputeShader _cs, Camera _camera, IHizCullingObject _obj)
        {
            if(_camera == Camera.main)
            {
                ProcessMainCamera(_cmd, _cs, _camera, _obj);
            }
            else 
            {
                ProcessOtherCamera(_cmd, _cs, _camera, _obj);
            }
        }

        void ProcessMainCamera(CommandBuffer _cmd, ComputeShader _cs, Camera _camera, IHizCullingObject _obj)
        {
            Texture hiz = Shader.GetGlobalTexture(Shader.PropertyToID("_HiZTexture"));
            if (hiz == null) return;

            int kanel = _cs.FindKernel("CSCulling");

            uint sizeX;
            _cs.GetKernelThreadGroupSizes(
                kanel,
                out sizeX,
                out _,
                out _
            );

            int count = postionBuffer.count;

            cullingResultBuffer.SetCounterValue(0);

            _cs.SetFloat(Shader.PropertyToID("_ObjectRadius"), _obj.getRadius());
            _cs.SetInt(Shader.PropertyToID("_ObjectCount"), count);

            _cs.SetVector(Shader.PropertyToID("cameraDir"), _camera.transform.forward);
            _cs.SetFloat(Shader.PropertyToID("tanHalfFov"), Mathf.Tan(Mathf.PI * _camera.fieldOfView / 360f));

            var viewProj = GL.GetGPUProjectionMatrix(_camera.projectionMatrix, false) * _camera.worldToCameraMatrix;
            _cs.SetMatrix(Shader.PropertyToID("matrixVP"), viewProj);

            _cs.SetTexture(kanel, Shader.PropertyToID("_HiZTexture"), hiz);

            _cs.SetVector(Shader.PropertyToID("_RT_Size"), texSize);
            _cs.SetInt(Shader.PropertyToID("_MaxMipLevel"), 6);

            _cs.SetBuffer(kanel, Shader.PropertyToID("postionBuffer"), postionBuffer);
            _cs.SetBuffer(kanel, Shader.PropertyToID("cullingResult"), cullingResultBuffer);

            _cs.Dispatch(kanel,
                                Mathf.CeilToInt((count + sizeX - 1) / sizeX),
                                1, 1);
            
            _obj.onRender(_cmd);
        }

        void ProcessOtherCamera(CommandBuffer _cmd, ComputeShader _cs, Camera _camera, IHizCullingObject _obj)
        {
            _obj.onRender(_cmd);
        }
    }

    CullingRenderPass m_ScriptablePass;

    Queue<IHizCullingObject> m_HizCullingQueue;

    public static CullingRenderPassFeature Instance { private set; get; }

    [System.Serializable]
    public class Setting
    {
        public ComputeShader computeShader;
    }

    public Setting setting = new Setting();

    /// <inheritdoc/>
    public override void Create()
    {
        CullingRenderPassFeature.Instance = this;

        m_HizCullingQueue = new Queue<IHizCullingObject>();

        m_ScriptablePass = new CullingRenderPass(m_HizCullingQueue, setting);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }

    public void Register(IHizCullingObject _object)
    {
        if (m_HizCullingQueue != null)
        {
            m_HizCullingQueue.Enqueue(_object);
        }
    }
}

