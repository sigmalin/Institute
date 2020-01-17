//https://github.com/Scrawk/Phillips-Ocean/blob/master/Assets/PhillipsOcean/Scripts/FourierCPU.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FastFourier
{
    const int PASS_X_1 = 0, PASS_Y_1 = 1;
    const int PASS_X_2 = 2, PASS_Y_2 = 3;
    const int PASS_X_3 = 4, PASS_Y_3 = 5;

    int mFourierSize;
    float mfFourierSize;
    int mFourierPass;

    Texture2D[] mButterflyLookupTable = null;
    Material[] mMatFastFouriers;

    int ShaderID_ButterFlyLookUp;
    int ShaderID_ReadBuffer0;
    int ShaderID_ReadBuffer1;
    int ShaderID_ReadBuffer2;
    int ShaderID_Size;
    
    public FastFourier(int _fourierSize)
    {
        if(256 < _fourierSize)
        {
            Debug.LogError("Fourier grid size must not be greater than 256, changing to 256");
            return;
        }

        if (Mathf.IsPowerOfTwo(_fourierSize) == false)
            return;

        mFourierSize = _fourierSize;
        mfFourierSize = (float)_fourierSize;
        mFourierPass = (int)(Mathf.Log(mfFourierSize) / Mathf.Log(2.0f));

        mButterflyLookupTable = new Texture2D[mFourierPass];
        ComputeButterflyLookupTable();
        
        ShaderID_ButterFlyLookUp = Shader.PropertyToID("_ButterFlyLookUp");
        ShaderID_ReadBuffer0 = Shader.PropertyToID("_ReadBuffer0");
        ShaderID_ReadBuffer1 = Shader.PropertyToID("_ReadBuffer1");
        ShaderID_ReadBuffer2 = Shader.PropertyToID("_ReadBuffer2");
        ShaderID_Size = Shader.PropertyToID("_Size");

        CreateMaterials(mFourierPass);
    }

    void CreateMaterials(int _pass)
    {
        mMatFastFouriers = new Material[_pass];
        for(int i = 0; i < mMatFastFouriers.Length; ++i)
        {
            mMatFastFouriers[i] = new Material((Shader.Find("FourierOcean/FastFourier")));
            mMatFastFouriers[i].hideFlags = HideFlags.HideAndDontSave;
        }        
    }

    int BitReverse(int i)
    {
        int j = i;
        int Sum = 0;
        int W = 1;
        int M = mFourierSize >> 1;
        while (M != 0)
        {
            j = ((i & M) > M - 1) ? 1 : 0;
            Sum += j * W;
            W <<= 1;
            M >>= 1;
        }
        return Sum;
    }

    void ComputeButterflyLookupTable()
    {
        for(int i = 0; i < mFourierPass; ++i)
        {
            int Block = (int)Mathf.Pow(2, mFourierPass - 1 - i);
            int Inputs = (int)Mathf.Pow(2, i);

            mButterflyLookupTable[i] = new Texture2D(mFourierSize, 1, TextureFormat.ARGB32, false, true);
            mButterflyLookupTable[i].filterMode = FilterMode.Point;
            mButterflyLookupTable[i].wrapMode = TextureWrapMode.Clamp;

            Color[] Cols = new Color[mFourierSize];

            for(int j = 0; j < Block; ++j)
            {
                for(int k = 0; k < Inputs; ++k)
                {
                    int i1, i2, j1, j2;
                    if (i == 0)
                    {
                        i1 = j * (Inputs << 1) + k;
                        i2 = j * (Inputs << 1) + Inputs + k;
                        j1 = BitReverse(i1);
                        j2 = BitReverse(i2);
                    }
                    else
                    {
                        i1 = j * (Inputs << 1) + k;
                        i2 = j * (Inputs << 1) + Inputs + k;
                        j1 = i1;
                        j2 = i2;
                    }

                    Cols[i1] = new Color((float)j1 / 255.0f, (float)j2 / 255.0f, (float)(k * Block) / 255.0f, 0);
                    Cols[i2] = new Color((float)j1 / 255.0f, (float)j2 / 255.0f, (float)(k * Block) / 255.0f, 1);
                }
            }

            mButterflyLookupTable[i].SetPixels(Cols);
            mButterflyLookupTable[i].Apply();
        }
    }
    
    void MultiTargetBlit(CommandBuffer _cmd, RenderTexture[] _rts, Material _drawer, int _pass = 0)
    {
        RenderTargetIdentifier[] ids = new RenderTargetIdentifier[_rts.Length];

        for (int i = 0; i < _rts.Length; ++i)
        {
            ids[i] = _rts[i];
        }

        _cmd.SetRenderTarget(ids, _rts[0].depthBuffer);
        _cmd.Blit(null, BuiltinRenderTextureType.CurrentActive, _drawer, _pass);
    }


    public int PerformFFT_H(CommandBuffer _cmd, RenderTexture[] data0, RenderTexture[] data1, RenderTexture[] data2)
    {
        RenderTexture[] pass0 = new RenderTexture[] { data0[0], data1[0], data2[0] };
        RenderTexture[] pass1 = new RenderTexture[] { data0[1], data1[1], data2[1] };

        int i;
        int idx = 0; int idx1;
        int j = 0;

        int matIndx = 0;

        for (i = 0; i < mFourierPass; i++, j++)
        {
            idx = j & 0x01;
            idx1 = (j + 1) & 0x01;

            mMatFastFouriers[matIndx].SetTexture(ShaderID_ButterFlyLookUp, mButterflyLookupTable[i]);

            mMatFastFouriers[matIndx].SetTexture(ShaderID_ReadBuffer0, data0[idx1]);
            mMatFastFouriers[matIndx].SetTexture(ShaderID_ReadBuffer1, data1[idx1]);
            mMatFastFouriers[matIndx].SetTexture(ShaderID_ReadBuffer2, data2[idx1]);

            mMatFastFouriers[matIndx].SetFloat(ShaderID_Size, mfFourierSize);

            if (idx == 0)
                MultiTargetBlit(_cmd, pass0, mMatFastFouriers[matIndx], PASS_X_3);
            else
                MultiTargetBlit(_cmd, pass1, mMatFastFouriers[matIndx], PASS_X_3);

            ++matIndx;
        }

        return j;
    }

    public int PerformFFT_V(int _startIndx, CommandBuffer _cmd, RenderTexture[] data0, RenderTexture[] data1, RenderTexture[] data2)
    {
        RenderTexture[] pass0 = new RenderTexture[] { data0[0], data1[0], data2[0] };
        RenderTexture[] pass1 = new RenderTexture[] { data0[1], data1[1], data2[1] };

        int i;
        int idx = 0; int idx1;
        int j = _startIndx;

        int matIndx = 0;
        
        for (i = 0; i < mFourierPass; i++, j++)
        {
            idx = j & 0x01;
            idx1 = (j + 1) & 0x01;

            mMatFastFouriers[matIndx].SetTexture(ShaderID_ButterFlyLookUp, mButterflyLookupTable[i]);

            mMatFastFouriers[matIndx].SetTexture(ShaderID_ReadBuffer0, data0[idx1]);
            mMatFastFouriers[matIndx].SetTexture(ShaderID_ReadBuffer1, data1[idx1]);
            mMatFastFouriers[matIndx].SetTexture(ShaderID_ReadBuffer2, data2[idx1]);

            mMatFastFouriers[matIndx].SetFloat(ShaderID_Size, mfFourierSize);

            if (idx == 0)
                MultiTargetBlit(_cmd, pass0, mMatFastFouriers[matIndx], PASS_Y_3);
            else
                MultiTargetBlit(_cmd, pass1, mMatFastFouriers[matIndx], PASS_Y_3);

            ++matIndx;
        }

        return idx;
    }
}
