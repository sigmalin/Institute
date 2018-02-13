// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Shadow/ProjectionShadowMap" 
{
	Properties 
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader 
	{
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		BlendOp Max

		Tags { "RenderType"="Opaque" }
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
			    float4 pos : SV_POSITION;
			};
			v2f vert( appdata_img v ) {
			    v2f o;
			    o.pos = UnityObjectToClipPos(v.vertex);
			    return o;
			}
			float4 frag(v2f i) : SV_Target {
				return float4(1,0,0,1);
			}
			ENDCG
		}
	}

	SubShader 
	{
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		BlendOp Max

		Tags { "RenderType"="Transparent" }
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
			    float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;

			v2f vert( appdata_img v ) {
			    v2f o;
			    o.pos = UnityObjectToClipPos(v.vertex);
			    o.uv = float2(v.texcoord.x, v.texcoord.y);
			    return o;
			}

			float4 frag(v2f i) : SV_Target {
				float4 col = tex2D(_MainTex, i.uv);
				return float4(1,0,0,col.a);
			}
			ENDCG
		}
	}
	Fallback Off
}
