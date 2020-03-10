using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetShapeGenerator
{
    PlanetShapeSetting mSetting;

    NoiseGanerator mNoise;

    ValueMinMax mValueMinMax;
    public Vector4 VecMinMax { get { return mValueMinMax != null ? mValueMinMax.VecMinMax : Vector4.one; } }
    public PlanetShapeGenerator()
    {
        mNoise = new NoiseGanerator();

        mValueMinMax = new ValueMinMax();
    }

    public void Initialize(PlanetShapeSetting _setting)
    {
        mSetting = _setting;
        
        mValueMinMax.Clear();
    }

    public float CalculateUnscaledElevation(Vector3 pos)
    {
        if (mSetting == null) return 0f;
        
        float firstLayerValue = 0f;
        float elevation = 0f;

        if (mSetting.PlanetNoises != null)
        {
            mNoise.Initialize(mSetting.PlanetNoises[0].Noise);
            firstLayerValue = mNoise.Evaluate(pos);
            if (mSetting.PlanetNoises[0].Enabled)
            {
                elevation = firstLayerValue;
            }

            for (int i = 1; i < mSetting.PlanetNoises.Length; ++i)
            {
                if (mSetting.PlanetNoises[i] == null) continue;
                if (mSetting.PlanetNoises[i].Enabled == false) continue;
                
                mNoise.Initialize(mSetting.PlanetNoises[i].Noise);

                float mask = mSetting.PlanetNoises[i].UseFirstLayerAsMask ? firstLayerValue : 1f;
                elevation += mNoise.Evaluate(pos) * mask;
            }
        }

        mValueMinMax.AddValue(elevation);

        return elevation;
    }

    public float GetScaledElevation(float unscaledElevation)
    {
        float elevation = Mathf.Max(0f, unscaledElevation);
        float radius = (mSetting != null ? mSetting.PlanetRadius : 1f);
        return (1 + elevation) * radius;
    }
}
