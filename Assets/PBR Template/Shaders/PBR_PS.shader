Shader "PBR Template/PBR_PS"
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
				float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
				float4 posWorld : TEXCOORD5;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _BumpTex;
			float4 _BumpTex_ST;

			float _Metal;
			float _Gloss;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv1 = TRANSFORM_TEX(v.uv, _BumpTex);
				o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float3 nor = UnpackNormal(tex2D(_BumpTex,i.uv1));				

				float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
				float3 normalDirection = normalize(mul( nor, tangentTransform ));

				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float NdotL = saturate(dot( normalDirection, lightDirection ));

				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
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
