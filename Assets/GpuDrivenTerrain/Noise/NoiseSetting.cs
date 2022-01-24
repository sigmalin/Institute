using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSetting
{
    [HideInInspector]
    public readonly int SimplexNoiseSize = 2048;

    [HideInInspector]
    public readonly int SimplexNoiseMask = 2047;

    public long NoiseSeed = 114514;
}
