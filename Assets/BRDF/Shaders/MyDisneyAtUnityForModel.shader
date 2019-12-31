// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "BRDF/MyDisneyAtUnityForModel"
{
    Properties
    {
        _baseColor ("baseColor", 2D) = "white" {}

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
			#include "UnityGI.cginc"

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
				float2 uv : TEXCOORD0;
				float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
				float4 posWorld : TEXCOORD6;

				LIGHTING_COORDS(7,8)
				UNITY_FOG_COORDS(9)
            };

			sampler2D _baseColor;
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


            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
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
				
				fixed3 baseColor = tex2D(_baseColor, i.uv);
				
				#ifdef UNITY_COLORSPACE_GAMMA
				float3 col = GammaToLinearSpace (baseColor);
				#else
				float3 col = baseColor;
				#endif


				float3 Ctint = col / (col.r * 0.3 + col.g * 0.6 + col.b * 0.1);
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

				float3 outD = ((lerp(Fd, ss, _subsurface) * col.rgb) / UNITY_PI + CFsheen) * (1 - _metallic);

				// clearcoat
				float Dr = D_GTR(NdotH, lerp(0.1, 0.001, _clearcoatGloss));
				float Fr = lerp(0.04, 1, Fh);
				float Gr = G_GGX(NdotL, 0.25) * G_GGX(NdotV, 0.25);

				float outC = 0.25 * _clearcoat * Gr*Fr*Dr;

				float3 result = outS+outD+outC;

				result *= NdotL;

				// GI
				UNITY_LIGHT_ATTENUATION(atten,i,i.posWorld);
				atten = (1+atten) * 0.5;				

				UnityGI gi =  GetUnityGI(_LightColor0.rgb, lightDirection, normalDirection, viewDirection, viewReflectDirection, atten, _roughness, i.posWorld.xyz);
				
				#ifdef UNITY_COLORSPACE_GAMMA
				float3 indirectDiffuse = GammaToLinearSpace (gi.indirect.diffuse.rgb);
				float3 indirectSpecular = GammaToLinearSpace (gi.indirect.specular.rgb);
				float3 attenColor = atten * GammaToLinearSpace (_LightColor0.rgb);
				#else
				float3 indirectDiffuse = gi.indirect.diffuse.rgb ;
				float3 indirectSpecular = gi.indirect.specular.rgb;
				float3 attenColor = atten * _LightColor0.rgb;
				#endif
				
				result += (indirectDiffuse + indirectSpecular)* _metallic;
				result *= attenColor;

				#ifdef UNITY_COLORSPACE_GAMMA
				result = LinearToGammaSpace (result);
				#endif

				float4 final = fixed4(result, 1);

				UNITY_APPLY_FOG(i.fogCoord, final);

				return final;
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
