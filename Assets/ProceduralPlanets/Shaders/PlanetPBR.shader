Shader "ProceduralPlanets/PlanetPBR"
{
    Properties
    {
        _Gradient ("Gradient", 2D) = "white" {}
		_VecMinMax ("Parameters Of Height", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "AutoLight.cginc"
            #include "Lighting.cginc"
			#include "../../BRDF/Shaders/TorranceSparrow_NDF.cginc"
			#include "../../BRDF/Shaders/TorranceSparrow_GSF.cginc"
			#include "../../BRDF/Shaders/TorranceSparrow_SGSF.cginc"
			#include "../../BRDF/Shaders/Fresnel.cginc"
			#include "../../BRDF/Shaders/DiffuseTerm.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
				float4 posWorld : TEXCOORD5;
            };

            sampler2D _Gradient;
			float4 _VecMinMax;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.uv.x = (length(v.vertex.xyz) - _VecMinMax.x) / (_VecMinMax.z);
				o.uv.y = v.uv.x;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 normalDirection = normalize(i.normalDir);
				float3 lightDirection = _WorldSpaceLightPos0.xyz;

				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float shiftAmount = dot(i.normalDir, viewDirection);
				normalDirection = shiftAmount < 0.0f ? normalDirection + viewDirection * (-shiftAmount + 1e-5f) : normalDirection;

				float NdotL = clamp(dot( normalDirection, lightDirection ), 0, 1);				
				float NdotV = clamp(dot( normalDirection, viewDirection ), 0, 1);	

				float3 halfDirection = normalize(viewDirection+lightDirection);
				float LdotH = max(0, dot(lightDirection, halfDirection));
				float NdotH = max(0, dot( normalDirection, halfDirection ));
				float VdotH = max(0, dot( viewDirection, halfDirection ));
				
				fixed4 col = tex2Dlod(_Gradient, float4(i.uv, 0.0, 0.0));
				return col;
				float3 Ctint;
				float3 Cspec = UnityApproximation(col.rgb, 0.4, Ctint);

				float roughness = col.a;

				float3 F = Cspec * FresnelSchlick(Cspec, NdotV);
				float3 D = Cspec * GGX_Trowbridge_Reitz(NdotH, roughness);
				float G = SchlickGGX(NdotV, NdotL, roughness);

				float3 diffuse = Burley(Ctint, roughness, NdotV, NdotL, VdotH);
				float3 specular = D * G * F / (4*LdotH*NdotH + 0.01);

				float4 res = 1;
				
				res.rgb = diffuse + specular;
                return res;
            }
            ENDCG
        }
    }
}
