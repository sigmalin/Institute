Shader "Urp/UrpDecalShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEQUAL

            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float3 viewDir : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);

                float4 posWorld = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = mul(unity_MatrixVP, posWorld);
                                
                o.viewDir = posWorld.xyz - _WorldSpaceCameraPos.xyz;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 ray = normalize(i.viewDir);
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                float depth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams); // [0, _ProjectionParams.z]
                float3 camForward = -UNITY_MATRIX_V[2].xyz; // = mul(UNITY_MATRIX_V, float4(0,0,-1,0))
                float viewDistance = depth / dot(ray, camForward); // = depth / cosTheta

                float3 rayHitPos = _WorldSpaceCameraPos.xyz + ray * viewDistance;
                float3 objectPos = mul(unity_WorldToObject, float4(rayHitPos, 1)).xyz;

                // clip out of object range, for default unity cube [-0.5, 0.5]
                clip(0.5 - abs(objectPos));

                float2 uv = objectPos.xz + 0.5; // remap [-0.5, 0.5] to [0, 1]

                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            }
            ENDHLSL
        }
    }
}
