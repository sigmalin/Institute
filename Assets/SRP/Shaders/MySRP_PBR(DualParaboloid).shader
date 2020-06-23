Shader "SRP/PBR(DualParaboloid)"
{
    Properties
    {
		_baseColor ("Base Color", 2D) = "white" {}
		_Tint ("Tint", Color) = (1,1,1,1)
		[NoScaleOffset] _bumpMap ("Bumpmap", 2D) = "bump" {}

		_roughness ("perceptual roughness", Range(0.089,1)) = 0.5 // avoid half-float(fp16) issue, 0.045 for single precision floats (fp32). 
		_metallic ("metallic", Range(0,1)) = 0

		[NoScaleOffset] _IrradianceD ("Diffuse Irradiance (LDR)", 2D) = "grey" {}
		[NoScaleOffset] _PrefiliterEnv ("Prefiliter Enviornment Map (LDR)", 2D) = "grey" {}
		[NoScaleOffset] [Linear] _IntegrateBRDF ("Integrate BRDF", 2D) = "grey" {}		
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
			#pragma multi_compile _ _SHADOWS_SOFT
			
			#pragma target 3.5
			//#pragma prefer_hlslcc gles

			#define _DUAL_PARABOLOID_
			
            #include "MyUnlit.hlsl"
			#include "MyPBR.hlsl"

			TEXTURE2D(_baseColor);
			SAMPLER(sampler_baseColor);

			TEXTURE2D(_bumpMap);
			SAMPLER(sampler_bumpMap);

			CBUFFER_START(UnityPerMaterial)
				float4 _baseColor_ST;
				float4 _Tint;
				half _roughness;
				half _metallic;
			CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
                float4 tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
				float4 posWorld : TEXCOORD6;
            };

            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.posWorld = mul(UNITY_MATRIX_M, v.vertex);
                o.pos = mul(unity_MatrixVP, o.posWorld);
				o.normalDir = mul((float3x3)UNITY_MATRIX_M, v.normal);
                o.uv = v.uv * _baseColor_ST.xy +_baseColor_ST.zw;
				o.tangentDir = normalize( mul( UNITY_MATRIX_M, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
				float4 col = SAMPLE_TEXTURE2D(_baseColor, sampler_baseColor, i.uv);
				
				float3 tint = _Tint.rgb;

				#ifdef UNITY_COLORSPACE_GAMMA
				col.rgb = pow(abs(col.rgb), 2.2);
				tint = pow(abs(tint), 2.2);
				#endif

				col.rgb *= tint;

				float3 nor = SAMPLE_TEXTURE2D(_bumpMap, sampler_bumpMap, i.uv).xyz * 2 - 1;//UnpackNormal(SAMPLE_TEXTURE2D(_bumpMap, sampler_bumpMap, i.uv));
				float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
				float3 normalDirection = normalize(mul( nor, tangentTransform ));

				float3 lightDirection = _LightDirection.xyz;
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

				float NdotL = clamp(dot(normalDirection, lightDirection), 0, 1);				
				float NdotV = dot(normalDirection, viewDirection);
				
				float3 halfDirection = normalize(viewDirection+lightDirection);
				float LdotH = dot(lightDirection, halfDirection);
				float NdotH = dot(normalDirection, halfDirection);

				float alphaRoughness = _roughness * _roughness;

				AlbedoAndFresnelFromMetallic(col.rgb, _metallic, albedo, f0, f90)

				BRDF_Data brdf;
				brdf.albedo = albedo;
				brdf.F0 = f0;
				brdf.F90 = f90;
				brdf.NdotL = NdotL;
				brdf.NdotV = NdotV;
				brdf.LdotH = LdotH;
				brdf.NdotH = NdotH;
				brdf.alphaRoughness = alphaRoughness;

				float3 directLight = BRDF(brdf);

				float attenuation = ShadowAttenuation(i.posWorld.xyz);
				directLight *= attenuation;

				IBL_Data ibl;
				ibl.albedo = albedo;
				ibl.F0 = f0;
				ibl.normalDirection = normalDirection;
				ibl.reflectDirection = 2.0 * NdotV * normalDirection - viewDirection;
				ibl.NdotV = NdotV;
				ibl.alphaRoughness = alphaRoughness;
				ibl.metallic = _metallic;

				float3 indirectLight = IBL(ibl);

				col.rgb = directLight + indirectLight;
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
			#pragma multi_compile _ _SHADOWS_SOFT
			
			#pragma target 3.5
			//#pragma prefer_hlslcc gles

			#define _DUAL_PARABOLOID_
			
            #include "MyUnlit.hlsl"
			#include "MyPBR.hlsl"

			CBUFFER_START(_DualParaboloid)
				float4x4 unity_MatrixV;
				float4 _DualParaboloidParams;
				float4 _DualParaboloidCameraPos;
			CBUFFER_END

			TEXTURE2D(_baseColor);
			SAMPLER(sampler_baseColor);

			TEXTURE2D(_bumpMap);
			SAMPLER(sampler_bumpMap);

			CBUFFER_START(UnityPerMaterial)
				float4 _baseColor_ST;
				float4 _Tint;
				half _roughness;
				half _metallic;
			CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
                float4 tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
				float4 posWorld : TEXCOORD6;
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
				o.pos.y = lerp(o.pos.y, -o.pos.y, _DualParaboloidParams.x < 0);
				o.pos.w = 1;

				//o.vertex.z = (L - _Near) / (_Far-_Near) * lerp(1, -1, o.vertex.z < _Bias);
				o.pos.z = (L - _DualParaboloidParams.y) * _DualParaboloidParams.w * lerp(1, -1, o.pos.z < -0.05);
				
				// Convert to Clip Space
				if(UNITY_NEAR_CLIP_VALUE  == 1) // for DirectX // for DirectX (near(1),far(0))
					o.pos.z = 1 - o.pos.z;
				else							// for OpenGL (near(-1),far(1))
					o.pos.z = o.pos.z - 1;

				o.normalDir = mul((float3x3)UNITY_MATRIX_M, v.normal);
                o.uv = v.uv * _baseColor_ST.xy +_baseColor_ST.zw;
				o.tangentDir = normalize( mul( UNITY_MATRIX_M, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
				float4 col = SAMPLE_TEXTURE2D(_baseColor, sampler_baseColor, i.uv);
				
				float3 tint = _Tint.rgb;

				#ifdef UNITY_COLORSPACE_GAMMA
				col.rgb = pow(abs(col.rgb), 2.2);
				tint = pow(abs(tint), 2.2);
				#endif

				col.rgb *= tint;

				float3 nor = SAMPLE_TEXTURE2D(_bumpMap, sampler_bumpMap, i.uv).xyz * 2 - 1;//UnpackNormal(SAMPLE_TEXTURE2D(_bumpMap, sampler_bumpMap, i.uv));
				float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
				float3 normalDirection = normalize(mul( nor, tangentTransform ));

				float3 lightDirection = _LightDirection.xyz;
				float3 viewDirection = normalize(_DualParaboloidCameraPos - i.posWorld.xyz);

				float NdotL = clamp(dot(normalDirection, lightDirection), 0, 1);				
				float NdotV = dot(normalDirection, viewDirection);
				
				float3 halfDirection = normalize(viewDirection+lightDirection);
				float LdotH = dot(lightDirection, halfDirection);
				float NdotH = dot(normalDirection, halfDirection);

				float alphaRoughness = _roughness * _roughness;

				AlbedoAndFresnelFromMetallic(col.rgb, _metallic, albedo, f0, f90)

				BRDF_Data brdf;
				brdf.albedo = albedo;
				brdf.F0 = f0;
				brdf.F90 = f90;
				brdf.NdotL = NdotL;
				brdf.NdotV = NdotV;
				brdf.LdotH = LdotH;
				brdf.NdotH = NdotH;
				brdf.alphaRoughness = alphaRoughness;

				float3 directLight = BRDF(brdf);

				float attenuation = ShadowAttenuation(i.posWorld.xyz);
				directLight *= attenuation;

				IBL_Data ibl;
				ibl.albedo = albedo;
				ibl.F0 = f0;
				ibl.normalDirection = normalDirection;
				ibl.reflectDirection = 2.0 * NdotV * normalDirection - viewDirection;
				ibl.NdotV = NdotV;
				ibl.alphaRoughness = alphaRoughness;
				ibl.metallic = _metallic;

				float3 indirectLight = IBL(ibl);

				col.rgb = directLight + indirectLight;
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
