using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseGanerator
{
    NoiseSetting mSetting;

    public NoiseGanerator()
    {

    }

    public void Initialize(NoiseSetting _setting)
    {
        mSetting = _setting;
    }

    public float Evaluate(Vector3 pos)
    {
        if (mSetting == null) return 0f;

        float res = 0f;
        switch (mSetting.NoiseType)
        {
            case NoiseSetting.Type.Simplex:                
                res = SimplexNoiseFiliter.Evaluate(pos, mSetting);
                break;

            case NoiseSetting.Type.OpenSimplex:
                res = OpenSimplexNoiseFiliter.Evaluate(pos, mSetting);
                break;

            case NoiseSetting.Type.Ridgid:
                res = RidgidNoiseFiliter.Evaluate(pos, mSetting);
                break;
        }
        return res;
    }
}
