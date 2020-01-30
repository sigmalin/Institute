Shader "FourierOcean/FourierOcean (Projection)"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_Specular ("Specular", Color) = (1, 0.86, 0.57, 0)

		_Metallic ("Metallic", Range(0, 1)) = 1
        _Glossiness ("Smoothness",Range(0,1)) = 1

        _Displacement ("Displacement", 2D) = "black" {}
		_Normal ("Normal", 2D) = "bump" {}
		_Ratio ("Ratio", FLOAT) = 0.0078125

		_Foam ("Foam", 2D) = "white" {}
		_Noise ("Noise", 2D) = "gray" {}

		_Transparency ("Transparency", Range(0,1)) = 0.8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100

        Pass
        {
			Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }

			Blend SrcAlpha OneMinusSrcAlpha

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

			#include "../../../ProjectionOcean/Shaders/ProjectionGrid/ProjectionGrid.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
            };

            struct v2f
            {                
                float4 pos : SV_POSITION;

				float3 posWorld : TEXCOORD0;                
				float2 uv : TEXCOORD1;
				float2 uvFoam : TEXCOORD2;
				UNITY_FOG_COORDS(7)
            };

			float4 _Color;
			float4 _Specular;

			float _Metallic;
			float _Glossiness;

            sampler2D _Displacement;
            sampler2D _Normal;
			sampler2D _Foam;
			sampler2D _Noise;

			float4 _Foam_ST;

			float _Ratio;
			float _Transparency;

            v2f vert (appdata v)
            {
                v2f o;

				float2 uv = v.uv.xy;

				//Interpolate between frustums world space projection points. p is in world space.
				float4 p = PROJECTION_TO_WORLD(uv);

				float3 worldPos = p.xyz;

				o.uv = worldPos.xz*_Ratio;
				o.uvFoam = TRANSFORM_TEX(o.uv, _Foam);

				float4 D = tex2Dlod(_Displacement, float4(o.uv, 0.0, 0.0)).xzyw;
				p.xyz += D.xyz;

                o.pos = mul(UNITY_MATRIX_VP, p);
                o.posWorld = p;
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 nor = tex2D(_Normal,i.uv).rbg * 2 - 1;	
				float3 normalDirection = normalize(nor);

				float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));

				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float shiftAmount = dot(nor, viewDirection);
				normalDirection = shiftAmount < 0.0f ? normalDirection + viewDirection * (-shiftAmount + 1e-5f) : normalDirection;

				float NdotL = max(0, dot( normalDirection, lightDirection ));				
				float NdotV = max(0, dot( normalDirection, viewDirection ));

				float3 halfDirection = normalize(viewDirection+lightDirection);
				float LdotH = max(0, dot(lightDirection, halfDirection));
				float NdotH = max(0, dot( normalDirection, halfDirection ));
				float VdotH = max(0, dot( viewDirection, halfDirection ));

				float3 viewReflectDirection = normalize(reflect( -viewDirection, normalDirection ));

				float roughness = 1 - (_Glossiness * _Glossiness);
				roughness = roughness * roughness;


				float3 diffuse = _Color.rgb * (1 - _Metallic);
				float3 f0 = lerp(_Specular.rgb, _Color.rgb, _Metallic * 0.5);

				float3 F = f0 * FresnelSchlick(f0, NdotV);
				float3 D = f0 * GGX_Trowbridge_Reitz(NdotH, roughness);
				float G = SchlickGGX(NdotV, NdotL, roughness);

				diffuse = Burley(diffuse, roughness, NdotV, NdotL, VdotH);

				float3 specular = D * G * F / (4*LdotH*NdotH + 0.01);
				fixed4 result = fixed4(0,0,0,_Transparency);
				result.rgb = (diffuse + specular) * ((NdotL+1)*0.5);

				float2 noise = (tex2D(_Noise,i.uv + _Time.xx).rr - 0.5) * 2;
				float4 foam = tex2D(_Foam,i.uvFoam + noise*_Ratio*2).rrrr;

				float jacobian = tex2D(_Displacement,i.uv).a;				
				result += foam * jacobian;
											   
				UNITY_APPLY_FOG(i.fogCoord, result);

                return result;
            }
            ENDCG
        }
    }
}
