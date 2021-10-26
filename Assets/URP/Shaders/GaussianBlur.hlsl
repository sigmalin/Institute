#ifndef UNIVERSAL_GAUSSIAN_BLUR_INCLUDED
#define UNIVERSAL_GAUSSIAN_BLUR_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    int _GaussSamples;
    real _GaussAmount;
CBUFFER_END

//bilateral blur from 
static const real gauss_filter_weights[] = { 0.14446445, 0.13543542, 0.11153505, 0.08055309, 0.05087564, 0.02798160, 0.01332457, 0.00545096 };
#define BLUR_DEPTH_FALLOFF 100.0

struct appdata
{
    real4 vertex : POSITION;
    real2 uv : TEXCOORD0;
};

struct v2f
{
    real2 uv : TEXCOORD0;
    real4 vertex : SV_POSITION;
};

v2f vertGaussianBlur(appdata v)
{
    v2f o;
    o.vertex = TransformWorldToHClip(v.vertex);
    o.uv = v.uv;
    return o;
}

real fragGaussianBlur(v2f i) : SV_Target
{
    real col = 0;
    real accumResult = 0;
    real accumWeights = 0;
    //depth at the current pixel
    real depthCenter;
    #if UNITY_REVERSED_Z
        depthCenter = SampleSceneDepth(i.uv);
    #else
        // Adjust z to match NDC for OpenGL
        depthCenter = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(i.uv));
    #endif

    for (real index = -_GaussSamples; index <= _GaussSamples; index++) {
        //we offset our uvs by a tiny amount 
        #if defined(_GAUSSIAN_BLUR_X)
            real2 uv = i.uv + real2(index * _GaussAmount / 1000, 0);
        #else
            real2 uv = i.uv + real2(0, index * _GaussAmount / 1000);
        #endif
        //sample the color at that location
        real kernelSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
        //depth at the sampled pixel
        real depthKernel;
        #if UNITY_REVERSED_Z
            depthKernel = SampleSceneDepth(uv);
        #else
        // Adjust z to match NDC for OpenGL
        depthKernel = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(uv));
    #endif
        //weight calculation depending on distance and depth difference
        real depthDiff = abs(depthKernel - depthCenter);
        real r2 = depthDiff * BLUR_DEPTH_FALLOFF;
        real g = exp(-r2 * r2);
        real weight = g * gauss_filter_weights[abs(index)];
        //sum for every iteration of the color and weight of this sample 
        accumResult += weight * kernelSample;
        accumWeights += weight;
    }
    //final color
    col = accumResult / accumWeights;

    return col;
}

#endif
