Shader "MyBRDF/UnityStandard(roughness)"
{
    Properties
    {
        _baseColor ("Base Color", 2D) = "white" {}
		[NoScaleOffset] _bumpMap ("Bumpmap", 2D) = "bump" {}

		[NoScaleOffset] _MetallicMap ("metallic", 2D) = "black" {}
		[NoScaleOffset] _RoughnessMap ("roughness", 2D) = "black" {}
		[NoScaleOffset] _OcclusionMap ("Occlusion", 2D) = "white" {}

		[NoScaleOffset] _IrradianceD ("Diffuse Irradiance (LDR)", Cube) = "grey" {}
		[NoScaleOffset] _PrefiliterEnv ("Prefiliter Enviornment Map (LDR)", Cube) = "grey" {}
		[NoScaleOffset] [Linear] _IntegrateBRDF ("Integrate BRDF", 2D) = "grey" {}		
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
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight	
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "MyPBR.cginc"

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

				SHADOW_COORDS(8)
				UNITY_FOG_COORDS(9)
            };

            sampler2D _baseColor;
			float4 _baseColor_ST;

			sampler2D _bumpMap;
			sampler2D _MetallicMap;
			sampler2D _RoughnessMap;
			sampler2D _OcclusionMap;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _baseColor);
				o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);

                UNITY_TRANSFER_FOG(o,o.pos);
				TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col = tex2D(_baseColor, i.uv);

				float metallic = tex2D(_MetallicMap, i.uv).r;
				float roughness = tex2D(_RoughnessMap, i.uv).r;

				roughness = max(0.089, roughness); // avoid half-float(fp16) issue, 0.045 for single precision floats (fp32). 

				float occlusion = tex2D(_OcclusionMap, i.uv).g;
				
				#ifdef UNITY_COLORSPACE_GAMMA
				col.rgb = GammaToLinearSpace (col.rgb);
				#endif

				float3 nor = UnpackNormal(tex2D(_bumpMap,i.uv));

				float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
				float3 normalDirection = normalize(mul( nor, tangentTransform ));
				
				float3 lightDirection = _WorldSpaceLightPos0.xyz;
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

				float NdotL = clamp(dot(normalDirection, lightDirection), 0, 1);				
				float NdotV = clamp(dot(normalDirection, viewDirection), 0, 1);

				float3 halfDirection = normalize(viewDirection+lightDirection);
				float LdotH = dot(lightDirection, halfDirection);
				float NdotH = dot(normalDirection, halfDirection);

				float alphaRoughness = roughness * roughness;

				AlbedoAndFresnelFromMetallic(col.rgb, metallic, albedo, f0, f90)

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

				IBL_Data ibl;
				ibl.albedo = albedo;
				ibl.F0 = f0;
				ibl.normalDirection = normalDirection;
				ibl.reflectDirection = 2.0 * NdotV * normalDirection - viewDirection;
				ibl.NdotV = NdotV;
				ibl.alphaRoughness = alphaRoughness;
				ibl.metallic = metallic;

				float3 indirectLight = IBL(ibl);

				col.rgb = directLight + indirectLight;

				col.rgb *= occlusion;

				UNITY_LIGHT_ATTENUATION(atten, i, i.posWorld)
				atten = (atten + 1) * 0.5;
				col.rgb *= atten;

				col.rgb = ACES_tone_mapping(col.rgb);//Filmic_tone_mapping(col.rgb);

				#ifdef UNITY_COLORSPACE_GAMMA
				col.rgb = LinearToGammaSpace (col.rgb);
				#endif

				UNITY_APPLY_FOG(i.fogCoord, final);

                return col;
            }
            ENDCG
        }

		Pass 
		{
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

	Fallback "VertexLit"
}
