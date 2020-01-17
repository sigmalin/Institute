using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FastFourierOceanCore : IFourierOceanCore
{
    FastFourier mFastFourier;
    FFTOceanOutputer mFFTOceanOutputer;

    Material mSpectrumDrawer;

    RenderTexture[] mHeightRTs;
    RenderTexture[] mSlopeRTs;
    RenderTexture[] mDisplacementRTs;

    RenderTexture mNormalRT;
    RenderTexture mDisplacementRT;

    CommandBuffer mCmd;

    int ShaderID_HTidle0;
    int ShaderID_Dispersion;

    int mStartIndxFFT;
    int mFourierBufferIndx;

    enum eStage
    {
        UpdateSpectrum,
        PerformFFT_H,
        PerformFFT_V,
        EvaluateWave,
    }

    eStage mCurStage;

    void InitMaterials()
    {
        if (mSpectrumDrawer == null)
        {
            mSpectrumDrawer = new Material(Shader.Find("FourierOcean/FFT_Spectrum"));
            mSpectrumDrawer.hideFlags = HideFlags.HideAndDontSave;
        }

        ShaderID_HTidle0 = Shader.PropertyToID("_HTidle0");
        ShaderID_Dispersion = Shader.PropertyToID("_Dispersion");
    }

    void InitRenderTextures(int _fourierSize)
    {
        CreateRenderBuffers(ref mHeightRTs, _fourierSize);
        CreateRenderBuffers(ref mSlopeRTs, _fourierSize);
        CreateRenderBuffers(ref mDisplacementRTs, _fourierSize);

        mNormalRT = new RenderTexture(_fourierSize << 1, _fourierSize << 1, 0, RenderTextureFormat.Default);
        mNormalRT.filterMode = FilterMode.Bilinear;
        mNormalRT.wrapMode = TextureWrapMode.Repeat;

        mDisplacementRT = new RenderTexture(_fourierSize << 1, _fourierSize << 1, 0, FourierTextureFormat.GetFourierRenderTextureFormat());
        mDisplacementRT.filterMode = FilterMode.Bilinear;
        mDisplacementRT.wrapMode = TextureWrapMode.Repeat;
    }

    void CreateRenderBuffers(ref RenderTexture[] _rts, int _fourierSize)
    {
        _rts = new RenderTexture[2];

        for (int i = 0; i < _rts.Length; ++i)
        {
            _rts[i] = new RenderTexture(_fourierSize, _fourierSize, 0, FourierTextureFormat.GetFourierRenderTextureFormat());
            _rts[i].filterMode = FilterMode.Point;
            _rts[i].wrapMode = TextureWrapMode.Clamp;
        }
    }

    void CreateCmd()
    {
        mCmd = new CommandBuffer();
        mCmd.name = "Fast Fourier Ocean";
    }

    void ClearRenderBuffer(RenderTexture[] _rts)
    {
        for (int i = 0; i < _rts.Length; ++i)
        {
            mCmd.SetRenderTarget(_rts[i]);
            mCmd.ClearRenderTarget(true, true, Color.clear);
        }
    }
    
    public void Init(int _fourierSize)
    {
        InitMaterials();

        InitRenderTextures(_fourierSize);

        CreateCmd();

        mSpectrumDrawer.SetInt("_FourierSize", _fourierSize);

        mFastFourier = new FastFourier(_fourierSize);
        mFFTOceanOutputer = new FFTOceanOutputer(_fourierSize);

        mCurStage = eStage.UpdateSpectrum;         
    }

    public void Perform(Texture2D _Spectrum0, Texture2D _Omega, out RenderTexture _normal, out RenderTexture _displacement)
    {
        switch(mCurStage)
        {
            case eStage.UpdateSpectrum:
                UpdateSpectrum(_Spectrum0, _Omega);
                mCurStage = eStage.PerformFFT_H;
                break;
            case eStage.PerformFFT_H:
                PerformFFT_H();
                mCurStage = eStage.PerformFFT_V;
                break;
            case eStage.PerformFFT_V:
                PerformFFT_V();
                mCurStage = eStage.EvaluateWave;
                break;
            case eStage.EvaluateWave:
                EvaluateWave();
                mCurStage = eStage.UpdateSpectrum;
                break;
        }        
        
        Graphics.ExecuteCommandBuffer(mCmd);
        mCmd.Clear();

        _normal = mNormalRT;
        _displacement = mDisplacementRT;
    }

    void UpdateSpectrum(Texture2D _Spectrum0, Texture2D _Omega)
    {
        mSpectrumDrawer.SetTexture(ShaderID_HTidle0, _Spectrum0);
        mSpectrumDrawer.SetTexture(ShaderID_Dispersion, _Omega);

        ClearRenderBuffer(mHeightRTs);
        ClearRenderBuffer(mSlopeRTs);
        ClearRenderBuffer(mDisplacementRTs);

        RenderTargetIdentifier[] ids = new RenderTargetIdentifier[]
        {
            mHeightRTs[1],
            mSlopeRTs[1],
            mDisplacementRTs[1],
        };

        mCmd.SetRenderTarget(ids, mHeightRTs[1].depthBuffer);
        mCmd.Blit(null, BuiltinRenderTextureType.CurrentActive, mSpectrumDrawer);
    }

    void PerformFFT_H()
    {
        mStartIndxFFT = mFastFourier.PerformFFT_H(mCmd, mHeightRTs, mSlopeRTs, mDisplacementRTs);
    }

    void PerformFFT_V()
    {
        mFourierBufferIndx = mFastFourier.PerformFFT_V(mStartIndxFFT, mCmd, mHeightRTs, mSlopeRTs, mDisplacementRTs);
    }

    void EvaluateWave()
    {
        mFFTOceanOutputer.Evaluate(mCmd, mHeightRTs[mFourierBufferIndx], mSlopeRTs[mFourierBufferIndx], mDisplacementRTs[mFourierBufferIndx], new RenderTexture[] { mNormalRT, mDisplacementRT });
    }
}
