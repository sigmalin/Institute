Shader "RimDarkSide/Frag"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}	

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
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float3 view : TEXCOORD1;
				float3 light : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			half4 _RimColor;
			fixed _RimPower;
			fixed _RimRange;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.normal = UnityObjectToWorldNormal(v.normal);
				o.normal = v.normal;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.view = ObjSpaceViewDir(v.vertex);
				o.light = ObjSpaceLightDir(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				half NdotL = (dot(normalize(i.light), i.normal));
				half rim = 1.0h - saturate(dot(normalize(i.view), i.normal));
				half weight = pow(rim, _RimPower) * (1 - step(0, NdotL - _RimRange));

				col.rgb *= NdotL * 0.5 + 0.5;

				col.rgb += _RimColor.rgb * weight;//lerp(i.color, _RimColor.rgb, weight);		
				return col;
			}
			ENDCG
		}
	}
}
