Shader "PBR Template/PBR_VS"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BumpTex ("Texture", 2D) = "bump" {}
		_Metal ("Metal", Range(0, 1)) = 1
        _Gloss ("Gloss", Range(0, 1)) = 0.2136752
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"

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
				float2 uv1 : TEXCOORD1;
				float3 localview : TEXCOORD2;
				float3 locallight : TEXCOORD3;
				float4 posWorld : TEXCOORD4;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _BumpTex;
			float4 _BumpTex_ST;

			float _Metal;
			float _Gloss;

			float4x4 InvTangentMatrix(float3 tan, float3 bin, float3 nor) 
            {
                float4x4 mat = float4x4(
                    float4(tan, 0),
                    float4(bin, 0),
                    float4(nor, 0),
                    float4(0, 0, 0, 1)
                );

                return transpose(mat);
            }
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv1 = TRANSFORM_TEX(v.uv, _BumpTex);
				
				float3 b = cross(v.normal, v.tangent);
                float3 view = ObjSpaceViewDir(v.vertex);
				float3 light = ObjSpaceLightDir(v.vertex);
                float4x4 invTangent = InvTangentMatrix(v.tangent, b, v.normal);

				o.localview = mul(view, invTangent);
				o.locallight = mul(light, invTangent);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float3 nor = UnpackNormal(tex2D(_BumpTex,i.uv1));				

				float3 normalDirection = nor;

				float3 lightDirection = normalize(i.locallight);
				float NdotL = saturate(dot( normalDirection, lightDirection ));

				float3 viewDirection = normalize(i.localview);
				float NdotV = abs(dot( normalDirection, viewDirection ));

				float3 halfDirection = normalize(viewDirection+lightDirection);
				float LdotH = saturate(dot(lightDirection, halfDirection));
				float NdotH = saturate(dot( normalDirection, halfDirection ));

				float perceptualRoughness = 1.0 - _Gloss;
                float roughness = perceptualRoughness * perceptualRoughness;

				float3 specularColor;
                float specularMonochrome;
                float3 diffuseColor = DiffuseAndSpecularFromMetallic( col, _Metal, specularColor, specularMonochrome );
				
				// diffuse
				half fd90 = 0.5 + 2 * LdotH * LdotH * perceptualRoughness;
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL);
                float3 diffuse = directDiffuse * diffuseColor;

				// specular
				float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
				float specularPBL = (visTerm*normTerm) * UNITY_PI;
				specularPBL = max(0, specularPBL * NdotL);
				float3 directSpecular = specularPBL*FresnelTerm(specularColor, LdotH);

				return fixed4(diffuse + directSpecular,1);
			}
			ENDCG
		}
	}
}
