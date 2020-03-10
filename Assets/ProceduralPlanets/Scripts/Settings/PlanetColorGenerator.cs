using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetColorGenerator
{
    PlanetColorSetting mColorSetting;

    Material mMatPlanet;
    public Material MatPlanet { get { return mMatPlanet;  } }

    Texture2D mGradientTex;

    NoiseGanerator mNoise;

    const int GRADIENT_TEX_SIZE = 128;

    public PlanetColorGenerator()
    {
        mNoise = new NoiseGanerator();

        //mMatPlanet = new Material(Shader.Find("ProceduralPlanets/Planet"));
        //mMatPlanet = new Material(Shader.Find("ProceduralPlanets/PlanetPBR"));
        mMatPlanet = new Material(Shader.Find("ProceduralPlanets/PlanetFinal"));
        mMatPlanet.hideFlags = HideFlags.HideAndDontSave;
    }

    public void Initialize(PlanetColorSetting _setting)
    {
        mColorSetting = _setting;
        
        mNoise.Initialize(mColorSetting == null ? null : mColorSetting.Noise);
    }

    bool InitialGradientTex()
    {
        if (mColorSetting == null || mColorSetting.Biomes == null) return false;
        if (mColorSetting.Biomes.Length == 0) return false;

        if (mGradientTex != null && 
            mGradientTex.height != mColorSetting.Biomes.Length)
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(mGradientTex);
#else
            GameObject.Destroy(mGradientTex);            
#endif
            mGradientTex = null;
        }

        if (mGradientTex == null)
        {
            int numBiome = mColorSetting.Biomes.Length;

            mGradientTex = new Texture2D(GRADIENT_TEX_SIZE << 1, numBiome);
            mGradientTex.wrapMode = TextureWrapMode.Clamp;
        }

        return true;
    }

    public void UpdateGradientTex()
    {
        if (InitialGradientTex() == false) return;

        int numBiome = mColorSetting.Biomes.Length;
        int texSize = GRADIENT_TEX_SIZE << 1;

        Color[] cols = mGradientTex.GetPixels();
        int colIndx = 0;

        for(int y = 0; y < numBiome; ++y)
        {
            for (int x = 0; x < texSize; ++x)
            {
                Color gradient;
                if (x < GRADIENT_TEX_SIZE)
                    gradient = mColorSetting.Ocean.Evaluate(((float)(x)) / (GRADIENT_TEX_SIZE - 1));                
                else
                    gradient = mColorSetting.Biomes[y].gradient.Evaluate(((float)(x - GRADIENT_TEX_SIZE)) / (GRADIENT_TEX_SIZE - 1));

                Color tint = mColorSetting.Biomes[y].tint;
                cols[colIndx] = gradient * (1f - mColorSetting.Biomes[y].tintPercent) + tint * mColorSetting.Biomes[y].tintPercent;
                ++colIndx;
            }
        }

        mGradientTex.SetPixels(cols);
        mGradientTex.Apply(false, false);

        mMatPlanet.SetTexture("_Gradient", mGradientTex);
    }

    public void SetMinMaxValue(Vector4 _value)
    {
        mMatPlanet.SetVector("_VecMinMax", _value);
    }

    public float CalculateBiomeOnPlanet(Vector3 pos)
    {
        if (mColorSetting == null) return 0f;
        if (mColorSetting.Biomes == null) return 0f;

        int numBiomes = mColorSetting.Biomes.Length;
        if (numBiomes == 0) return 0f;

        float heightPercent = (pos.y + 1f) * 0.5f;
        heightPercent += (mNoise.Evaluate(pos) - mColorSetting.NoiseOffset) * mColorSetting.NoiseStrength;

        float blendRange = mColorSetting.BlendAmount * 0.5f + 0.001f;
        float biome = 0f;

        for(int i = 0; i < numBiomes; ++i)
        {
            float weight = Mathf.InverseLerp(-blendRange, blendRange, heightPercent - mColorSetting.Biomes[i].startHeight);
            biome *= (1 - weight);
            biome += weight * i;
        }

        return (float)biome / Mathf.Max(1, numBiomes - 1);
    }
}
