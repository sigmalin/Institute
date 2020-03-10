using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSetting
{
    public enum Type
    {
        Simplex,
        OpenSimplex,
        Ridgid,
    }
    public Type NoiseType = Type.Simplex;
        
    public float Strength = 1f;

    [Range(1, 8)]
    public int Octave = 1;

    public float BaseRoughness = 1f;
    public float Roughness = 2f;
    public float Persistence = 0.5f;
    public Vector3 Center;
    public float Threshold = 0f;

    [ConditionalHide("NoiseType", 2)]
    public float WeightMultiper = 0.8f;
}
