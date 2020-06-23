using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[CreateAssetMenu(menuName = "Test Pipeline")]
public class TestPipelineAsset : UnityEngine.Rendering.RenderPipelineAsset
{
    public enum TextureSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
    }

    #region Shadow
    [System.Serializable]
    public class ShadowSettings
    {
        [Min(0f)]
        public float maxDistance = 100f;

        public TextureSize atlasSize = TextureSize._1024;
    }
    #endregion

    #region FXAA
    public enum FxaaContrastThreshold
    {
        // Trims the algorithm from processing darks.
        //   0.0833 - upper limit (default, the start of visible unfiltered edges)
        //   0.0625 - high quality (faster)
        //   0.0312 - visible limit (slower)	
        Default,
        Faster,
        Slower,
    }

    public enum FxaaRelativeThreshold
    {
        // The minimum amount of local contrast required to apply algorithm.
        //   0.333 - too little (faster)
        //   0.250 - low quality
        //   0.166 - default
        //   0.125 - high quality 
        //   0.063 - overkill (slower)
        Faster,
        LowQuality,
        Default,
        HighQuality,
        Slower,
    }

    [System.Serializable]
    public class FxaaSettings
    {
        public FxaaContrastThreshold fxaaContrastThreshold = FxaaContrastThreshold.Default;

        public FxaaRelativeThreshold fxaaRelativeThreshold = FxaaRelativeThreshold.Default;

        [Range(0, 1)]
        public float SubPixelBlending = 0f;

        public float GetContrastThreshold()
        {
            float res = 0f;

            switch (fxaaRelativeThreshold)
            {
                case FxaaRelativeThreshold.Faster:
                    res = 0.333f;
                    break;
                case FxaaRelativeThreshold.Slower:
                    res = 0.063f;
                    break;
                case FxaaRelativeThreshold.LowQuality:
                    res = 0.25f;
                    break;
                case FxaaRelativeThreshold.HighQuality:
                    res = 0.125f;
                    break;
                case FxaaRelativeThreshold.Default:
                default:
                    res = 0.166f;
                    break;
            }

            return res;
        }

        public float GetRelativeThreshold()
        {
            float res = 0f;

            switch (fxaaContrastThreshold)
            {
                case FxaaContrastThreshold.Faster:
                    res = 0.0833f;
                    break;
                case FxaaContrastThreshold.Slower:
                    res = 0.0312f;
                    break;
                case FxaaContrastThreshold.Default:
                default:
                    res = 0.0833f;
                    break;
            }

            return res;
        }
    }
    #endregion

    #region Bloom
    [System.Serializable]
    public class BloomSettings
    {
        [Range(0, 10)]
        public float intensity = 1;

        [Range(1, 16)]
        public int iterations = 4;

        [Range(0, 10)]
        public float threshold = 1;

        [Range(0, 1)]
        public float softThreshold = 0.5f;

        public Vector4 GetFiliterParameters()
        {
            float knee = threshold * softThreshold;

            Vector4 filter;
            filter.x = threshold;
            filter.y = filter.x - knee;
            filter.z = 2f * knee;
            filter.w = 0.25f / (knee + 0.00001f);
            return filter;
        }
    }
    #endregion

    [System.Serializable]
    public class Settings
    {
        public bool useSRPBatcher = true;

        [Range(0.1f, 5.0f)]
        public float Exposure = 0.5f;

        public ShadowSettings shadow = default;

        public FxaaSettings Fxaa = default;

        public BloomSettings Bloom = default;
    }

    [SerializeField]
    Settings Setting = default;

    protected override RenderPipeline CreatePipeline()
    {
        return new TestPipeline(Setting);
    }
}
