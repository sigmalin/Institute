Shader "SG/SGDemo"
{
    Properties
    {
		_baseColor ("Base Color", COLOR) = (1,1,1,1)
		_roughness ("perceptual roughness", Range(0.089,1)) = 0.5 // avoid half-float(fp16) issue, 0.045 for single precision floats (fp32). 
		_metallic ("metallic", Range(0,1)) = 0

		[KeywordEnum(Punctual, Fitted, Standard)] _SGDiffuseModes("SG Diffuse Modes", Float) = 0
		[KeywordEnum(SGWarp, ASGWarp)] _SGSpecularModes("SG Specular Modes", Float) = 0
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
			#pragma multi_compile_fwdbase
			#pragma multi_compile _SGDIFFUSEMODES_PUNCTUAL _SGDIFFUSEMODES_FITTED _SGDIFFUSEMODES_STANDARD
			#pragma multi_compile _SGSPECULARMODES_SGWARP _SGSPECULARMODES_ASGWARP

            #include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "SphericalGaussDiffuse.cginc"
			#include "SphericalGaussSpecular.cginc"
			#include "AnisotropicSphericalGauss.cginc"
			#include "AnisotropicSphericalGaussSpecular.cginc"
			#include "ToneMapping.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float3 normalDir : TEXCOORD1;
				float4 posWorld : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

			float4 _baseColor;
			float _roughness;
			float _metallic;
			
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				o.normalDir = UnityObjectToWorldNormal(v.normal);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 lightDirection = _WorldSpaceLightPos0.xyz;
				float3 normalDirection = normalize(i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				
				float3 linearLightColor = _LightColor0.rgb;
				float3 linearBaseColor   = _baseColor.rgb;
				#ifdef UNITY_COLORSPACE_GAMMA
				linearLightColor = GammaToLinearSpace (linearLightColor);
				linearBaseColor = GammaToLinearSpace (linearBaseColor);
				#endif

				SG lightingLobe = DirectionalLightSG(lightDirection, 1, linearLightColor);
				
				float3 diffuseAlbedo = linearBaseColor  * (1.0 - 0.04) * (1.0 - _metallic);
				float3 specularAlbedo = lerp(0.04, linearBaseColor, _metallic);
				float roughness = _roughness * _roughness;
				
				#ifdef _SGDIFFUSEMODES_PUNCTUAL				
				float3 diffuse = SGDiffusePunctual(lightingLobe, normalDirection, diffuseAlbedo);
				#elif  _SGDIFFUSEMODES_FITTED
				float3 diffuse = SGDiffuseFitted(lightingLobe, normalDirection, diffuseAlbedo);
				#else
				float3 diffuse = SGDiffuseInnerProduct(lightingLobe, normalDirection, diffuseAlbedo);				
				#endif
				
				#ifdef _SGSPECULARMODES_SGWARP
				float3 specular = SGSpecular(lightingLobe, normalDirection, viewDirection, specularAlbedo, roughness);
				#else
				float3 specular = ASGSpecular(lightingLobe, normalDirection, viewDirection, specularAlbedo, roughness);
				#endif

				float3 col = diffuse + specular;
				col = ACES_tone_mapping(col);

				#ifdef UNITY_COLORSPACE_GAMMA
				col = LinearToGammaSpace (col);
				#endif
                
				return fixed4(col,1);
            }
            ENDCG
        }
    }
}
