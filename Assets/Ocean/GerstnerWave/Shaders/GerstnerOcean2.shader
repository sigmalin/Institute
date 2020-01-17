Shader "Ocean/GerstnerWave/Ocean2"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_Specular ("Specular", Color) = (1, 0.86, 0.57, 0)

		_Metallic ("Metallic", Range(0, 1)) = 1
        _Glossiness ("Smoothness",Range(0,1)) = 1

        _Amplitude ("Amplitude", Vector) = (0.8, 0.8, 0.4, 0.9)
		_Frequency ("Frequency", Vector) = (0.4, 1.8, 1.0, 1.2)
		_Steepness ("Steepness", Vector) = (0.2, 0.3, 0.7, 0.4)
		_Speed ("Speed", Vector) = (20, 30, 10, 30)
		_DirectionA ("Wave A(X,Y) and B(Z,W)", Vector) = (0.47, 0.35, -0.96, 0.23)
		_DirectionB ("C(X,Y) and D(Z,W)", Vector) = (0.77, -1.47, -0.3, -0.2)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
			#pragma multi_compile_fwdbase_fullshadows	

            #include "UnityCG.cginc"
			#include "AutoLight.cginc"
            #include "Lighting.cginc"

			#include "../../../BRDF/Shaders/TorranceSparrow_NDF.cginc"
			#include "../../../BRDF/Shaders/TorranceSparrow_GSF.cginc"
			#include "../../../BRDF/Shaders/TorranceSparrow_SGSF.cginc"
			#include "../../../BRDF/Shaders/Fresnel.cginc"
			#include "../../../BRDF/Shaders/DiffuseTerm.cginc"
			#include "Gerstner.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                
                float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float2 planePos : TEXCOORD1;
				UNITY_FOG_COORDS(2)		
				LIGHTING_COORDS(3,4)
            };

			float4 _Color;
			float4 _Specular;

			float _Metallic;
			float _Glossiness;

            float4 _Amplitude;
			float4 _Frequency;
			float4 _Steepness;
			float4 _Speed;
			float4 _DirectionA;
			float4 _DirectionB;

            v2f vert (appdata v)
            {
                v2f o;
                
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.planePos = worldPos.xz;
                
				float3 pos = float3(0.0, 0.0, 0.0);				

				float time = _Time.x;				

				pos += GerstnerWave(_Amplitude.x, _Frequency.x, _Steepness.x, _Speed.x, _DirectionA.xy, worldPos.xz, time);
				pos += GerstnerWave(_Amplitude.y, _Frequency.y, _Steepness.y, _Speed.y, _DirectionA.zw, worldPos.xz, time);
				pos += GerstnerWave(_Amplitude.z, _Frequency.z, _Steepness.z, _Speed.z, _DirectionB.xy, worldPos.xz, time);
				pos += GerstnerWave(_Amplitude.w, _Frequency.w, _Steepness.w, _Speed.w, _DirectionB.zw, worldPos.xz, time);

				o.worldPos = worldPos.xyz + pos;
				o.pos = mul(UNITY_MATRIX_VP, float4(o.worldPos, 1.0));
                UNITY_TRANSFER_FOG(o,o.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 binormal = float3(1.0, 0.0, 0.0);
				float3 tangent = float3(0.0, 0.0, 1.0); 

				float time = _Time.x;

				binormal += CalcBinormal(_Amplitude.x, _Frequency.x, _Steepness.x, _Speed.x, _DirectionA.xy, i.planePos, time);
				binormal += CalcBinormal(_Amplitude.y, _Frequency.y, _Steepness.y, _Speed.y, _DirectionA.zw, i.planePos, time);
				binormal += CalcBinormal(_Amplitude.z, _Frequency.z, _Steepness.z, _Speed.z, _DirectionB.xy, i.planePos, time);
				binormal += CalcBinormal(_Amplitude.w, _Frequency.w, _Steepness.w, _Speed.w, _DirectionB.zw, i.planePos, time);

				tangent += CalcTangent(_Amplitude.x, _Frequency.x, _Steepness.x, _Speed.x, _DirectionA.xy, i.planePos, time);
				tangent += CalcTangent(_Amplitude.y, _Frequency.y, _Steepness.y, _Speed.y, _DirectionA.zw, i.planePos, time);
				tangent += CalcTangent(_Amplitude.z, _Frequency.z, _Steepness.z, _Speed.z, _DirectionB.xy, i.planePos, time);
				tangent += CalcTangent(_Amplitude.w, _Frequency.w, _Steepness.w, _Speed.w, _DirectionB.zw, i.planePos, time);
				
				float3 normalDirection = normalize(cross(tangent, binormal));
				float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.worldPos.xyz,_WorldSpaceLightPos0.w));
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				float shiftAmount = dot(normalDirection, viewDirection);
				normalDirection = shiftAmount < 0.0f ? normalDirection + viewDirection * (-shiftAmount + 1e-5f) : normalDirection;

				float NdotL = max(0, dot( normalDirection, lightDirection ));				
				float NdotV = max(0, dot( normalDirection, viewDirection ));

				float3 halfDirection = normalize(viewDirection+lightDirection);
				float LdotH = max(0, dot(lightDirection, halfDirection));
				float NdotH = max(0, dot( normalDirection, halfDirection ));
				float VdotH = max(0, dot( viewDirection, halfDirection ));

				float roughness = 1 - (_Glossiness * _Glossiness);
				roughness = roughness * roughness;

				float3 diffuse = _Color.rgb * (1 - _Metallic);
				float3 f0 = lerp(_Specular.rgb, _Color.rgb, _Metallic * 0.5);

				float3 F = f0 * FresnelSchlick(f0, NdotV);
				float3 D = f0 * GGX_Trowbridge_Reitz(NdotH, roughness);
				float G = SchlickGGX(NdotV, NdotL, roughness);

				diffuse = Burley(diffuse, roughness, NdotV, NdotL, VdotH);

				float3 specular = D * G * F / (4*LdotH*NdotH + 0.01);
				fixed4 result = fixed4(0,0,0,1);
				result.rgb = (diffuse + specular) * NdotL;

				//UNITY_LIGHT_ATTENUATION(atten,i,i.worldPos);
				//result.rgb *= atten * _LightColor0.rgb;

				UNITY_APPLY_FOG(i.fogCoord, result);

                return result;
            }
            ENDCG
        }

		Pass {
             Name "ShadowCaster"
             Tags { "LightMode" = "ShadowCaster" }
 
             Fog {Mode Off}
             ZWrite On ZTest Less Cull Off
             Offset 1, 1
 
             CGPROGRAM
 
             #pragma vertex vert
             #pragma fragment frag
             #pragma fragmentoption ARB_precision_hint_fastest
             #pragma multi_compile_shadowcaster
             #include "UnityCG.cginc"
             
             struct v2f
             {
                 V2F_SHADOW_CASTER; 
             };
 
 
             v2f vert(appdata_full v )
             {
                v2f o;
                TRANSFER_SHADOW_CASTER(o)
                  
               return o;
             }
 
             float4 frag( v2f i ) : COLOR
             {
                 SHADOW_CASTER_FRAGMENT(i)
             }
             ENDCG
        }
    }
}
