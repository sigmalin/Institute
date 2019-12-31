Shader "BRDF/MyPBR"
{
    Properties
    {
        _Albedo ("Albedo (Metallic Workflow)", Color) = (1,1,1,1)

		_Color ("Color (Specular Workflow)", Color) = (1,1,1,1)
		_Specular ("Specular (Specular Workflow)", Color) = (1, 0.86, 0.57, 0)

		_Metallic ("Metallic", Range(0, 1)) = 1
        _Glossiness ("Smoothness",Range(0,1)) = 1
		
		_Anisotropic ("Anisotropic",  Range(-20,1)) = 0
		_IOR ("Ior",  Range(1,4)) = 1.5

		[KeywordEnum(Metallic, Spec)] _Workflow ("Work Flow", Float) = 0

		[KeywordEnum(GGX_Trowbridge_Reitz, GGX, Blinn_Phong, Phong, Beckmann, Gaussian, Trowbridge_Reitz_Anisotropic, Ward_Anisotropic)] _NDF ("Normal Distribution Function", Float) = 0
		[KeywordEnum(NormalBase, SmithBase, Anisotropic)] _GTYPE ("Geometric Shadowing Function Type", Float) = 0
		[KeywordEnum(Implicit, AshikhminPremoze, Kelemen, ModifiedKelemen, CookTorrence)] _GSF ("Normal Base Geometric Shadowing Function", Float) = 0
		[KeywordEnum(Walter, SmithBeckman, GGX, Schlick, SchlickBeckman, SchlickGGX)] _SGSF ("Smith Base Geometric Shadowing Function", Float) = 0
		[KeywordEnum(AshikhminShirley, Duer, Neumann, Ward, Kurt)] _AGSF ("Suited For Anisotropic Normal Distribution", Float) = 0
		
		[KeywordEnum(FresnelSchlick, IOR, SphericalGaussian, UnrealFresnel)] _FTYPE ("Fresnel Function", Float) = 0
		[KeywordEnum(Lambert, Burley, OrenNayar, Gotanda)] _DIFUSE_TERM ("Difusee Term", Float) = 0
		[KeywordEnum(DiffuseOnly, SpecularOnly, IndirectOnly, WithoutIndirect, All)] _RESULT ("Result", Float) = 0
		
		[Toggle] _APPLY_VIS_TERM("Apply Vis-Term", Float) = 0
		[Toggle] _APPLY_D_TERM("Apply D-Term", Float) = 0
		[Toggle] _APPLY_G_TERM("Apply G-Term", Float) = 0
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
			#pragma target 3.0

			
			#pragma multi_compile_fwdbase_fullshadows				
			#pragma multi_compile_fog
			#pragma multi_compile _WORKFLOW_METALLIC _WORKFLOW_SPEC
			#pragma multi_compile _NDF_GGX_TROWBRIDGE_REITZ _NDF_GGX _NDF_BLINN_PHONG _NDF_PHONG _NDF_BECKMANN _NDF_GAUSSIAN _NDF_TROWBRIDGE_REITZ_ANISOTROPIC _NDF_WARD_ANISOTROPIC
			#pragma multi_compile _GTYPE_NORMALBASE _GTYPE_SMITHBASE _GTYPE_ANISOTROPIC
			#pragma multi_compile _GSF_IMPLICIT _GSF_ASHIKHMINPREMOZE  _GSF_KELEMEN _GSF_MODIFIEDKELEMEN _GSF_COOKTORRENCE 
			#pragma multi_compile _SGSF_WALTER _SGSF_SMITHBECKMAN _SGSF_GGX _SGSF_SCHLICK _SGSF_SCHLICKBECKMAN _SGSF_SCHLICKGGX
			#pragma multi_compile _AGSF_ASHIKHMINSHIRLEY _AGSF_DUER _AGSF_NEUMANN _AGSF_WARD _AGSF_KURT			
			#pragma multi_compile _FTYPE_FRESNELSCHLICK _FTYPE_IOR _FTYPE_SPHERICALGAUSSIAN _FTYPE_UNREALFRESNEL
			#pragma multi_compile _DIFUSE_TERM_LAMBERT _DIFUSE_TERM_BURLEY _DIFUSE_TERM_ORENNAYAR _DIFUSE_TERM_GOTANDA
			#pragma multi_compile _RESULT_DIFFUSEONLY _RESULT_SPECULARONLY _RESULT_INDIRECTONLY _RESULT_WITHOUTINDIRECT _RESULT_ALL

			#pragma shader_feature _APPLY_VIS_TERM_ON
			#pragma shader_feature _APPLY_D_TERM_ON
			#pragma shader_feature _APPLY_G_TERM_ON

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

			#include "TorranceSparrow_NDF.cginc"
			#include "TorranceSparrow_GSF.cginc"
			#include "TorranceSparrow_SGSF.cginc"
			#include "Fresnel.cginc"
			#include "DiffuseTerm.cginc"
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
				float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
				float4 posWorld : TEXCOORD6;

				LIGHTING_COORDS(7,8)
				UNITY_FOG_COORDS(9)
            };
			
            fixed4 _Albedo;
			fixed4 _Color;
			fixed4 _Specular;

			float _Metallic;
			float _Glossiness;
			
			float _Anisotropic;
			float _IOR;
			
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
				//float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);

				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float shiftAmount = dot(i.normalDir, viewDirection);
				normalDirection = shiftAmount < 0.0f ? normalDirection + viewDirection * (-shiftAmount + 1e-5f) : normalDirection;

				float3 viewReflectDirection = normalize(reflect( -viewDirection, normalDirection ));


				float NdotL = max(0, dot( normalDirection, lightDirection ));

				
				float NdotV = max(0, dot( normalDirection, viewDirection ));

				float3 halfDirection = normalize(viewDirection+lightDirection);
				float LdotH = max(0, dot(lightDirection, halfDirection));
				float NdotH = max(0, dot( normalDirection, halfDirection ));
				float VdotH = max(0, dot( viewDirection, halfDirection ));

				float HdotX = dot( halfDirection, (i.tangentDir) );//dot( halfDirection, normalize(i.tangentDir) );
				float HdotY = dot( halfDirection, (i.bitangentDir) );//dot( halfDirection, normalize(i.bitangentDir) );

				float roughness = 1 - (_Glossiness * _Glossiness);
				roughness = roughness * roughness;


				// Diffuse
			#ifdef _WORKFLOW_METALLIC
				float3 diffuse;
				float3 f0 = UnityApproximation(_Albedo.rgb, _Metallic, diffuse);
			#else // _WORKFLOW_SPEC
				float3 diffuse = _Color.rgb * (1 - _Metallic);
				float3 f0 = lerp(_Specular.rgb, _Color.rgb, _Metallic * 0.5);
			#endif

				// KD
				//float3 KD = saturate(1 - F) * (1 - _Metallic);
				float KD = NormalIncidenceReflection(NdotV, NdotL, LdotH, roughness);
				diffuse *= KD;


				// F-term			
			
			#ifdef _FTYPE_FRESNELSCHLICK
				float3 F = f0 * FresnelSchlick(f0, NdotV);
			#elif _FTYPE_IOR
				float3 F = f0 * FresnelIOR(_IOR, NdotV);
			#elif _FTYPE_SPHERICALGAUSSIAN
				float3 F = f0 * SphericalGaussian(f0, NdotV);
			#else // _FTYPE_UNREALFRESNEL
				float3 F = f0 * UnrealFresnel(f0, VdotH);
			#endif

				// GI
				UNITY_LIGHT_ATTENUATION(atten,i,i.posWorld);
				float3 attenColor = atten * _LightColor0.rgb;

				UnityGI gi =  GetUnityGI(_LightColor0.rgb, lightDirection, normalDirection, viewDirection, viewReflectDirection, atten, 1- _Glossiness, i.posWorld.xyz);
				float3 indirectDiffuse = gi.indirect.diffuse.rgb ;
				float3 indirectSpecular = gi.indirect.specular.rgb;
				
				float grazingTerm = saturate(roughness + _Metallic);
				indirectSpecular *=  FresnelLerp(f0,grazingTerm,NdotV) * max(0.15,_Metallic) * (1-roughness*roughness* roughness);

				// D-term
			#ifdef _NDF_GGX_TROWBRIDGE_REITZ
				float3 D = f0 * GGX_Trowbridge_Reitz(NdotH, roughness);
			#elif _NDF_GGX
				float3 D = f0 * GGX(NdotH, roughness);
			#elif _NDF_BLINN_PHONG
				float3 D = f0 * Blinn_Phong(NdotH, roughness);
			#elif _NDF_PHONG
				float3 D = f0 * Phong(NdotH, _Glossiness, max(1, _Glossiness * 40));
			#elif _NDF_BECKMANN
				float3 D = f0 * Beckmann(NdotH, roughness);
			#elif _NDF_GAUSSIAN
				float3 D = f0 * Gaussian(NdotH, roughness);
			#elif _NDF_TROWBRIDGE_REITZ_ANISOTROPIC
				float3 D = f0 * Trowbridge_Reitz_Anisotropic(NdotH, _Glossiness, _Anisotropic, HdotX, HdotY);
			#elif _NDF_WARD_ANISOTROPIC
				float3 D = f0 * Ward_Anisotropic(NdotH, NdotV, NdotL, _Glossiness, _Anisotropic, HdotX, HdotY);
			#endif

				// G-term
		#ifdef _GTYPE_NORMALBASE
			
			#ifdef _GSF_IMPLICIT
				float G = Implicit(NdotV, NdotL);			
			#elif _GSF_ASHIKHMINPREMOZE
				float G = AshikhminPremoze(NdotV, NdotL);			
			#elif _GSF_KELEMEN
				float G = Kelemen(NdotV, NdotL, LdotH, VdotH);
			#elif _GSF_MODIFIEDKELEMEN
				float G = ModifiedKelemen(NdotV, NdotL, roughness);
			#elif _GSF_COOKTORRENCE
				float G = CookTorrence(NdotV, NdotL, VdotH, NdotH);
			#endif

		#elif _GTYPE_SMITHBASE

			#ifdef _SGSF_WALTER
				float G = Walter(NdotV, NdotL, roughness);
			#elif _SGSF_SMITHBECKMAN
				float G = SmithBeckman(NdotV, NdotL, roughness);
			#elif _SGSF_GGX
				float G = GeometryGGX(NdotV, NdotL, roughness);
			#elif _SGSF_SCHLICK
				float G = Schlick(NdotV, NdotL, roughness);
			#elif _SGSF_SCHLICKBECKMAN
				float G = SchlickBeckman(NdotV, NdotL, roughness);
			#elif _SGSF_SCHLICKGGX
				float G = SchlickGGX(NdotV, NdotL, roughness);
			#endif

		#else // _GTYPE_ANISOTROPIC

			#if _AGSF_ASHIKHMINSHIRLEY
				float G = AshikhminShirley(NdotV, NdotL, LdotH);
			#elif _AGSF_DUER
				float G = Duer(lightDirection, viewDirection, normalDirection, NdotV, NdotL);
			#elif _AGSF_NEUMANN
				float G = Neumann(NdotV, NdotL);
			#elif _AGSF_WARD
				float G = Ward(NdotV, NdotL);
			#elif _AGSF_KURT
				float G = Kurt(NdotV, NdotL, VdotH, roughness);
			#endif

		#endif


			#ifdef _DIFUSE_TERM_LAMBERT
				diffuse = Lambert(diffuse);
			#elif _DIFUSE_TERM_BURLEY
				diffuse = Burley(diffuse, roughness, NdotV, NdotL, VdotH);
			#elif _DIFUSE_TERM_ORENNAYAR
				diffuse = OrenNayar(diffuse, roughness, NdotV, NdotL, VdotH);
			#else // _DIFUSE_TERM_GOTANDA
				diffuse = Gotanda(diffuse, roughness, NdotV, NdotL, VdotH);
			#endif
			
				float3 specular = fixed3(1,1,1);
				

			#ifdef _APPLY_VIS_TERM_ON
				specular *= F / (4*LdotH*NdotH + 0.01); //(4*LdotH*NdotH + 0.000001);
			#endif

			#ifdef _APPLY_D_TERM_ON
				specular *= D;
			#endif

			#ifdef _APPLY_G_TERM_ON
				specular *= G;
			#endif


			fixed4 result = fixed4(0,0,0,1);

			#ifdef _RESULT_DIFFUSEONLY
				result.rgb = diffuse;
			#elif _RESULT_SPECULARONLY
				result.rgb = specular;
			#elif _RESULT_INDIRECTONLY
				result.rgb = indirectDiffuse + indirectSpecular;
				result.rgb *= NdotL;
				result.rgb *= attenColor;
			#elif _RESULT_WITHOUTINDIRECT
				result.rgb = diffuse + specular;
				result.rgb *= NdotL;
				result.rgb *= attenColor;
			#else
				result.rgb = diffuse + specular + indirectDiffuse + indirectSpecular;
				result.rgb *= NdotL;
				result.rgb *= attenColor;
			#endif				

				UNITY_APPLY_FOG(i.fogCoord, result);

				return saturate(result);
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
