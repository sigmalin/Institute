Shader "ProjectionOcean/OceanShader"
{
	Properties
	{
		_WaterNormal1 ("Water Normal 1", 2D) = "bump" {}
		_WaterNormal2 ("Water Normal 2", 2D) = "bump" {}

		_WaterFlow ("Water Flow", Color) = (1,0,0,1)
		
		_WaterSpeed1 ("Water Speed 1", FLOAT) = 1
		_WaterSpeed2 ("Water Speed 2", FLOAT) = 1

		///

		_WaterColor ("Water Color", Color) = (0,0.1328125,0.2265625,1)
		_DiffuseColor ("Diffuse Color", Color) = (1,1,1,1)
		_SpecularColor ("Specular Color", Color) = (1,1,1,1)
		_Gloss ("Gloss", Range(0,1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
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
			#include "ProjectionGrid\\ProjectionGrid.cginc"

			sampler2D _WaterNormal1;
			sampler2D _WaterNormal2;
			float4 _WaterNormal1_ST;
			float4 _WaterNormal2_ST;
			float4 _WaterFlow;
			float _WaterSpeed1;
			float _WaterSpeed2;

			float4 _WaterColor;
			float4 _DiffuseColor;
			float4 _SpecularColor;
			float  _Gloss;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv1 : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 worldPos : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata v)
			{
				v2f o;

				o.worldPos = PROJECTION_TO_WORLD(v.uv);
				o.vertex = mul(UNITY_MATRIX_VP, o.worldPos);
				o.uv1 = TRANSFORM_TEX(v.uv, _WaterNormal1);
				o.uv2 = TRANSFORM_TEX(v.uv, _WaterNormal2);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 offset1 = float2(0,_Time.x * _WaterSpeed1);//float2(_Time.x * _WaterSpeed,0);
				float2 offset2 = float2(0,_Time.x * _WaterSpeed2);
				float2 flowdir = normalize((_WaterFlow.gr - 0.5) * 2);
				float2x2 rotmat = float2x2(flowdir.x, -flowdir.y, flowdir.y ,flowdir.x);

				float3 n1 = (tex2D(_WaterNormal1, mul(rotmat, i.uv1) - offset1).rbg * 2) - 1;
				float3 n2 = (tex2D(_WaterNormal2, mul(rotmat, i.uv2) - offset2).rbg * 2) - 1;
				float3 n = normalize(n1+n2);

				//n = (n * 0.5) + 0.5;
				//return fixed4(n.rbg,1);

				/// BPR
				fixed3 normalDirection = n;	
				fixed3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				fixed NdotL = saturate(dot( normalDirection, lightDirection ));

				fixed3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				fixed NdotV = abs(dot( normalDirection, viewDirection ));

				fixed3 halfDirection = normalize(viewDirection+lightDirection);
				fixed LdotH = saturate(dot(lightDirection, halfDirection));
				fixed NdotH = saturate(dot( normalDirection, halfDirection ));

				fixed gloss = _Gloss;
				fixed perceptualRoughness = 1.0 - gloss;
                fixed roughness = perceptualRoughness * perceptualRoughness;

				// diffuse
				half fd90 = 0.5 + 2 * LdotH * LdotH * perceptualRoughness;
                fixed nlPow5 = Pow5(1-NdotL);
                fixed nvPow5 = Pow5(1-NdotV);
                fixed3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL);
                fixed3 diffuse = directDiffuse * _DiffuseColor * 2;

				// specular
				/// https://community.arm.com/graphics/b/blog/posts/moving-mobile-graphics	
				float roughness4 = roughness*roughness;
				float rTerm = (NdotH*NdotH)*(roughness4-1)+1;
				float lTerm = (LdotH*LdotH)*(roughness+0.5);
				float dTerm = 4*UNITY_PI*rTerm*rTerm*lTerm;
				float3 directSpecular = (roughness4 / dTerm)*_SpecularColor;	

				return float4(_WaterColor.rgb + diffuse + directSpecular, 1);
			}
			ENDCG
		}
	}
}
