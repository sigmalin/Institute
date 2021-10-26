// https://valeriomarty.medium.com/raymarched-volumetric-lighting-in-unity-urp-e7bc84d31604
Shader "Urp/UrpVolumetricLightShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "Volumetric Light Raymarching"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

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

            // must before CBUFFER block
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                real4x4 _ClipToWorld;
                real _Scattering;
                real _Steps;
                real _MaxDistance;
                real _JitterVolumetric;
            CBUFFER_END

            real ShadowAtten(real3 worldPosition)
            {
                return MainLightRealtimeShadow(TransformWorldToShadowCoord(worldPosition));
            }

            real3 GetWorldPos(real2 uv) {
#if UNITY_REVERSED_Z
                real depth = SampleSceneDepth(uv);
#else
                // Adjust z to match NDC for OpenGL
                real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(uv));
#endif
                return ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);
            }

            // Mie scaterring approximated with Henyey-Greenstein phase function.
            real ComputeScattering(real lightDotView)
            {
                real result = 1.0f - _Scattering * _Scattering;
                result /= (4.0f * PI * pow(1.0f + _Scattering * _Scattering - (2.0f * _Scattering) * lightDotView, 1.5f));
                return result;
            }

            // return [0,1]
            real random(real2 p) {
                return frac(sin(dot(p, real2(41, 289))) * 45758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            real frag(v2f i) : SV_Target
            {
                real3 cameraPos = _WorldSpaceCameraPos.xyz;

                real3 lightDirection = -_MainLightPosition.xyz; // invert for mie scaterring

                real3 worldPos = GetWorldPos(i.uv);
                real3 vec = worldPos - cameraPos;
                real3 ray = normalize(vec);
                real eyeDepth = length(vec);//LinearEyeDepth(SampleSceneDepth(i.uv), _ZBufferParams); // [0, _ProjectionParams.z]
                real rayLnegth = min(eyeDepth, _MaxDistance);

                real stepLength = rayLnegth / _Steps;
                real3 step = ray * stepLength;

                real rayStartOffset = random(i.uv) * stepLength * _JitterVolumetric / 100;
                real3 currentPosition = cameraPos + rayStartOffset * ray;

                real accumFog = 0;

                for (real j = 0; j < _Steps - 1; j++)
                {
                    real shadowMapValue = ShadowAtten(currentPosition);

                    //if it is in light
                    if (shadowMapValue > 0) {
                        real kernelColor = ComputeScattering(dot(ray, lightDirection));
                        accumFog += kernelColor;
                    }
                    currentPosition += step;
                }

                accumFog /= _Steps;

                return accumFog;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Gaussian Blur X"

            HLSLPROGRAM
            #pragma vertex vertGaussianBlur
            #pragma fragment fragGaussianBlur
            #pragma multi_compile _ _GAUSSIAN_BLUR_X

            #define _GAUSSIAN_BLUR_X

            #include "GaussianBlur.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "Gaussian Blur Y"

            HLSLPROGRAM
            #pragma vertex vertGaussianBlur
            #pragma fragment fragGaussianBlur
            #pragma multi_compile _ _GAUSSIAN_BLUR_X

            #include "GaussianBlur.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "SampleDepth"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformWorldToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            real frag(v2f i) : SV_Target
            {
                #if UNITY_REVERSED_Z
                    real depth = SampleSceneDepth(i.uv);
                #else
                // Adjust z to match NDC for OpenGL
                real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(i.uv));
                #endif
                return depth;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Compositing"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_Volumetric);
            SAMPLER(sampler_Volumetric);
            TEXTURE2D(_LowResDepth);
            SAMPLER(sampler_LowResDepth);

            CBUFFER_START(UnityPerMaterial)
                real _Intensity;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformWorldToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            real3 frag(v2f i) : SV_Target
            {
                real col = 0;
                //based on https://eleni.mutantstargoat.com/hikiko/on-depth-aware-upsampling/ 

                int offset = 0;
                real d0 = SampleSceneDepth(i.uv);

                /* calculating the distances between the depths of the pixels
                * in the lowres neighborhood and the full res depth value
                * (texture offset must be compile time constant and so we
                * can't use a loop)
                */
                real d1 = _LowResDepth.Sample(sampler_LowResDepth, i.uv, int2(0, 1)).x;
                real d2 = _LowResDepth.Sample(sampler_LowResDepth, i.uv, int2(0, -1)).x;
                real d3 = _LowResDepth.Sample(sampler_LowResDepth, i.uv, int2(1, 0)).x;
                real d4 = _LowResDepth.Sample(sampler_LowResDepth, i.uv, int2(-1, 0)).x;

                d1 = abs(d0 - d1);
                d2 = abs(d0 - d2);
                d3 = abs(d0 - d3);
                d4 = abs(d0 - d4);

                real dmin = min(min(d1, d2), min(d3, d4));

                if (dmin == d1)
                offset = 0;

                else if (dmin == d2)
                offset = 1;

                else if (dmin == d3)
                offset = 2;

                else  if (dmin == d4)
                offset = 3;

                col = 0;
                switch (offset) {
                    case 0:
                    col = _Volumetric.Sample(sampler_Volumetric, i.uv, int2(0, 1));
                    break;
                    case 1:
                    col = _Volumetric.Sample(sampler_Volumetric, i.uv, int2(0, -1));
                    break;
                    case 2:
                    col = _Volumetric.Sample(sampler_Volumetric, i.uv, int2(1, 0));
                    break;
                    case 3:
                    col = _Volumetric.Sample(sampler_Volumetric, i.uv, int2(-1, 0));
                    break;
                    default:
                    col = _Volumetric.Sample(sampler_Volumetric, i.uv);
                    break;
                }


                real3 finalShaft = saturate(col) * _Intensity * _MainLightColor.rgb;

                real3 screen = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return (screen + finalShaft);// / (1 + finalShaft);
            }
            ENDHLSL
        }
    }
}
