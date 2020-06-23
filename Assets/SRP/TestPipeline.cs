using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class TestPipeline : UnityEngine.Rendering.RenderPipeline
{
    public enum RenderFlow
    {
        Default,
        Custom,
    }

    TestPipelineAsset.Settings setting;

    int shadowMapSize;
    float invShadowMapSize;

    public TestPipeline(TestPipelineAsset.Settings _setting)
    {
        setting = _setting;

        GraphicsSettings.useScriptableRenderPipelineBatching = setting.useSRPBatcher;
        
        shadowMapSize = (int)setting.shadow.atlasSize;

        invShadowMapSize = 1f / shadowMapSize;

        Initialize();
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        CreateRenderBuffer(context);

        for (int i = 0; i < cameras.Length; ++i)
            Render(context, cameras[i]);

        ReleaseRenderBuffer(context);

        context.Submit();      
    }

    void Render(ScriptableRenderContext context, Camera camera)
    {
        TestPipelineCamera testPipelineCamera = camera.GetComponent<TestPipelineCamera>();
        RenderFlow flow = testPipelineCamera == null ? RenderFlow.Default : testPipelineCamera.RenderFlow;

        switch(flow)
        {
            case RenderFlow.Custom:
                CustomRenderFlow(context, camera);
                break;

            default:
                DefaultRenderFlow(context, camera);
                break;
        }
    }

    void DefaultRenderFlow(ScriptableRenderContext context, Camera camera)
    {
        ScriptableCullingParameters cullingParameters;
        if (!camera.TryGetCullingParameters(out cullingParameters))
            return;

#if UNITY_EDITOR
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
#endif

        cullingParameters.shadowDistance = Mathf.Min(setting.shadow.maxDistance, camera.farClipPlane);

        CmdBuff.BeginSample(camera.name);

        cullingResults = context.Cull(ref cullingParameters);

        SetShadow(context, camera);

        SetCamera(context, camera);

        ClearRenderBuffer(context, camera);

        SetLighting(context, camera);

        DrawDepthOnly(context, camera);

        DrawOpaque(context, camera);

        DrawSky(context, camera);

#if UNITY_EDITOR
        if (UnityEditor.Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
        }
#endif

#if UNITY_EDITOR
        if (UnityEditor.Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
#endif

        CmdBuff.EndSample(camera.name);
        context.ExecuteCommandBuffer(CmdBuff);
        CmdBuff.Clear();
    }

    void CustomRenderFlow(ScriptableRenderContext context, Camera camera)
    {
        ScriptableCullingParameters cullingParameters;
        if (!camera.TryGetCullingParameters(out cullingParameters))
            return;
        
        cullingParameters.shadowDistance = Mathf.Min(setting.shadow.maxDistance, camera.farClipPlane);

        CmdBuff.BeginSample(camera.name);

        cullingResults = context.Cull(ref cullingParameters);

        SetShadow(context, camera);

        SetCamera(context, camera);

        ClearRenderBuffer(context, camera);

        SetRenderBuffer(context, camera);

        SetLighting(context, camera);

        DrawDepthOnly(context, camera);

        DrawOpaque(context, camera);

        DrawSky(context, camera);

        Flip(context, camera);
        
        CmdBuff.EndSample(camera.name);
        context.ExecuteCommandBuffer(CmdBuff);
        CmdBuff.Clear();
    }
}
