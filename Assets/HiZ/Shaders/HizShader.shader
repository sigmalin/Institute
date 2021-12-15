Shader "HiZ/MyHizShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType" = "transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5  // for compute shader support

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #if SHADER_TARGET >= 45
            StructuredBuffer<float3> positionBuffer;
            #endif

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;

#if SHADER_TARGET >= 45
                float3 posWorld = positionBuffer[instanceID];
#else
                float3 posWorld = 0;
#endif

                posWorld += v.vertex.xyz * 0.1;

                o.vertex = mul(unity_MatrixVP, float4(posWorld, 1));
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
#if SHADER_TARGET >= 45
                return half4(1, 1, 1, 1);
#else
                return half4(0, 0, 0, 1);
                #endif
            }
            ENDHLSL
        }
    }
}
