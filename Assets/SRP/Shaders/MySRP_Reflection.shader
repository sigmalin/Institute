Shader "SRP/Reflection"
{
    Properties
    {
		_EnviornmentMap ("Enviornment map", 2D) = "black" {}
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

				float attenuation = ShadowAttenuation(i.posWorld.xyz);
				col.rgb *= (attenuation + 1) * 0.5;
				
				return col;
            }
            ENDHLSL
        }

		Pass
        {
			Name "DUALPARABOLOID"

            Tags { "LightMode"="DualParaboloid" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling // if don't use invWorld2Object 

			#pragma target 3.5
			
            #include "MyUnlit.hlsl"
			#include "ToneMapping.hlsl"
			#include "DualParaboloid.hlsl"

			CBUFFER_START(_DualParaboloid)
				float4x4 unity_MatrixV;
				float4 _DualParaboloidParams;
				float4 _DualParaboloidCameraPos;
			CBUFFER_END

			TEXTURE2D(_EnviornmentMap);
			SAMPLER(sampler_EnviornmentMap);

			struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
				float3 normalDir : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
            };


            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.posWorld = mul(UNITY_MATRIX_M, v.vertex);

				o.pos.xyz = mul(unity_MatrixV, float4(o.posWorld.xyz, 1)).xyz;
				// Right-handed to Left-handed coordinate system 
				o.pos.z = -o.pos.z; 

				float L = length(o.pos.xyz);
				o.pos.xyz /= L;				
				o.pos.xy /= 1 + o.pos.z;

				//handle upside-down, https://docs.unity3d.com/2020.2/Documentation/Manual/SL-PlatformDifferences.html
				if(_DualParaboloidParams.x < 0)
					o.pos.y = -o.pos.y;
				o.pos.w = 1;

				//o.vertex.z = (L - _Near) / (_Far-_Near) * lerp(1, -1, o.vertex.z < _Bias);
				o.pos.z = (L - _DualParaboloidParams.y) * _DualParaboloidParams.w * lerp(1, -1, o.pos.z < -0.05);
				
				// Convert to Clip Space
				if(UNITY_NEAR_CLIP_VALUE  == 1) // for DirectX (near(1),far(0))
					o.pos.z = 1 - o.pos.z;
				else							// for OpenGL (near(-1),far(1))
					o.pos.z = o.pos.z - 1;

				o.normalDir = mul((float3x3)UNITY_MATRIX_M, v.normal);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
				float3 normalDirection = normalize(i.normalDir);
				float3 viewDirection = normalize(_DualParaboloidCameraPos.xyz - i.posWorld.xyz);

				float NdotV = dot(normalDirection, viewDirection);

				float3 reflectDirection = 2.0 * NdotV * normalDirection - viewDirection;

				float4 col = SAMPLE_DUALPARBOLOID_LOD (_EnviornmentMap, sampler_EnviornmentMap, reflectDirection, 0);
				col.rgb = Inv_Reinhard_tone_mapping(col.rgb);

				float attenuation = ShadowAttenuation(i.posWorld.xyz);
				col.rgb *= (attenuation + 1) * 0.5;
				
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
