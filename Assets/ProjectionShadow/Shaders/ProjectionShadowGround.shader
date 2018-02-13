Shader "Shadow/ProjectionShadowGround"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 uv_shadow : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _ProjectionShadowMap;
			float4x4  _ProjectionViewProj;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				float4 world = mul (unity_ObjectToWorld, v.vertex);
				float4 pos = mul (_ProjectionViewProj, world);
				o.uv_shadow = ComputeScreenPos (pos);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				
				fixed4 shadow = tex2D(_ProjectionShadowMap, (i.uv_shadow/i.uv_shadow.w).xy);
				
				return lerp(col, shadow, shadow.a);
			}
			ENDCG
		}
	}
}
