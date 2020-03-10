using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlanetColorSetting : ISettingData
{
    public Gradient Ocean;

    public Biome[] Biomes;

    public NoiseSetting Noise;
    public float NoiseOffset;
    public float NoiseStrength;
    [Range(0, 1)]
    public float BlendAmount;

    [System.Serializable]
    public class Biome
    {
        public Gradient gradient;
        public Color tint;
        [Range(0, 1)]
        public float startHeight;
        [Range(0, 1)]
        public float tintPercent;
    }
}
