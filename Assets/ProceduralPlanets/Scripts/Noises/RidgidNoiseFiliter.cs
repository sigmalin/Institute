using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RidgidNoiseFiliter
{
    static OpenSimplexNoise mNoise;
    static OpenSimplexNoise Noise { get { if (mNoise == null) mNoise = new OpenSimplexNoise(2525L); return mNoise; } }

    public static float Evaluate(Vector3 point, NoiseSetting setting)
    {
        float noiseValue = 0f;
        float freq = setting.BaseRoughness;
        float amp = 1f;
        float weight = 1f;

        for(int i = 0; i < setting.Octave; ++i)
        {
            float v = 1f - Mathf.Abs(_Evaluate(point * freq + setting.Center));
            v *= v;
            v *= weight;
            weight = Mathf.Clamp01(v * setting.WeightMultiper);

            noiseValue += v * amp;
            freq *= setting.Roughness;
            amp *= setting.Persistence;
        }
        
        noiseValue = noiseValue - setting.Threshold;
        return noiseValue * setting.Strength;
    }

    static float _Evaluate(Vector3 point)
    {
        return Noise.Evaluate(point.x, point.y, point.z);        
    }
}
