using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueMinMax
{
    public float Max { get; private set; }
    public float Min { get; private set; }

    public Vector4 VecMinMax { get { return new Vector4(Min, Max, (Min == 0f ? 100f : 1f / Min), (Max == 0f ? 0f : 1f / Max)); } }
    
    public ValueMinMax()
    {
        Clear();
    }

    public void AddValue(float v)
    {
        if (v < Min) Min = v;
        if (Max < v) Max = v;
    }

    public void Clear()
    {
        Min = float.MaxValue;
        Max = float.MinValue;
    }
}
