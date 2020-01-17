using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FourierCompute
{
    Mesh mMesh;

    Matrix4x4 matView, matProj;

    CommandBuffer mCmd;

    public void Init(int _fourierSize)
    {
        CreateMesh(_fourierSize);

        CalcMatrix(_fourierSize);

        CreateCmd();
    }

    void CreateMesh(int _fourierSize)
    {
        if (mMesh == null) mMesh = new Mesh();

        mMesh.Clear();

        int FourierSizePlusOne = _fourierSize + 1;
        int FourierSizeMinusOne = _fourierSize - 1;

        List<Vector3> vertices = new List<Vector3>(FourierSizePlusOne * FourierSizePlusOne);
        List<Vector2> uvs = new List<Vector2>(FourierSizePlusOne * FourierSizePlusOne);
        List<int> indices = new List<int>(FourierSizePlusOne * FourierSizePlusOne * 6);

        for (int y = 0; y < FourierSizePlusOne; ++y)
        {
            for (int x = 0; x < FourierSizePlusOne; ++x)
            {
                vertices.Add(new Vector3(x,y,0f));

                Vector2 uv = new Vector2(x, y);

                if (_fourierSize == uv.x) uv.x = 0f;
                if (_fourierSize == uv.y) uv.y = 0f;

                uvs.Add(uv);
            }
        }       

        for (int y = 0; y < _fourierSize; ++y)
        {
            for (int x = 0; x < _fourierSize; ++x)
            {
                indices.Add(x + y * FourierSizePlusOne);
                indices.Add(x + (y+1) * FourierSizePlusOne);
                indices.Add(x + 1 + y * FourierSizePlusOne);
                indices.Add(x + 1 + (y + 1) * FourierSizePlusOne);
                indices.Add(x + 1 + y * FourierSizePlusOne);
                indices.Add(x + (y + 1) * FourierSizePlusOne);
            }
        }

        mMesh.SetVertices(vertices);
        mMesh.SetUVs(0, uvs);
        mMesh.SetTriangles(indices, 0);
        mMesh.UploadMeshData(true);
    }

    void CalcMatrix(int _fourierSize)
    {
        float halfFourierSize = _fourierSize * 0.5f;

        Vector3 lookat = new Vector3(halfFourierSize, halfFourierSize, 0f);
        Vector3 dir = new Vector3(0f, 0f, 1f);
        Vector3 pos = lookat - dir;

        matView = Matrix4x4.Inverse(Matrix4x4.TRS(
            pos,
            Quaternion.LookRotation(dir, Vector3.up),
            new Vector3(1, 1, -1)
        ));

        ///

        matProj = Matrix4x4.Ortho(-halfFourierSize, halfFourierSize, -halfFourierSize, halfFourierSize, 0.3f, 2f);
    }

    void CreateCmd()
    {
        mCmd = new CommandBuffer();
        mCmd.name = "DFT";
    }

    public void Exec(Material _mat, RenderTexture _rt)
    {
        if (mCmd == null || mMesh == null) return;
        
        mCmd.SetViewProjectionMatrices(matView, matProj);
        mCmd.SetRenderTarget(_rt);
        mCmd.ClearRenderTarget(true, true, Color.clear);
        mCmd.DrawMesh(mMesh, Matrix4x4.identity, _mat);

        Graphics.ExecuteCommandBuffer(mCmd);

        mCmd.Clear();
    }

    public void Exec(Material _mat, RenderTexture[] _rts)
    {
        if (mCmd == null || mMesh == null) return;

        for(int i = 0; i < _rts.Length; ++i)
        {
            mCmd.SetRenderTarget(_rts[i]);
            mCmd.ClearRenderTarget(true, true, Color.clear);
        }

        RenderTargetIdentifier[] ids = new RenderTargetIdentifier[_rts.Length];

        for (int i = 0; i < _rts.Length; ++i)
        {
            ids[i] = _rts[i];
        }

        mCmd.SetViewProjectionMatrices(matView, matProj);
        mCmd.SetRenderTarget(ids, _rts[0].depthBuffer);
        mCmd.DrawMesh(mMesh, Matrix4x4.identity, _mat);

        Graphics.ExecuteCommandBuffer(mCmd);

        mCmd.Clear();
    }
}
