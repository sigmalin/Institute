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

float3 perlinNoiseDeriv(float2 p, float seed)
{
    // Calculate 2D integer coordinates i and fraction p.
    float2 i = floor(p);
    float2 f = p - i;

    // Get weights from the coordinate fraction
    float2 w = f * f * f * (f * (f * 6 - 15) + 10); // 6f^5 - 15f^4 + 10f^3
    float4 w4 = float4(1, w.x, w.y, w.x * w.y);

    // Get the derivative dw/df
    float2 dw = f * f * (f * (f * 30 - 60) + 30); // 30f^4 - 60f^3 + 30f^2

    // Get the derivative d(w*f)/df
    float2 dwp = f * f * f * (f * (f * 36 - 75) + 40); // 36f^5 - 75f^4 + 40f^3

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

    // Calculate the derivatives dn/dx and dn/dy
    float dx = (g1.x + (g1.z - g1.x) * w.y) + ((g2.y - g1.y) * f.y - g2.x +
        ((g1.y - g2.y - g1.w + g2.w) * f.y + g2.x + g1.w - g2.z - g2.w) * w.y) *
        dw.x + ((g2.x - g1.x) + (g1.x - g2.x - g1.z + g2.z) * w.y) * dwp.x;
    float dy = (g1.y + (g2.y - g1.y) * w.x) + ((g1.z - g1.x) * f.x - g1.w + ((g1.x -
        g2.x - g1.z + g2.z) * f.x + g2.x + g1.w - g2.z - g2.w) * w.x) * dw.y +
        ((g1.w - g1.y) + (g1.y - g2.y - g1.w + g2.w) * w.x) * dwp.y;

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

float swissTurbulence(float2 p, float seed, int octaves,
    float amp = 1000.0, float freq = 0.0005, float lacunarity = 2.0, float gain = 0.5, 
    float warp = 0.15)
{
    float sum = 0;
    float2 dsum = float2(0, 0);
    for (int i = 0; i < octaves; i++)
    {
        float3 n = perlinNoiseDeriv((p + warp * dsum) * freq, seed + i);
        sum += amp * (1 - abs(n.x));
        dsum += amp * n.yz * -n.x;
        freq *= lacunarity;
        amp *= gain * saturate(sum);
    }
    return sum;
}

float jordanTurbulence(float2 p, float seed, int octaves, 
    float amp = 1000.0, float freq = 0.0005, float lacunarity = 2.0, float gain = 0.5,
    float warp0 = 0.4, float warp = 0.35,
    float damp0 = 1.0, float damp = 0.8,
    float damp_scale = 1.0)
{
    float3 n = perlinNoiseDeriv(p, seed);
    float3 n2 = n * n.x;
    float sum = n2.x;
    float2 dsum_warp = warp0 * n2.yz;
    float2 dsum_damp = damp0 * n2.yz;

    float damped_amp = amp * gain;

    for (int i = 1; i < octaves; i++)
    {
        n = perlinNoiseDeriv(p * freq + dsum_warp.xy, seed + i / 256.0);
        n2 = n * n.x;
        sum += damped_amp * n2.x;
        dsum_warp += warp * n2.yz;
        dsum_damp += damp * n2.yz;
        freq *= lacunarity;
        amp *= gain;
        damped_amp = amp * (1 - damp_scale / (1 + dot(dsum_damp, dsum_damp)));
    }
    return sum;
}
#endif
