using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DiscreteFourierOceanCore : IFourierOceanCore
{
    DFTOceanOutputer mDFTOceanOutputer;

    RenderTexture mNormalRT;
    RenderTexture mDisplacementRT;

    CommandBuffer mCmd;

    void InitRenderTextures(int _fourierSize)
    {
        mNormalRT = new RenderTexture(_fourierSize << 1, _fourierSize << 1, 0, RenderTextureFormat.Default);
        mNormalRT.filterMode = FilterMode.Bilinear;
        mNormalRT.wrapMode = TextureWrapMode.Repeat;

        mDisplacementRT = new RenderTexture(_fourierSize << 1, _fourierSize << 1, 0, FourierTextureFormat.GetFourierRenderTextureFormat());
        mDisplacementRT.filterMode = FilterMode.Bilinear;
        mDisplacementRT.wrapMode = TextureWrapMode.Repeat;
    }

    void CreateCmd()
    {
        mCmd = new CommandBuffer();
        mCmd.name = "Discrete Fourier Ocean";
    }

    public void Init(int _fourierSize)
    {
        InitRenderTextures(_fourierSize);

        CreateCmd();

        mDFTOceanOutputer = new DFTOceanOutputer(_fourierSize);
    }

    public void Perform(Texture2D _Spectrum0, Texture2D _Omega, out RenderTexture _normal, out RenderTexture _displacement)
    {
        mDFTOceanOutputer.Evaluate(mCmd, _Spectrum0, _Omega, new RenderTexture[] { mNormalRT, mDisplacementRT });

        Graphics.ExecuteCommandBuffer(mCmd);
        mCmd.Clear();

        _normal = mNormalRT;
        _displacement = mDisplacementRT;
    }
}
