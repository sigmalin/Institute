using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlanetShapeSetting : ISettingData
{
    public float PlanetRadius;

    public PlanetNoiseSetting[] PlanetNoises;

    [System.Serializable]
    public class PlanetNoiseSetting
    {        
        public bool Enabled = false;
        public bool UseFirstLayerAsMask = false;
        public NoiseSetting Noise;
    }
}
