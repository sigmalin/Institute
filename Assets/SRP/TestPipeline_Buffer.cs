using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class TestPipeline
{
    void CreateRenderBuffer(ScriptableRenderContext context)
    {
        int bufferWidth = Screen.width;
        int bufferHeight = Screen.height;
        
#if !UNITY_EDITOR
        bufferWidth >>= 1;
        bufferHeight >>= 1;
#endif

        CmdBuff.GetTemporaryRT(RenderBuffer1ID, bufferWidth, bufferHeight, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR);
        CmdBuff.GetTemporaryRT(RenderBuffer2ID, bufferWidth, bufferHeight, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR);
        CmdBuff.GetTemporaryRT(DepthBufferID, bufferWidth, bufferHeight, 16, FilterMode.Point, RenderTextureFormat.Depth);
        CmdBuff.GetTemporaryRT(ShadowBufferID, (int)shadowMapSize, (int)shadowMapSize, 16, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        
        if (renderBuffers == null) renderBuffers = new RenderTargetIdentifier[4];
        renderBuffers[0] = new RenderTargetIdentifier(RenderBuffer1ID);
        renderBuffers[1] = new RenderTargetIdentifier(RenderBuffer2ID);
        renderBuffers[2] = new RenderTargetIdentifier(DepthBufferID);
        renderBuffers[3] = new RenderTargetIdentifier(ShadowBufferID);

        context.ExecuteCommandBuffer(CmdBuff);
        CmdBuff.Clear();
    }

    void ReleaseRenderBuffer(ScriptableRenderContext context)
    {
        CmdBuff.ReleaseTemporaryRT(RenderBuffer1ID);
        CmdBuff.ReleaseTemporaryRT(RenderBuffer2ID);
        CmdBuff.ReleaseTemporaryRT(DepthBufferID);
        CmdBuff.ReleaseTemporaryRT(ShadowBufferID);
        
        context.ExecuteCommandBuffer(CmdBuff);
        CmdBuff.Clear();
    }

    void GetClearFlags(Camera camera, out bool isClearColor, out bool isClearDepth)
    {
        isClearColor = true;
        isClearDepth = true;

        CameraClearFlags clearFlags = camera.clearFlags;

        switch (clearFlags)
        {
            case CameraClearFlags.Depth:
                isClearColor = false;
                break;

            case CameraClearFlags.Nothing:
                isClearDepth = false;
                isClearColor = false;
                break;
        }
    }

    void ClearRenderBuffer(ScriptableRenderContext context, Camera camera)
    {
        bool isClearColor, isClearDepth;

        GetClearFlags(camera, out isClearColor, out isClearDepth);

        CmdBuff.ClearRenderTarget(isClearDepth, isClearColor, camera.backgroundColor);
                
        context.ExecuteCommandBuffer(CmdBuff);
        CmdBuff.Clear();
    }

    void SetRenderBuffer(ScriptableRenderContext context, Camera camera)
    {
        bool isClearColor, isClearDepth;

        GetClearFlags(camera, out isClearColor, out isClearDepth);

        //if (camera.cameraType == CameraType.Game)
        {
            CmdBuff.SetRenderTarget(renderBuffers[0], renderBuffers[2]);
            CmdBuff.ClearRenderTarget(isClearDepth, isClearColor, camera.backgroundColor);
        }

        context.ExecuteCommandBuffer(CmdBuff);
        CmdBuff.Clear();
    }

    void Flip(ScriptableRenderContext context, Camera camera)
    {
        //if (camera.cameraType == CameraType.Game)
        {
            ApplyBloom(CmdBuff, renderBuffers[0]);
            ApplyToneMapping(CmdBuff, renderBuffers[0], renderBuffers[1]);
            ApplyFXAA(CmdBuff, renderBuffers[1], renderBuffers[0]);            
            CmdBuff.Blit(renderBuffers[0], BuiltinRenderTextureType.CameraTarget);
        }
    }
}
