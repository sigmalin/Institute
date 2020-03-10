using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardBenjaminNoiseFiliter
{
    public static float Evaluate(Vector3 point, NoiseSetting setting)
    {
        if (WardBenjaminNoise.Seed == 0)
        {
            WardBenjaminNoise.Seed = 114514;
        }

        float noiseValue = 0f;
        float freq = setting.BaseRoughness;
        float amp = 1f;

        for (int i = 0; i < setting.Octave; ++i)
        {
            float v = _Evaluate(point * freq + setting.Center);
            noiseValue += (v + 1) * 0.5f * amp;
            freq *= setting.Roughness;
            amp *= setting.Persistence;
        }

        noiseValue = Mathf.Max(0f, noiseValue - setting.Threshold);
        return noiseValue * setting.Strength;
    }

    static float _Evaluate(Vector3 point)
    {
        //return Noise.Evaluate(point.x, point.y, point.z);
        return WardBenjaminNoise.Generate(point.x, point.y, point.z);
    }
}
