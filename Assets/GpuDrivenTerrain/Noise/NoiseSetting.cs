using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurbulenceType
{
    Turbulence,
    IQ,
    Billowed,
    Ridged,
    Swiss,
    Jordan
};

[System.Serializable]
public class TurbulenceSetting
{
    public float TurbulenceSeed = 0f;

    [Range(4, 12)]
    public int Octaves = 8;

    public float Amplitude = 500f;

    public float Frequence = 5f;

    public float Lacunarity = 2.0f;

    [Range(0.001f, 10f)]
    public float Gain = 0.5f;

    public float TurbulenceFreq
    {
        get { return Frequence * 0.0001f; }
    }
}

[System.Serializable]
public class NoiseSetting
{
    [HideInInspector]
    public readonly int SimplexNoiseSize = 2048;

    [HideInInspector]
    public readonly int SimplexNoiseMask = 2047;

    public long NoiseSeed = 114514;

    public TurbulenceType type;

    public TurbulenceSetting Turbulence;
}
