#ifndef UNIVERSAL_DUAL_KAWASE_INCLUDED
#define UNIVERSAL_DUAL_KAWASE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    real _Offset;
    real2 _MainTex_TexelSize;
CBUFFER_END

struct appdata
{
    real4 vertex : POSITION;
    real2 uv : TEXCOORD0;
};

struct v2f_downSample
{
    real2 uv : TEXCOORD0;
    real4 uv01 : TEXCOORD1;
    real4 uv23 : TEXCOORD2;
    real4 vertex : SV_POSITION;
};

struct v2f_upSample
{
    real2 uv : TEXCOORD0;
    real4 uv01 : TEXCOORD1;
    real4 uv23 : TEXCOORD2;
    real4 uv45 : TEXCOORD3;
    real4 uv67 : TEXCOORD4;
    real4 vertex : SV_POSITION;
};

v2f_downSample vertDownSample(appdata v)
{
    v2f_downSample o;
    o.vertex = TransformWorldToHClip(v.vertex.xyz);
    o.uv = v.uv;

#if UNITY_UV_STARTS_TOP
    o.uv.y = 1 - o.uv.y;
#endif

    real2 offset = real2(1 + _Offset, 1 + _Offset);
    o.uv01.xy = o.uv - _MainTex_TexelSize * offset;
    o.uv01.zw = o.uv + _MainTex_TexelSize * offset;
    o.uv23.xy = o.uv - float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * offset;
    o.uv23.zw = o.uv + float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * offset;

    return o;
}

half4 fragDownSample(v2f_downSample i) : SV_Target
{
    half4 sum = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv) * 4;
    sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv01.xy);
    sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv01.zw);
    sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv23.xy);
    sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv23.zw);

    return sum * 0.125;
}

v2f_upSample vertUpSample(appdata v)
{
    v2f_upSample o;
    o.vertex = TransformWorldToHClip(v.vertex.xyz);
    o.uv = v.uv;

#if UNITY_UV_STARTS_TOP
    o.uv.y = 1 - o.uv.y;
#endif

    real2 texSize = _MainTex_TexelSize * 0.5;
    real2 offset = real2(1 + _Offset, 1 + _Offset);
    o.uv01.xy = o.uv + real2(-texSize.x * 2, 0) * offset;
    o.uv01.zw = o.uv + real2(-texSize.x, texSize.y) * offset;
    o.uv23.xy = o.uv + real2(0, texSize.y * 2) * offset;
    o.uv23.zw = o.uv + texSize * offset;
    o.uv45.xy = o.uv + real2(texSize.x * 2, 0) * offset;
    o.uv45.zw = o.uv + real2(texSize.x, -texSize.y) * offset;
    o.uv67.xy = o.uv + real2(0, -texSize.y * 2) * offset;
    o.uv67.zw = o.uv - texSize * offset;

    return o;
}

half4 fragUpSample(v2f_upSample i) : SV_Target
{
    half4 sum = 0;
    sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv01.xy);
    sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv01.zw) * 2;
    sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23.xy);
    sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23.zw) * 2;
    sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv45.xy);
    sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv45.zw) * 2;
    sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv67.xy);
    sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv67.zw) * 2;

    return sum * 0.0833;
}
#endif
