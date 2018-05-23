Shader "RimDarkSide/Vert"
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
				float4 rim : TEXCOORD1;				
				float2 uv : TEXCOORD0;
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
				
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float3 view = ObjSpaceViewDir(v.vertex);
				float3 light = ObjSpaceLightDir(v.vertex);

				half NdotL = (dot(normalize(light), v.normal));
				half rim = 1.0h - saturate(dot(normalize(view), v.normal));
				half weight = pow(rim, _RimPower) * (1 - step(0, NdotL - _RimRange));

				o.rim.xyz = _RimColor.rgb * weight;
				o.rim.w   = NdotL * 0.5 + 0.5;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				col.rgb *= i.rim.w;

				col.rgb += i.rim.xyz;
				return col;
			}
			ENDCG
		}
	}
}
