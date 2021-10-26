// https://www.raywenderlich.com/22027819-volumetric-light-scattering-as-a-custom-renderer-feature-in-urp
Shader "Urp/UrpGodRayShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurWidth("Blur Width", Range(0,1)) = 0.85
        _Intensity("Intensity", Range(0,1)) = 1
        _Center("Center", Vector) = (0.5,0.5,0,0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "Occluder"

            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);

                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return half4(0,0,0,1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Radial Blur"

            Blend One One

            HLSLPROGRAM
            #pragma vertex vertRadialBlur
            #pragma fragment fragRadialBlur

            #include "RadialBlur.hlsl"
            ENDHLSL
        }
    }
}
