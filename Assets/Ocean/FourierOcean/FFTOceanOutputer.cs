using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FFTOceanOutputer
{
    Mesh mMesh;

    Matrix4x4 matView, matProj;

    Material mOceanDrawer;

    int ShaderID_Height;
    int ShaderID_Slope;
    int ShaderID_Displacement;

    public FFTOceanOutputer(int _fourierSize)
    {
        if (256 <= _fourierSize)
        {
            Debug.LogError("Fourier grid size must not be greater than 256, Because Of 256 * 256 over Max Vertices Of Mesh");
            return;
        }

        CreateMesh(_fourierSize);

        CalcMatrix(_fourierSize);

        CreateMaterial(_fourierSize);
    }

    void CreateMesh(int _fourierSize)
    {
        if (mMesh == null) mMesh = new Mesh();

        mMesh.Clear();

        int FourierSizePlusOne = _fourierSize + 1;
        float FourierSizeMinusOne = _fourierSize - 1;

        List<Vector3> vertices = new List<Vector3>(FourierSizePlusOne * FourierSizePlusOne);
        List<Vector3> uvs = new List<Vector3>(FourierSizePlusOne * FourierSizePlusOne);
        List<int> indices = new List<int>(FourierSizePlusOne * FourierSizePlusOne * 6);

        for (int y = 0; y < FourierSizePlusOne; ++y)
        {
            for (int x = 0; x < FourierSizePlusOne; ++x)
            {
                vertices.Add(new Vector3(x, y, 0f));

                int n_prime = x;
                int m_prime = y;

                if (_fourierSize == n_prime) n_prime = 0;
                if (_fourierSize == m_prime) m_prime = 0;

                float sign = (((n_prime + m_prime) & 0x01) == 0) ? 1f : -1f;

                Vector3 uv = new Vector3(n_prime / FourierSizeMinusOne, m_prime / FourierSizeMinusOne, sign);
                uvs.Add(uv);
            }
        }

        for (int y = 0; y < _fourierSize; ++y)
        {
            for (int x = 0; x < _fourierSize; ++x)
            {
                indices.Add(x + y * FourierSizePlusOne);
                indices.Add(x + (y + 1) * FourierSizePlusOne);
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

    void CreateMaterial(int _fourierSize)
    {
        mOceanDrawer = new Material(Shader.Find("FourierOcean/FFT_Output"));        
        mOceanDrawer.hideFlags = HideFlags.HideAndDontSave;

        mOceanDrawer.SetInt("_FourierSize", _fourierSize);

        ShaderID_Height = Shader.PropertyToID("_Height");
        ShaderID_Slope = Shader.PropertyToID("_Slope");
        ShaderID_Displacement = Shader.PropertyToID("_Displacement");
    }
    public void Evaluate(CommandBuffer _cmd, RenderTexture _heightRT, RenderTexture _slopeRT, RenderTexture _displacementRT, RenderTexture[] _outputRTs)
    {
        if (_cmd == null || mMesh == null) return;

        for (int i = 0; i < _outputRTs.Length; ++i)
        {
            _cmd.SetRenderTarget(_outputRTs[i]);
            _cmd.ClearRenderTarget(true, true, Color.clear);
        }

        RenderTargetIdentifier[] ids = new RenderTargetIdentifier[_outputRTs.Length];

        for (int i = 0; i < _outputRTs.Length; ++i)
        {
            ids[i] = _outputRTs[i];
        }

        mOceanDrawer.SetTexture(ShaderID_Height, _heightRT);
        mOceanDrawer.SetTexture(ShaderID_Slope, _slopeRT);
        mOceanDrawer.SetTexture(ShaderID_Displacement, _displacementRT);

        _cmd.SetRenderTarget(ids, _outputRTs[0].depthBuffer);
        _cmd.SetViewProjectionMatrices(matView, matProj);
        _cmd.DrawMesh(mMesh, Matrix4x4.identity, mOceanDrawer);
    }
}
