//https://github.com/Scrawk/GPU-GEMS-Improved-Perlin-Noise/blob/master/Assets/ImprovedPerlinNoise/Scripts/GPUPerlinNoise.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PerlinNoise
{
    const int SIZE = 256;

    long seed;

    GraphicsBuffer PerlinPerm;
    GraphicsBuffer PerlinGrad;

    NoiseSetting Setting;

    public PerlinNoise(NoiseSetting setting)
    {
        Setting = setting;
    }

    public void Initialize()
    {
        if (Setting != null)
        {
            InitGraphicsBuffer();
        }
    }

    public void Release()
    {
        ReleaseGraphicsBuffer();
    }

    bool isValid()
    {
        return Setting != null &&
            PerlinPerm != null &&
            PerlinGrad != null;
    }

    void InitGraphicsBuffer()
    {
        ReleaseGraphicsBuffer();

        int bufferCount = BufferCount();

        PerlinPerm = new GraphicsBuffer(GraphicsBuffer.Target.Structured, bufferCount, sizeof(int));
        PerlinGrad = new GraphicsBuffer(GraphicsBuffer.Target.Structured, bufferCount, sizeof(int));

        Generate(Setting.NoiseSeed);
    }

    void ReleaseGraphicsBuffer()
    {
        if (PerlinPerm != null)
        {
            PerlinPerm.Release();
            PerlinPerm.Dispose();
            PerlinPerm = null;
        }

        if (PerlinGrad != null)
        {
            PerlinGrad.Release();
            PerlinGrad.Dispose();
            PerlinGrad = null;
        }
    }

    void Generate(long _seed)
    {
        seed = _seed;

        UnityEngine.Random.InitState((int)seed);

        byte[] perms;
        GetPerm(out perms);

        GeneratePerlinPermTexture(perms);
        GeneratePerlinGradTexture(perms);
    }

    void GetPerm(out byte[] _perms)
    {
        byte[] perm = {
            151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

        int[] shuffle = new int[SIZE];
        for (int i = 0; i < SIZE; ++i)
            shuffle[i] = i;

        for (int i = 0; i < SIZE; ++i)
        {
            int target = UnityEngine.Random.Range(0, 256);
            int swap = shuffle[i];
            shuffle[i] = shuffle[target];
            shuffle[target] = swap;
        }

        int permSize = SIZE * 2;
        _perms = new byte[permSize];
        for (int i = 0; i < permSize; ++i)
        {
            int t = shuffle[i % SIZE];
            _perms[i] = perm[t];
        }
    }

    int BufferCount()
    {
        return SIZE * SIZE;
    }

    void GeneratePerlinPermTexture(byte[] _perms)
    {
        int bufferCount = BufferCount();
        int[] datas = new int[bufferCount];

        for (int y = 0; y < SIZE; y++)
        {
            for (int x = 0; x < SIZE; x++)
            {
                int A = _perms[x] + y;
                int AA = _perms[A];
                int AB = _perms[A + 1];

                int B = _perms[x + 1] + y;
                int BA = _perms[B];
                int BB = _perms[B + 1];

                int index = y * SIZE + x;
                datas[index] = ((AA << 24) | (AB << 16) | (BA << 8) | (BB));
            }
        }

        PerlinPerm.SetData(datas);
    }

    void GeneratePerlinGradTexture(byte[] perm)
    {
        int[] GRADIENT2 = new int[] {
            0, 1,
            1, 1,
            1, 0,
            1, -1,
            0, -1,
            -1, -1,
            -1, 0,
            -1, 1,
        };

        int bufferCount = BufferCount();
        int[] datas = new int[bufferCount];

        for (int y = 0; y < SIZE; ++y)
        {
            int gradIdxY = perm[y] & 0x07;
            // remap [-1,1] -> [0, 2]
            int B = (GRADIENT2[gradIdxY * 2 + 0] + 1);
            int A = (GRADIENT2[gradIdxY * 2 + 1] + 1);

            for (int x = 0; x < SIZE; ++x)
            {
                int gradIdxX = perm[x] & 0x07;
                // remap [-1,1] -> [0, 2]
                int R = (GRADIENT2[gradIdxX * 2 + 0] + 1);
                int G = (GRADIENT2[gradIdxX * 2 + 1] + 1);

                int index = y * SIZE + x;
                datas[index] = ((R << 24) | (G << 16) | (B << 8) | (A));
            }
        }

        PerlinGrad.SetData(datas);
    }

    public bool Apply(Material _mat)
    {
        if (isValid() == false) return false;

        _mat.SetBuffer(Shader.PropertyToID("_PerlinPerm"), PerlinPerm);
        _mat.SetBuffer(Shader.PropertyToID("_PerlinGrad"), PerlinGrad);

        _mat.SetFloat(Shader.PropertyToID("TurbulenceSeed"), Setting.Turbulence.TurbulenceSeed);
        _mat.SetInt(Shader.PropertyToID("TurbulenceOctaves"), Setting.Turbulence.Octaves);
        _mat.SetFloat(Shader.PropertyToID("TurbulenceAmplitude"), Setting.Turbulence.Amplitude);
        _mat.SetFloat(Shader.PropertyToID("TurbulenceFrequence"), Setting.Turbulence.TurbulenceFreq);
        _mat.SetFloat(Shader.PropertyToID("TurbulenceLacunarity"), Setting.Turbulence.Lacunarity);
        _mat.SetFloat(Shader.PropertyToID("TurbulenceGain"), Setting.Turbulence.Gain);

        return true;
    }
}
