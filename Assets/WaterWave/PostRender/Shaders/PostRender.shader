Shader "WaterWave/PostRender"
{
	Properties
	{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
		[HideInInspector] _WaveTex ("Wave", 2D) = "gray" {}
		_VertexDisplacementScale("Displacement Scale", Float) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				float4 uv_Projection : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;

			sampler2D _WaveTex;
			float _VertexDisplacementScale;
			
			v2f vert (appdata v)
			{
				v2f o;

				v.vertex.y += WaveHeight(_WaveTex, v.uv) * _VertexDisplacementScale;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv_Projection = ComputeScreenPos (o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed2 uv = (i.uv_Projection/i.uv_Projection.w).xy;
				fixed4 col = tex2D(_MainTex, uv);
				return col;
			}
			ENDCG
		}
	}
}
