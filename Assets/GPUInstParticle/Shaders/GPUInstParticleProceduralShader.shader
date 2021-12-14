Shader "GPUInstParticle/GPUInstParticleProceduralShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5  // for compute shader support

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #include "GPUInstParticleData.cginc"

            #if SHADER_TARGET >= 45
            StructuredBuffer<particle> particleBuffer;
            #endif

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 color : COLOR;
            };

            v2f vert (uint vertex_id: SV_VertexID, uint instanceID : SV_InstanceID)
            {
                v2f o;

#if SHADER_TARGET >= 45
                particle data = particleBuffer[instanceID];
#else
                particle data = 0;
#endif

                float3 posWorld = data.position;
                o.vertex = mul(unity_MatrixVP, float4(posWorld, 1));
                o.color = data.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                return half4(i.color, 1);
            }
            ENDHLSL
        }
    }
}
