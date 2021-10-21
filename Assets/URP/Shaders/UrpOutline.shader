// https://zhuanlan.zhihu.com/p/109101851
// https://zhuanlan.zhihu.com/p/95986273
Shader "Urp/UrpOutline"
{
    Properties
    {
        _OutlineWidth("Outline Width", Range(0.01, 1)) = 0.24
        _OutlineColor("OutLine Color", Color) = (0.5,0.5,0.5,1)
        _Aspect("Camera Aspect", FLOAT) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Outline" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            Cull Front

            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                half4 _OutlineColor;
                float _Aspect;
            CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
                half3 normal : NORMAL;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);

                float4 pos = TransformObjectToHClip(v.vertex.xyz);

                float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                float3 ndcNormal = normalize(mul((float3x3)UNITY_MATRIX_P, viewNormal)) * pos.w;

                ndcNormal.y *= _Aspect;

                pos.xy += 0.01 * _OutlineWidth * ndcNormal.xy;
                o.vertex = pos;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
