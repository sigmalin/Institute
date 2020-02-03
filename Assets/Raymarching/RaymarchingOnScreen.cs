using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RaymarchingOnScreen : MonoBehaviour
{
    CommandBuffer mCmd;

    Material mDrawer;

    // Start is called before the first frame update
    void Start()
    {
        mDrawer = new Material(Shader.Find("Raymarching/Screen/Shadow"));
        
        mDrawer.hideFlags = HideFlags.HideAndDontSave;

        mCmd = new CommandBuffer();
        mCmd.name = "Raymarching";

        mCmd.Blit(null, BuiltinRenderTextureType.CurrentActive, mDrawer);

        Camera.main.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, mCmd);
    }

    void OnDestroy()
    {
        if(Camera.main != null) Camera.main.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, mCmd);

        if(mCmd != null)
        {
            mCmd.Clear();
            mCmd.Release();
            mCmd = null;
        }        
    }

    void LateUpdate()
    {
        if (Camera.main == null) return;

        // https://gamedev.stackexchange.com/questions/131978/shader-reconstructing-position-from-depth-in-vr-through-projection-matrix
        Matrix4x4 proj = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, true);

        proj[2, 3] = proj[3, 2] = 0.0f;
        proj[3, 3] = 1.0f;

        Matrix4x4 view = Camera.main.worldToCameraMatrix;

        Matrix4x4 clip = Matrix4x4.Inverse(proj * view)
           * Matrix4x4.TRS(new Vector3(0, 0, -proj[2, 2]), Quaternion.identity, Vector3.one);

        mDrawer.SetMatrix("_ClipToWorld", clip);
    }
}
