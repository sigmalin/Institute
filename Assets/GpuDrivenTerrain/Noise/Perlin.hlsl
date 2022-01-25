//https://www.decarpentier.nl/scape-procedural-basics
#ifndef _PERLIN_NOISE_H
#define _PERLIN_NOISE_H

StructuredBuffer<int> _PerlinPerm;
StructuredBuffer<int> _PerlinGrad;

int4 GetPerm(int2 st)
{
    int index = (st.y & 0xff) * 256 + (st.x & 0xff);
    int perm = _PerlinPerm[index];
    return int4((perm >> 24) & 0xff, (perm >> 16) & 0xff, (perm >> 8) & 0xff, perm & 0xff);
}

float4 GetGrad(int2 st)
{
    int index = (st.y & 0xff) * 256 + (st.x & 0xff);
    int perm = _PerlinGrad[index];
    return float4(((perm >> 24) & 0x03) - 1.0, ((perm >> 16) & 0x03) - 1.0, ((perm >> 8) & 0x03) - 1.0, (perm & 0x03) - 1.0);
}

float perlinNoise(float2 p, float seed)
{
    // Calculate 2D integer coordinates i and fraction p.
    float2 i = floor(p);
    float2 f = p - i;

    // Get weights from the coordinate fraction
    float2 w = f * f * f * (f * (f * 6 - 15) + 10);
    float4 w4 = float4(1, w.x, w.y, w.x * w.y);

    // Get the four randomly permutated indices from the noise lattice nearest to
    // p and offset these numbers with the seed number.
    float4 perm = GetPerm(i) + seed;

    // Permutate the four offseted indices again and get the 2D gradient for each
    // of the four permutated coordinates-seed pairs.
    float4 g1 = GetGrad(perm.xy);
    float4 g2 = GetGrad(perm.zw);

    // Evaluate the four lattice gradients at p
    float a = dot(g1.xy, f);
    float b = dot(g2.xy, f + float2(-1, 0));
    float c = dot(g1.zw, f + float2(0, -1));
    float d = dot(g2.zw, f + float2(-1, -1));

    // Bi-linearly blend between the gradients, using w4 as blend factors.
    float4 grads = float4(a, b - a, c - a, a - b - c + d);
    float n = dot(grads, w4);

    // Return the noise value, roughly normalized in the range [-1, 1]
    return n * 1.5;
}

float3 perlinNoisePseudoDeriv(float2 p, float seed)
{
    // Calculate 2D integer coordinates i and fraction p.
    float2 i = floor(p);
    float2 f = p - i;

    // Get weights from the coordinate fraction
    float2 w = f * f * f * (f * (f * 6 - 15) + 10);
    float4 w4 = float4(1, w.x, w.y, w.x * w.y);

    // Get pseudo derivative weights
    float2 dw = f * f * (f * (30 * f - 60) + 30);

    // Get the four randomly permutated indices from the noise lattice nearest to
    // p and offset these numbers with the seed number.
    float4 perm = GetPerm(i) + seed;

    // Permutate the four offseted indices again and get the 2D gradient for each
    // of the four permutated coordinates-seed pairs.
    float4 g1 = GetGrad(perm.xy);
    float4 g2 = GetGrad(perm.zw);

    // Evaluate the four lattice gradients at p
    float a = dot(g1.xy, f);
    float b = dot(g2.xy, f + float2(-1, 0));
    float c = dot(g1.zw, f + float2(0, -1));
    float d = dot(g2.zw, f + float2(-1, -1));

    // Bi-linearly blend between the gradients, using w4 as blend factors.
    float4 grads = float4(a, b - a, c - a, a - b - c + d);
    float n = dot(grads, w4);

    // Calculate pseudo derivates
    float dx = dw.x * (grads.y + grads.w * w.y);
    float dy = dw.y * (grads.z + grads.w * w.x);

    // Return the noise value, roughly normalized in the range [-1, 1]
    // Also return the pseudo dn/dx and dn/dy, scaled by the same factor
    return float3(n, dx, dy) * 1.5;
}

float turbulence(float2 p, float seed, int octaves,
    float amp = 1000.0, float freq = 0.0005, float lacunarity = 2.0, float gain = 0.5)
{
    float sum = 0;
    for (int i = 0; i < octaves; i++)
    {
        float n = perlinNoise(p * freq, seed + i / 256.0);
        sum += n * amp;
        freq *= lacunarity;
        amp *= gain;
    }
    return sum;
}

float billowedTurbulence(float2 p, float seed, int octaves,
    float amp = 1000.0, float freq = 0.0005, float lacunarity = 2.0, float gain = 0.5)
{
    float sum = 0;
    for (int i = 0; i < octaves; i++)
    {
        float n = abs(perlinNoise(p * freq, seed + i / 256.0));
        sum += n * amp;
        freq *= lacunarity;
        amp *= gain;
    }
    return sum;
}

float RidgedTurbulence(float2 p, float seed, int octaves,
    float amp = 1000.0, float freq = 0.0005, float lacunarity = 2.0, float gain = 0.5)
{
    float sum = 0;
    for (int i = 0; i < octaves; i++)
    {
        float n = 1 - abs(perlinNoise(p * freq, seed + i / 256.0));
        sum += n * amp;
        freq *= lacunarity;
        amp *= gain;
    }
    return sum;
}


float iqTurbulence(float2 p, float seed, int octaves,
    float amp = 1000.0, float freq = 0.0005, float lacunarity = 2.0, float gain = 0.5)
{
    float sum = 0;
    float2 dsum = float2(0, 0);
    for (int i = 0; i < octaves; i++)
    {
        float3 n = perlinNoisePseudoDeriv(p * freq, seed + i / 256.0);
        dsum += n.yz;
        sum += amp * n.x / (1 + dot(dsum, dsum));
        freq *= lacunarity;
        amp *= gain;
    }
    return sum;
}

#endif
