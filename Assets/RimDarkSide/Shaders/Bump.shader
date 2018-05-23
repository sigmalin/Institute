Shader "RimDarkSide/Bump"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}	
		_BumpMap ("Bump map", 2D) = "bump" {}

		_RimColor ("Rim Color", Color) = (1,1,1,1)
		_RimPower ("Rim Powor", RANGE(0.1,10)) = 1
		_RimRange ("Rim Range", RANGE(-1,1)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Tags { "LightMode"="ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float3 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 view : TEXCOORD1;
				float3 light : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _BumpMap;
			float4 _BumpMap_ST;

			half4 _RimColor;
			fixed _RimPower;
			fixed _RimRange;

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
				
				float3 n = normalize(v.normal);
                float3 t = v.tangent;
                float3 b = cross(n, t);

				float3 view = ObjSpaceViewDir(v.vertex);
				float3 light = ObjSpaceLightDir(v.vertex);

				float4x4 invTangent = InvTangentMatrix(t, b, n);

				o.view = mul(view, invTangent);
				o.light = mul(light, invTangent);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				float3 normal = float4(UnpackNormal(tex2D(_BumpMap, i.uv)), 1);
				
				half NdotL = (dot(normalize(i.light), normal));
				half rim = 1.0h - saturate(dot(normalize(i.view), normal));
				half weight = pow(rim, _RimPower) * (1 - step(0, NdotL - _RimRange));

				col.rgb *= NdotL * 0.5 + 0.5;

				col.rgb += _RimColor.rgb * weight;
				return col;
			}
			ENDCG
		}
	}
}
