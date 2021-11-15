#ifndef _PERLIN_NOISE_H
#define _PERLIN_NOISE_H

StructuredBuffer<int> _Perm;

int _Octaves;

float Grad(int hash, float x, float y, float z)
{
    int h = hash & 15;
    float u = (h < 8) ? x : y;
    float v = (h < 4) ? y : (h == 12 || h == 14) ? x : z;
    return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
}

float Lerp(float t, float a, float b)
{
    return a + t * (b - a);
}

float Fade(float t)
{
    return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}

float Noise(float3 vec)
{
    int X = (int)floor(vec.x) & 255;
    int Y = (int)floor(vec.y) & 255;
    int Z = (int)floor(vec.z) & 255;

    vec.x -= floor(vec.x);
    vec.y -= floor(vec.y);
    vec.z -= floor(vec.z);

    float u = Fade(vec.x);
    float v = Fade(vec.y);
    float w = Fade(vec.z);

    int A, AA, AB, B, BA, BB;

    A = _Perm[X + 0] + Y; AA = _Perm[A] + Z; AB = _Perm[A + 1] + Z;
    B = _Perm[X + 1] + Y; BA = _Perm[B] + Z; BB = _Perm[B + 1] + Z;

    return Lerp(w, Lerp(v, Lerp(u, Grad(_Perm[AA + 0], vec.x + 0, vec.y + 0, vec.z + 0),
        Grad(_Perm[BA + 0], vec.x - 1, vec.y + 0, vec.z + 0)),
        Lerp(u, Grad(_Perm[AB + 0], vec.x + 0, vec.y - 1, vec.z + 0),
            Grad(_Perm[BB + 0], vec.x - 1, vec.y - 1, vec.z + 0))),
        Lerp(v, Lerp(u, Grad(_Perm[AA + 1], vec.x + 0, vec.y + 0, vec.z - 1),
            Grad(_Perm[BA + 1], vec.x - 1, vec.y + 0, vec.z - 1)),
            Lerp(u, Grad(_Perm[AB + 1], vec.x + 0, vec.y - 1, vec.z - 1),
                Grad(_Perm[BB + 1], vec.x - 1, vec.y - 1, vec.z - 1))));
}

float PerlinNoise(float3 vec)
{
    float result = 0;
    float amp = 1.0;

    result += Noise(vec) * amp;
    vec *= 2.0;
    amp *= 0.5;

    for (int i = 0; i < _Octaves; i++)
    {
        result += Noise(vec) * amp;
        vec *= 2.0;
        amp *= 0.5;
    }

    return result;
}


#endif
