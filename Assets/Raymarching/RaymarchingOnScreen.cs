using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RaymarchingOnScreen : MonoBehaviour
{
    CommandBuffer mCmd;

    public Material mDrawer;

    Material mBlit;
    RenderTexture mCloud;
    RenderTexture mColorBuffer;
    RenderTexture mDepthBuffer;

    int mBufferWidth = 0;
    int mBufferHeight = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (mDrawer == null) return;

        if (mBlit == null)
        {
            mBlit = new Material(Shader.Find("Raymarching/Blit/BlitRaymarching"));

            mBlit.hideFlags = HideFlags.HideAndDontSave;
        }

        mCmd = new CommandBuffer();
        mCmd.name = "Raymarching";

        Camera.main.AddCommandBuffer(CameraEvent.AfterImageEffects, mCmd);

        SetCommandBuffer(mCmd);
    }

    void OnDestroy()
    {
        if(Camera.main != null) Camera.main.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, mCmd);

        ClearAllRT();

        if (mCmd != null)
        {
            mCmd.Clear();
            mCmd.Release();
            mCmd = null;
        }        
    }

    void LateUpdate()
    {
        if (Camera.main == null || mDrawer == null) return;

        // https://gamedev.stackexchange.com/questions/131978/shader-reconstructing-position-from-depth-in-vr-through-projection-matrix
        Matrix4x4 proj = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, true);
        //Matrix4x4 proj = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false);

        proj[2, 3] = proj[3, 2] = 0.0f;
        proj[3, 3] = 1.0f;

        Matrix4x4 view = Camera.main.worldToCameraMatrix;

        Matrix4x4 clip = Matrix4x4.Inverse(proj * view)
             * Matrix4x4.TRS(new Vector3(0, 0, -proj[2, 2]), Quaternion.identity, Vector3.one);

        mDrawer.SetMatrix("_ClipToWorld", clip);
    }

    void FixedUpdate()
    {
        if(mBufferWidth != Screen.width ||
           mBufferHeight != Screen.height)
        {
            mCmd.Clear();

            SetCommandBuffer(mCmd);
        }
    }

    void SetCommandBuffer(CommandBuffer _cmd)
    {
        if (Camera.main == null || _cmd == null) return;

        Camera.main.depthTextureMode = DepthTextureMode.None;

        ClearAllRT();

        mBufferWidth = Screen.width;
        mBufferHeight = Screen.height;

        mColorBuffer = new RenderTexture(mBufferWidth, mBufferHeight, 0);
        mColorBuffer.name = "Color Buffer";
        mDepthBuffer = new RenderTexture(mBufferWidth, mBufferHeight, 24, RenderTextureFormat.Depth);
        mDepthBuffer.name = "Depth Buffer";

        Camera.main.SetTargetBuffers(mColorBuffer.colorBuffer, mDepthBuffer.depthBuffer);
        mDrawer.SetTexture("_DepthTexture", mDepthBuffer);

        mCloud = new RenderTexture(mBufferWidth >> 2, mBufferHeight >> 2, 0);
        mCloud.name = "Cloud";

        mBlit.SetTexture("_BackGround", mColorBuffer);
        mBlit.SetTexture("_Raymarching", mCloud);

        _cmd.SetRenderTarget(mCloud);
        _cmd.ClearRenderTarget(true, true, Color.clear);
        _cmd.Blit(null, BuiltinRenderTextureType.CurrentActive, mDrawer);
        _cmd.Blit(null, BuiltinRenderTextureType.CameraTarget, mBlit);
    }

    void ClearAllRT()
    {
        if (mCloud != null)
        {
            GameObject.Destroy(mCloud);
            mCloud = null;
        }

        if (mColorBuffer != null)
        {
            GameObject.Destroy(mColorBuffer);
            mColorBuffer = null;
        }

        if (mDepthBuffer != null)
        {
            GameObject.Destroy(mDepthBuffer);
            mDepthBuffer = null;
        }
    }
}
