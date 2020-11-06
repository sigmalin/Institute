Shader "Unlit/Ashikhmin"
{
    Properties
    {
        _diffuseColor ("Diffuse Color", COLOR) = (1,1,1,1)
		_specularColor ("Specular Color", COLOR) = (1,1,1,1)
		_lambda ("Lambda", Range(1,400)) = 1
		_mu ("Mu", Range(1,400)) = 1
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

			#include "UnityLightingCommon.cginc"
			#include "AutoLight.cginc"
            #include "UnityCG.cginc"

			#include "SphericalGaussDiffuse.cginc"
			#include "Ashikhmin.cginc"
			#include "ToneMapping.cginc"

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
            };
			
			float4 _diffuseColor;
            float4 _specularColor;
			float _lambda;
			float _mu;
			float _metallic;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 lightDirection = _WorldSpaceLightPos0.xyz;
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

				float3 normalDirection = normalize(i.normalDir);
				float3 tangentDirection = normalize(i.tangentDir);
				float3 bitangentDirection = normalize(i.bitangentDir);

				///
				float3 linearLightColor = _LightColor0.rgb;
				float3 diffuseAlbedo   = _diffuseColor.rgb;
				float3 specularAlbedo   = _specularColor.rgb;
				#ifdef UNITY_COLORSPACE_GAMMA
				linearLightColor = GammaToLinearSpace (linearLightColor);
				diffuseAlbedo = GammaToLinearSpace (diffuseAlbedo);
				specularAlbedo = GammaToLinearSpace (specularAlbedo);
				#endif

				SG lightingLobe = DirectionalLightSG(lightDirection, 1, linearLightColor);

				float3 diffuse = SGDiffuseInnerProduct(lightingLobe, normalDirection, diffuseAlbedo);

				float3 specular = AshikhminSpecular(
									lightingLobe,
									normalDirection, 
									tangentDirection, 
									bitangentDirection, 
									viewDirection,
									specularAlbedo, _lambda, _mu);

				float3 res = ACES_tone_mapping(diffuse + specular);
				
				#ifdef UNITY_COLORSPACE_GAMMA
				res = LinearToGammaSpace (res);
				#endif
				return float4(res, 1);
            }
            ENDCG
        }
    }
}
