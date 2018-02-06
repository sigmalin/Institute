Shader "Unlit/DrawWater"
{
	Properties
	{
		[HideInInspector] _WaveTex ("Wave", 2D) = "gray" {}
		[HideInInspector] _RefTex  ("Ref", 2D) = "black" {}

		_BumpTex  ("Bump", 2D) = "bump" {}
		_BumpAmt  ("BumpAmt", Range(0,100)) = 0
		_VertexDisplacementScale("Displacement Scale", Float) = 0.5
		_NormalScaleFactor("Normal Scale Factor", Float) = 1
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "LightMode"="ForwardBase" }
		LOD 100

		ZWrite On
		Cull back
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Assets\WaterWave\Lib\Wave.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 ref : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};


			sampler2D _WaveTex;
			float4 _WaveTex_TexelSize;
			sampler2D _RefTex;
			float4 _RefTex_TexelSize;
			sampler2D _BumpTex;
			float4 _BumpTex_ST;
			float4x4 _RefW;
			float4x4 _RefVP;
			float _BumpAmt;
			float _VertexDisplacementScale;
			float _NormalScaleFactor;
			
			v2f vert (appdata v)
			{
				v2f o;

				v.vertex.y += WaveHeight(_WaveTex, v.uv) * _VertexDisplacementScale;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _BumpTex);
				o.ref = mul(_RefVP, mul(_RefW, v.vertex));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float2 bump = UnpackNormal(tex2D( _BumpTex, i.uv + _Time.x / 2 )).rg;
				bump += WaveNormal(_WaveTex, i.uv, _WaveTex_TexelSize.xy * _NormalScaleFactor);

				//float2 offset = bump * _BumpAmt - _BumpAmt * 0.5;
				float2 offset = bump * _BumpAmt - _WaveTex_TexelSize.xy;
				i.ref.xy = offset * i.ref.z + i.ref.xy;
				float4 ref = tex2D(_RefTex, i.ref.xy / i.ref.w * 0.5 + 0.5);

				float4 ret = ref;
				ret.a = 1;
				return ret;
			}
			ENDCG
		}
	}
}
