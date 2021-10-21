Shader "Urp/UrpToonShader"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _BaseMap("Base Map", 2D) = "white"
        [NoScaleOffset] _bumpMap("Bumpmap", 2D) = "bump" {}

        _BrightThreshold1("Bright Threshold 1", Range(-1,1)) = 0
        _BrightThreshold2("Bright Threshold 2", Range(-1,1)) = -0.5

        _FresnelColor("Fresnel Color", Color) = (0.1, 0.1, 0.1, 1)

        _Roughness("Roughness", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            // must before CBUFFER block
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_bumpMap);
            SAMPLER(sampler_bumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _FresnelColor;
                float _BrightThreshold1;
                float _BrightThreshold2;
                float _Roughness;
            CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                half3 normal : NORMAL;
                half4 tangent : TANGENT;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;

                float4 posWorld : TEXCOORD1;

                half3 normalDir : TEXCOORD2;
                half3 tangentDir : TEXCOORD3;
                half3 bitangentDir : TEXCOORD4;

                float fogCoord : TEXCOORD5;
                float4 shadowCoord : TEXCOORD6;
            };

            float Sigmoid(float x, float sharpness, float center)
            {
                return 1 / (1 + exp(-sharpness * 10 * (x - center)));
            }

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);

                o.posWorld = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = mul(unity_MatrixVP, o.posWorld);

                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);

                o.normalDir = mul((float3x3)UNITY_MATRIX_M, v.normal);
                o.tangentDir = normalize(mul(UNITY_MATRIX_M, float4(v.tangent.xyz, 0.0)).xyz);
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);

                o.fogCoord = ComputeFogFactor(o.vertex.z);

#if SHADOWS_SCREEN
                o.shadowCoord = ComputeScreenPos(o.vertex);
#else
                o.shadowCoord = TransformWorldToShadowCoord(o.posWorld.xyz);
#endif
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 nor = SAMPLE_TEXTURE2D(_bumpMap, sampler_bumpMap, i.uv).xyz * 2 - 1;//UnpackNormal(SAMPLE_TEXTURE2D(_bumpMap, sampler_bumpMap, i.uv));
                float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
                float3 normalDirection = normalize(mul(nor, tangentTransform));

                Light light = GetMainLight(i.shadowCoord);

                float3 lightDirection = light.direction;
                float NdotL = dot(normalDirection, lightDirection);

                float3 carmeraPos = GetCameraPositionWS();
                float3 viewDirection = normalize(carmeraPos - i.posWorld.xyz);
                float NdotV = clamp(dot(normalDirection, viewDirection), 0, 1);
                float VdotL = dot(viewDirection, lightDirection);

                float theta = (1 - NdotV);
                theta *= theta * theta;

                float3 fresnel = _FresnelColor.rgb * theta; // Schlick re-write
                fresnel *= (1 - VdotL) * 0.5;


                float smoothness = 1 - _Roughness;
                float sharpness = 4 * smoothness * smoothness;

                float threshhold1 = (_BrightThreshold1 + 1) * 0.5;
                float threshhold2 = (_BrightThreshold2 + 1) * 0.5;

                float bright1 = (1 + threshhold1) * 0.5;
                float bright2 = (threshhold1 + threshhold2) * 0.5;
                float bright3 = (threshhold2) * 0.5;

                float S1 = Sigmoid(NdotL, sharpness, _BrightThreshold1);
                float S2 = Sigmoid(NdotL, sharpness, _BrightThreshold2);

                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);

                color.rgb *= (S1 * bright1) +
                             ((S2 - S1) * bright2) +
                             ((1 - S2) * bright3);

                color.rgb += fresnel;

                color.rgb = MixFog(color.rgb, i.fogCoord);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull  Back

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
}
