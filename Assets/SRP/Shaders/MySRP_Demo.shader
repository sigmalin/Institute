Shader "SRP/Demo"
{
    Properties
    {
		_EnviornmentMap ("Enviornment map", 2D) = "black" {}
		_ReflectMap ("Relfect map", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling // if don't use invWorld2Object 

			#pragma target 3.5
			
            #include "MyUnlit.hlsl"
			#include "ToneMapping.hlsl"
			#include "DualParaboloid.hlsl"

			TEXTURE2D(_EnviornmentMap);
			SAMPLER(sampler_EnviornmentMap);

			TEXTURE2D(_ReflectMap);
			SAMPLER(sampler_ReflectMap);

			struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 normalDir : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
            };


            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.posWorld = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = mul(unity_MatrixVP, o.posWorld);
				o.normalDir = mul((float3x3)UNITY_MATRIX_M, v.normal);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
				float3 normalDirection = normalize(i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

				float NdotV = dot(normalDirection, viewDirection);

				float3 reflectDirection = 2.0 * NdotV * normalDirection - viewDirection;

				float4 col = SAMPLE_DUALPARBOLOID_LOD (_EnviornmentMap, sampler_EnviornmentMap, reflectDirection, 0);
				col.rgb = Inv_Reinhard_tone_mapping(col.rgb);

				float4 reflect = SAMPLE_DUALPARBOLOID_LOD (_ReflectMap, sampler_ReflectMap, reflectDirection, 0);
				col.rgb = lerp(col.rgb, reflect.rgb, reflect.a);
				
				return col;
            }
            ENDHLSL
        }

		Pass
        {
            Name "SHADOWCASTER"

            Tags { "LightMode"="ShadowCaster" }

            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling

			#pragma target 3.5

            #include "MyShadow.hlsl"
            ENDHLSL
        }

		Pass
        {
            Name "DEPTHONLY"

            Tags { "LightMode"="DepthOnly" }

            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vertDepthOnly
            #pragma fragment fragDepthOnly
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling

			#pragma target 3.5

            #include "MyDepthOnly.hlsl"
            ENDHLSL
        }
    }
}
