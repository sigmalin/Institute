#ifndef UNIVERSAL_RADIAL_BLUR_INCLUDED
#define UNIVERSAL_RADIAL_BLUR_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#define NUM_SAMPLES 100

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    float4 _Center;
    float _BlurWidth;
    float _Intensity;
CBUFFER_END

v2f vertRadialBlur(appdata v)
{
    v2f o;
    o.vertex = TransformObjectToHClip(v.vertex.xyz);
    o.uv = v.uv;
    return o;
}

half4 fragRadialBlur(v2f i) : SV_Target
{
    half4 col = half4(0.0f, 0.0f, 0.0f, 1.0f);

    float2 ray = i.uv - _Center.xy;

    for (int i = 0; i < NUM_SAMPLES; i++)
    {
        float scale = 1.0f - _BlurWidth * (float(i) /
            float(NUM_SAMPLES - 1));
        col.rgb += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (ray * scale) +
            _Center.xy).rgb / float(NUM_SAMPLES);
    }

    col.rgb *= _Intensity;

    return col;
}

#endif
