using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RaymarchingOnSceneView : MonoBehaviour
{
#if UNITY_EDITOR
    Camera mCam;

    CommandBuffer mCmd;

    UnityEditor.SceneView mView;

    const int DATA_COUNT = 4;

    Transform mParent;

    Texture2D mRaymarchingData;
    Transform[] mRaymarchingTrans;

    public Material mDrawer;

    // Start is called before the first frame update
    void Start()
    {
        if (mDrawer == null) return;

        InitRaymarchingData();
        UpdateRaymarchingData();

        mCmd = new CommandBuffer();
        mCmd.name = "Raymarching";
        mCmd.Blit(null, BuiltinRenderTextureType.CurrentActive, mDrawer);

        mView = UnityEditor.SceneView.lastActiveSceneView;
        mCam = mView.camera;
        mCam.AddCommandBuffer(CameraEvent.AfterImageEffects, mCmd);

        UnityEditor.SceneView.onSceneGUIDelegate += SceneUpdate;
    }

    void OnDestroy()
    {
        if (mCam != null) mCam.RemoveCommandBuffer(CameraEvent.AfterImageEffects, mCmd);
        
        if (mCmd != null)
        {
            mCmd.Clear();
            mCmd.Release();
            mCmd = null;
        }

        if (mRaymarchingData != null)
        {
            GameObject.Destroy(mRaymarchingData);
            mRaymarchingData = null;
        }

        if (mRaymarchingTrans != null)
        {
            for(int i = 0; i < mRaymarchingTrans.Length; ++i)
            {
                if (mRaymarchingTrans[i] == null) continue;

                GameObject.Destroy(mRaymarchingTrans[i].gameObject);
                mRaymarchingTrans[i] = null;
            }

            mRaymarchingTrans = null;
        }
    }

    void SceneUpdate(UnityEditor.SceneView _view)
    {
        if (mView != _view) return;

        UpdateRaymarchingData();

        // https://gamedev.stackexchange.com/questions/131978/shader-reconstructing-position-from-depth-in-vr-through-projection-matrix
        Matrix4x4 proj = GL.GetGPUProjectionMatrix(mCam.projectionMatrix, true);
        //Matrix4x4 proj = GL.GetGPUProjectionMatrix(mCam.projectionMatrix, false);

        proj[2, 3] = proj[3, 2] = 0.0f;
        proj[3, 3] = 1.0f;

        Matrix4x4 view = mCam.worldToCameraMatrix;

        Matrix4x4 clip = Matrix4x4.Inverse(proj * view)
             * Matrix4x4.TRS(new Vector3(0, 0, -proj[2, 2]), Quaternion.identity, Vector3.one);

        mDrawer.SetMatrix("_ClipToWorld", clip);
    }

    void InitRaymarchingData()
    {
        if (mRaymarchingData != null) return;

        if (mParent == null)
        {
            mParent = new GameObject("Root").transform;
            mParent.position = Vector3.zero;
            mParent.localScale = Vector3.one;
            mParent.rotation = Quaternion.identity;
        }

        int count = Mathf.Max(1, DATA_COUNT);

        mRaymarchingTrans = new Transform[count];
        for(int i = 0; i < count; ++i)
        {
            mRaymarchingTrans[i] = new GameObject(i.ToString()).transform;            
            mRaymarchingTrans[i].SetParent(mParent);

            mRaymarchingTrans[i].localScale = Vector3.one;
            mRaymarchingTrans[i].position = Vector3.zero;
        }

        mRaymarchingData = new Texture2D(count, 1, TextureFormat.RGBAFloat, false);
        mRaymarchingData.filterMode = FilterMode.Point;
        mRaymarchingData.wrapMode = TextureWrapMode.Clamp;

        mDrawer.SetInt("_DataCount", count);
        mDrawer.SetFloat("_DataIteration", 1f / count);
    }

    void UpdateRaymarchingData()
    {
        if (mRaymarchingData == null) return;

        int count = Mathf.Max(1, DATA_COUNT);

        Color[] cols = mRaymarchingData.GetPixels();

        for (int i = 0; i < count; ++i)
        {
            Vector3 scale = mRaymarchingTrans[i].localScale;
            cols[i].r = mRaymarchingTrans[i].position.x;
            cols[i].g = mRaymarchingTrans[i].position.y;
            cols[i].b = mRaymarchingTrans[i].position.z;
            cols[i].a = Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
        }

        mRaymarchingData.SetPixels(cols);

        mRaymarchingData.Apply(false, false);

        mDrawer.SetTexture("_DataTex", mRaymarchingData);
    }
#endif
}
