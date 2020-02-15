//https://www.ronja-tutorials.com/2018/10/06/tiling-noise.html
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseUtility
{
    public delegate float Noise2D(Vector2 pos, float period);
    public delegate float Noise3D(Vector3 pos, float period);

    public static float RandomSeed = 114514.4545f;
    public static int Octaves = 4;
    public static float Persistence = 0.5f;
    public static float Scale = 2f;

    public static void CreateTexture(int _dim, float _period, Noise2D _func, bool _fBm, out Texture2D _tex)
    {
        _tex = new Texture2D(_dim, _dim);

        if (_func == null) return;

        Color[] col = _tex.GetPixels();

        for (int y = 0; y < _dim; ++y)
        {
            for (int x = 0; x < _dim; ++x)
            {
                Vector2 uv = new Vector2((float)x / (float)(_dim - 1), (float)y / (float)(_dim - 1));
                float p = _fBm == true ? fBm2D (uv, _period, _func) : _func(uv, _period);
                col[y * _dim + x] = new Color(p, p, p, p);
            }
        }

        _tex.SetPixels(col);
        _tex.Apply(false);
    }

    public static void CreateTexture(int _dim, float _period, Noise3D _func, bool _fBm, out Texture3D _tex)
    {
        _tex = new Texture3D(_dim, _dim, _dim, TextureFormat.RGBA32, true);

        Color[] col = _tex.GetPixels();

        for (int z = 0; z < _dim; ++z)
        {
            for (int y = 0; y < _dim; ++y)
            {
                for (int x = 0; x < _dim; ++x)
                {
                    Vector3 uv = new Vector3((float)x / (float)(_dim - 1), (float)y / (float)(_dim - 1), (float)z / (float)(_dim - 1));
                    float p = _fBm == true ? fBm3D(uv, _period, _func) : _func(uv, _period);
                    col[z * _dim * _dim + y * _dim + x] = new Color(p, p, p, p);
                }
            }
        }

        _tex.SetPixels(col);
        _tex.Apply(false);
    }

    //
    static Vector2 fract(Vector2 p)
    {
        return new Vector2(p.x - Mathf.Floor(p.x), p.y - Mathf.Floor(p.y));
    }

    static Vector3 fract(Vector3 p)
    {
        return new Vector3(p.x - Mathf.Floor(p.x), p.y - Mathf.Floor(p.y), p.z - Mathf.Floor(p.z));
    }

    static Vector2 floor(Vector2 p)
    {
        return new Vector2(Mathf.Floor(p.x), Mathf.Floor(p.y));
    }

    static Vector3 floor(Vector3 p)
    {
        return new Vector3(Mathf.Floor(p.x), Mathf.Floor(p.y), Mathf.Floor(p.z));
    }

    static Vector2 sin(Vector2 v, float multiplier = 1f)
    {
        return new Vector2(Mathf.Sin(v.x) * multiplier, Mathf.Sin(v.y) * multiplier);
    }

    static Vector3 sin(Vector3 v, float multiplier = 1f)
    {
        return new Vector3(Mathf.Sin(v.x) * multiplier, Mathf.Sin(v.y) * multiplier, Mathf.Sin(v.z) * multiplier);
    }

    static Vector2 random2(Vector2 p)
    {
        Vector2 v = new Vector2(Vector2.Dot(p, new Vector2(127.1f, 311.7f)), Vector2.Dot(p, new Vector2(269.5f, 183.3f)));
        v = sin(v, RandomSeed);
        return fract(v);
    }

    static Vector3 random3(Vector3 p)
    {
        Vector3 v = new Vector3(Vector3.Dot(p, new Vector3(127.1f, 311.7f, 74.7f)),
                                Vector3.Dot(p, new Vector3(269.5f, 183.3f, 246.1f)),
                                Vector3.Dot(p, new Vector3(113.5f, 271.9f, 124.6f)));

        v = sin(v, RandomSeed);
        return fract(v);
    }

    static float mod(float a, float b)
    {
        return a - b * Mathf.Floor(a / b);
    }

    static float modulo(float divident, float divisor)
    {
        float positiveDivident = mod(divident, divisor) + divisor;
        return mod(positiveDivident, divisor);
    }

    static Vector2 modulo(Vector2 divident, float divisor)
    {
        return new Vector2(modulo(divident.x, divisor), modulo(divident.y, divisor));
    }

    static Vector3 modulo(Vector3 divident, float divisor)
    {
        return new Vector3(modulo(divident.x, divisor), modulo(divident.y, divisor), modulo(divident.z, divisor));
    }

    ///

    public static float TiliedCellularNoise2D(Vector2 st, float period)
    {
        st.x *= period;
        st.y *= period;

        // Tile the space
        Vector2 i_st = floor(st);
        Vector2 f_st = fract(st);

        float dist = 1f;  // minimun distance

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                // Neighbor place in the grid
                Vector2 neighbor = new Vector2(x, y);

                Vector2 tiledCell = modulo(i_st + neighbor, period);

                // Random position from current + neighbor place in the grid
                Vector2 point = random2(tiledCell);

                // Animate the point
                point = sin(new Vector2(point.x * 6.2831f, point.y * 6.2831f) + new Vector2(0f, 0f));
                point.x = 0.5f + 0.5f * point.x;
                point.y = 0.5f + 0.5f * point.y;

                // Vector between the pixel and the point
                Vector2 diff = neighbor + point - f_st;//i_st + neighbor + point - st;

                // Distance to the point
                float d = diff.magnitude;

                // Keep the closer distance
                dist = Mathf.Min(d, dist);
            }
        }

        return dist;
    }

    public static float TiliedVoronoi2D(Vector2 st, float period)
    {
        return 1.0f - TiliedCellularNoise2D(st, period);
    }

    public static float TiliedCellularNoise3D(Vector3 st, float period)
    {
        st.x *= period;
        st.y *= period;
        st.z *= period;

        // Tile the space
        Vector3 i_st = floor(st);
        Vector3 f_st = fract(st);

        float dist = 1.0f;

        for (int k = -1; k <= 1; ++k)
        {
            for (int j = -1; j <= 1; ++j)
            {
                for (int i = -1; i <= 1; ++i)
                {
                    Vector3 neighbor = new Vector3(i, j, k);

                    Vector3 tiledCell = modulo(i_st + neighbor, period);

                    // Random position from current + neighbor place in the grid
                    Vector3 point = random3(tiledCell);

                    // Animate the point
                    point = sin(new Vector3(point.x * 6.2831f, point.y * 6.2831f, point.z * 6.2831f) + new Vector3(0f, 0f, 0f));
                    point.x = 0.5f + 0.5f * point.x;
                    point.y = 0.5f + 0.5f * point.y;
                    point.z = 0.5f + 0.5f * point.z;

                    // Vector between the pixel and the point
                    Vector3 diff = neighbor + point - f_st;//i_st + neighbor + point - st;

                    // Distance to the point
                    float d = diff.magnitude;

                    // Keep the closer distance
                    dist = Mathf.Min(d, dist);
                }
            }
        }

        return dist;
    }

    public static float TiliedVoronoi3D(Vector3 st, float period)
    {
        return 1.0f - TiliedCellularNoise3D(st, period);
    }

    ///
    static int[] perlinTable;

    // Hash lookup table as defined by Ken Perlin.  This is a randomly
    // arranged array of all numbers from 0-255 inclusive.
    static int[] perm = {
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

    static int inc(float x, float period)
    {
        x += 1f;
        x = modulo(x, period);
        return Mathf.FloorToInt(x);
    }

    static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    static float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    static float Grad(int hash, float x)
    {
        return (hash & 1) == 0 ? x : -x;
    }

    static float Grad(int hash, float x, float y)
    {
        return ((hash & 1) == 0 ? x : -x) + ((hash & 2) == 0 ? y : -y);
    }

    static float Grad(int hash, float x, float y, float z)
    {
        var h = hash & 15;
        var u = h < 8 ? x : y;
        var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    public static void InitPerlinTable()
    {
        perlinTable = new int[512];

        int[] seed = new int[256];
        for (int i = 0; i < 255; ++i)
            seed[i] = i;

        Random.InitState(Mathf.FloorToInt(RandomSeed));

        for (int i = 0; i < 255; ++i)
        {
            int swap = Random.Range(0, 256);
            int tmp = seed[i];
            seed[i] = seed[swap];
            seed[swap] = tmp;
        }

        for (int i = 0; i < 512; ++i)
        {
            perlinTable[i] = perm[seed[i % 256]];
        }
    }

    public static float TiliedPerlinNoise2D(Vector2 st, float period)
    {
        st.x *= period;
        st.y *= period;
        
        st = modulo(st, period);

        float x = st.x;
        float y = st.y;

        var X = Mathf.FloorToInt(x) & 0xff;
        var Y = Mathf.FloorToInt(y) & 0xff;
        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);
        var u = Fade(x);
        var v = Fade(y);

        int aaa, aba, baa, bba;
        aaa = perlinTable[perlinTable[X] + Y];
        aba = perlinTable[perlinTable[X] + inc(Y, period)];
        baa = perlinTable[perlinTable[inc(X, period)] + Y];
        bba = perlinTable[perlinTable[inc(X, period)] + inc(Y, period)];

        float x1, x2, y1;
        x1 = Lerp(u,
                Grad(aaa, x, y),
                Grad(baa, x - 1, y)
                );
        x2 = Lerp(u,
                Grad(aba, x, y - 1),
                Grad(bba, x - 1, y - 1)
                );

        y1 = Lerp(v, x1, x2);
        return (y1 + 1) * 0.5f;
    }

    public static float TiliedPerlinNoise3D(Vector3 st, float period)
    {
        st.x *= period;
        st.y *= period;
        st.z *= period;

        st = modulo(st, period);

        float x = st.x;
        float y = st.y;
        float z = st.z;

        var X = Mathf.FloorToInt(x) & 0xff;
        var Y = Mathf.FloorToInt(y) & 0xff;
        var Z = Mathf.FloorToInt(z) & 0xff;
        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);
        z -= Mathf.Floor(z);
        var u = Fade(x);
        var v = Fade(y);
        var w = Fade(z);

        int aaa, aba, aab, abb, baa, bba, bab, bbb;
        aaa = perlinTable[perlinTable[perlinTable[X] + Y] + Z];
        aba = perlinTable[perlinTable[perlinTable[X] + inc(Y, period)] + Z];
        aab = perlinTable[perlinTable[perlinTable[X] + Y] + inc(Z, period)];
        abb = perlinTable[perlinTable[perlinTable[X] + inc(Y, period)] + inc(Z, period)];
        baa = perlinTable[perlinTable[perlinTable[inc(X, period)] + Y] + Z];
        bba = perlinTable[perlinTable[perlinTable[inc(X, period)] + inc(Y, period)] + Z];
        bab = perlinTable[perlinTable[perlinTable[inc(X, period)] + Y] + inc(Z, period)];
        bbb = perlinTable[perlinTable[perlinTable[inc(X, period)] + inc(Y, period)] + inc(Z, period)];

        float x1, x2, y1, y2;
        x1 = Lerp(u,
                Grad(aaa, x, y),
                Grad(baa, x - 1, y)
                );
        x2 = Lerp(u,
                Grad(aba, x, y - 1),
                Grad(bba, x - 1, y - 1)
                );

        y1 = Lerp(v, x1, x2);

        x1 = Lerp(u,
                Grad(aab, x, y, z - 1),
                Grad(bab, x - 1, y, z - 1)
                );
        x2 = Lerp(u,
                Grad(abb, x, y - 1, z - 1),
                Grad(bbb, x - 1, y - 1, z - 1)
                );
        y2 = Lerp(v, x1, x2);


        return (Lerp(w, y1, y2) + 1) * 0.5f;
    }

    ///

    public static float fBm2D(Vector2 st, float period, Noise2D _func)
    {
        if (Octaves == 0) return 0;

        float final = 0f;
        float amplitude = 0.5f;
        float maxAmplitude = 0f;

        for (int i = 0; i < Octaves; ++i)
        {
            final += _func(st, period) * amplitude;
            st.x *= Scale;
            st.y *= Scale;
            period *= Scale;
            maxAmplitude += amplitude;
            amplitude *= Persistence;
        }

        return final / maxAmplitude;
    }

    public static float fBm3D(Vector3 st, float period, Noise3D _func)
    {
        if (Octaves == 0) return 0;

        float final = 0f;
        float amplitude = 0.5f;
        float maxAmplitude = 0f;

        for(int i = 0; i < Octaves; ++i)
        {
            final += _func(st, period) * amplitude;
            st.x *= Scale;
            st.y *= Scale;
            st.z *= Scale;
            period *= Scale;
            maxAmplitude += amplitude;
            amplitude *= Persistence;
        }

        return final / maxAmplitude;
    }
}
