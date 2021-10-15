using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class DrawCard
{
    Mesh mCardBack;
    Mesh mCardFront;
    Material mMatCardBack;
    Material mMatCardFront;
    CommandBuffer mCmd;

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct CardVertex
    {
        public Vector3 pos;
        public Vector2 uv;
    }

    CardVertex[] _vertices;
    ushort[] _indices;
    VertexAttributeDescriptor[] _layouts;

    Vector2 texSize;
    Vector2 halfSize;

    public bool isFlop { set; get; }

    public DrawCard()
    {
        mCardBack = new Mesh();
        mCardBack.MarkDynamic();

        mCardFront = new Mesh();
        mCardFront.MarkDynamic();

        mMatCardBack = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/matCard"));
        mMatCardFront = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/matJoker"));

        mCmd = new CommandBuffer();
        mCmd.name = "DrawCard";

        Camera.main.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, mCmd);

        _vertices = new CardVertex[8];
        _indices = new ushort[]
        {
            0,1,2,
            2,3,0,
            2,4,3,
        };

        _layouts = new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        };

        isFlop = false;
    }

    public void Release()
    {
        if (mCardBack != null)
        {
            mCardBack.Clear();
            mCardBack = null;
        }

        if (mCardFront != null)
        {
            mCardFront.Clear();
            mCardFront = null;
        }

        if (mMatCardBack != null)
        {
            GameObject.Destroy(mMatCardBack);
            mMatCardBack = null;
        }

        if (mMatCardFront != null)
        {
            GameObject.Destroy(mMatCardFront);
            mMatCardFront = null;
        }

        if (mCmd != null)
        {
            if (Camera.main != null)
            {
                Camera.main.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, mCmd);
            }
            mCmd.Release();
            mCmd = null;
        }

        if (_vertices != null)
        {
            _vertices = null;
        }

        if (_indices != null)
        {
            _indices = null;
        }

        if (_layouts != null)
        {
            _layouts = null;
        }
    }

    #region render
    MeshUpdateFlags GetMeshUpdateFlags()
    {
        return MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds |
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers;
    }

    public void Flip(Vector2 worldSize, Vector2 dragDir)
    {
        UpdateTexSize(worldSize);

        if(isFlop == true)
        {
            FlopAfter();
        }
        else
        {
            FlopBefore(worldSize, dragDir);
        }
    }

    public void Render()
    {
        DrawBegin();

        DrawMesh();

        DrawEnd();
    }

    void UpdateTexSize(Vector2 worldSize)
    {
        texSize.x = 560f;
        texSize.y = 770f;

        while (worldSize.x < texSize.x || worldSize.y < texSize.y)
        {
            texSize.x *= 0.5f;
            texSize.y *= 0.5f;
        }

        halfSize.x = texSize.x * 0.5f;
        halfSize.y = texSize.y * 0.5f;

        mCardBack.bounds = new Bounds(Vector3.zero, new Vector3(halfSize.x, halfSize.y, 0));
        mCardFront.bounds = new Bounds(Vector3.zero, new Vector3(halfSize.x, halfSize.y, 0));
    }

    void DrawBegin()
    {
        mCmd.Clear();
        mCmd.SetViewProjectionMatrices(Camera.main.worldToCameraMatrix, Camera.main.projectionMatrix);
    }

    void DrawMesh()
    {
        mCmd.DrawMesh(mCardBack, Matrix4x4.identity, mMatCardBack);
        mCmd.DrawMesh(mCardFront, Matrix4x4.identity, mMatCardFront);
    }

    void DrawEnd()
    {
        //Graphics.ExecuteCommandBuffer(_cmd);
    }
    #endregion
}
