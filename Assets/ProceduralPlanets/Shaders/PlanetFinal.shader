Shader "ProceduralPlanets/PlanetFinal"
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
						
			#include "../../BRDF/Shaders/DisneyModel.cginc"
	

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
				o.uv = v.uv;
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 normalDirection = normalize(i.normalDir);
				float3 lightDirection = _WorldSpaceLightPos0.xyz;

				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float shiftAmount = dot(i.normalDir, viewDirection);
				normalDirection = shiftAmount < 0.0f ? normalDirection + viewDirection * (-shiftAmount + 1e-5f) : normalDirection;

				float3 viewReflectDirection = normalize(reflect( -viewDirection, normalDirection ));
				float NdotL = dot( normalDirection, lightDirection );				
				float NdotV = dot( normalDirection, viewDirection );

				NdotL = (NdotL + 1) * 0.5;
				NdotV = (NdotV + 1) * 0.5;

				float3 halfDirection = normalize(viewDirection+lightDirection);
				float LdotH = dot(lightDirection, halfDirection);
				float NdotH = dot(normalDirection, halfDirection);
				float VdotH = dot(viewDirection, halfDirection);

				float HdotX = dot( halfDirection, (i.tangentDir) );
				float HdotY = dot( halfDirection, (i.bitangentDir) );

				float LdotX = dot( lightDirection, (i.tangentDir) );
				float LdotY = dot( lightDirection, (i.bitangentDir) );

				float VdotX = dot( viewDirection, (i.tangentDir) );
				float VdotY = dot( viewDirection, (i.bitangentDir) );					
				
				float ocean = clamp((i.uv.y - _VecMinMax.x) * (-_VecMinMax.z), 0, 1);
				float ground = clamp((i.uv.y) * (_VecMinMax.w), 0, 1);
				float mask = floor(ocean);				
				float sx = lerp(0, 0.5, ocean) * (1-mask) + lerp(0.5, 1, ground) * mask;
				float4 col = tex2Dlod(_Gradient, float4(sx,i.uv.x,0,0));
				
				float roughness = col.a;

				float3 Ctint = col.rgb / (col.r * 0.3 + col.g * 0.6 + col.b * 0.1);
				float3 Cspec = Ctint;

				//
				float Gs = G_GGX(NdotL, roughness) * G_GGX(NdotV, roughness);
				float Fh = Schlick(LdotH);
				float3 Fs = Cspec;
				float Ds = D_GTR(NdotH, roughness);

				float3 outS = Gs*Fs*Ds;

				// clearcoat
				float Dr = D_GTR(NdotH, lerp(0.1, 0.001, 0));
				float Fr = lerp(0.04, 1, Fh);
				float Gr = G_GGX(NdotL, 0.25) * G_GGX(NdotV, 0.25);

				float outC = 0.25 * Gr*Fr*Dr;

                return fixed4(outS + outC,1);
            }
            ENDCG
        }
    }
}
