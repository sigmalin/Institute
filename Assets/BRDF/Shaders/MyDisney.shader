// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "BRDF/MyDisney"
{
    Properties
    {
        _baseColor ("baseColor", Color) = (1,1,1,1)

		_subsurface ("subsurface", Range(0,1)) = 0
		_metallic ("metallic", Range(0,1)) = 0
		_specular ("specular", Range(0,1)) = 0
		_specularTint ("specularTint", Range(0,1)) = 0
		_roughness ("roughness", Range(0,1)) = 0
		_anisotropic ("anisotropic", Range(0,1)) = 0
		_sheen ("sheen", Range(0,1)) = 0
		_sheenTint ("sheenTint", Range(0,1)) = 0
		_clearcoat ("clearcoat", Range(0,1)) = 0
		_clearcoatGloss ("clearcoatGloss", Range(0,1)) = 0
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
            #pragma multi_compile_fwdbase_fullshadows				
			#pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
			#include "DisneyModel.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
				float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
				float4 posWorld : TEXCOORD6;

				LIGHTING_COORDS(7,8)
				UNITY_FOG_COORDS(9)
            };

			fixed4 _baseColor;
			float _subsurface;
			float _metallic;
			float _specular;
			float _specularTint;
			float _roughness;
			float _anisotropic;
			float _sheen;
			float _sheenTint;
			float _clearcoat;
			float _clearcoatGloss;

			const float F0 = 0.04;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
				o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);

				UNITY_TRANSFER_FOG(o,o.pos);
				TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normalDirection = normalize(i.normalDir);

				float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));

				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float shiftAmount = dot(i.normalDir, viewDirection);
				normalDirection = shiftAmount < 0.0f ? normalDirection + viewDirection * (-shiftAmount + 1e-5f) : normalDirection;

				float3 viewReflectDirection = normalize(reflect( -viewDirection, normalDirection ));

				//float NdotL = max(0, dot( normalDirection, lightDirection ));				
				//float NdotV = max(0, dot( normalDirection, viewDirection ));
				float NdotL = dot( normalDirection, lightDirection );				
				float NdotV = dot( normalDirection, viewDirection );

				//if (NdotL < 0 || NdotV < 0) return fixed4(0,0,0,1);
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


				float3 Ctint = _baseColor.rgb / (_baseColor.r * 0.3 + _baseColor.g * 0.6 + _baseColor.b * 0.1);
				float3 Cspec = _specular * lerp(float3(1,1,1), Ctint, _specularTint);//_specular * 0.08 * lerp(float3(1,1,1), Ctint, _specularTint);

				//鏡面 BRDF 
				float Gs = G_GGX_Ansio(NdotL, _roughness, _anisotropic, LdotX, LdotY) * G_GGX_Ansio(NdotV, _roughness, _anisotropic, VdotX, VdotY);
				float Fh = Schlick(LdotH);
				float3 Fs = lerp(Cspec, float3(1,1,1), Fh);
				float Ds = D_GTR2_Ansio(NdotH, _roughness, _anisotropic, HdotX, HdotY);

				float3 outS = Gs*Fs*Ds;

				//拡散 BRDF 
				float3 Csheen = lerp(float3(1,1,1), Ctint, _sheenTint);				
				float Fi = Schlick(NdotL);
				float Fo = Schlick(NdotV);
				float Fd90 = 0.5 + 2 * LdotH * VdotH * _roughness;
				float Fd = lerp(1, Fd90, Fi) * lerp(1, Fd90, Fo);
				float Fs90 = LdotH * VdotH * _roughness;
				float Fss = lerp(1, Fs90, Fi) * lerp(1, Fs90, Fo);
				float ss = 1.25 * (Fss * ( 1/max(0.001,(NdotL+NdotV)) - 0.5) + 0.5);
				float3 CFsheen = Fh * Csheen * _sheen;

				float3 outD = ((lerp(Fd, ss, _subsurface) * _baseColor.rgb) / UNITY_PI + CFsheen) * (1 - _metallic);

				// clearcoat
				float Dr = D_GTR(NdotH, lerp(0.1, 0.001, _clearcoatGloss));
				float Fr = lerp(0.04, 1, Fh);
				float Gr = G_GGX(NdotL, 0.25) * G_GGX(NdotV, 0.25);

				float outC = 0.25 * _clearcoat * Gr*Fr*Dr;

				float3 result = outS+outD+outC;

				result *= NdotL;

				return fixed4(result, 1);
            }
            ENDCG
        }
    }
}
