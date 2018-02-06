Shader "Rain/RainShader"
{
	Properties
	{
		//_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0,0,0,1)
		_Speed ("Speed", Float) = 6.0
		_Scale ("Scale", Float) = 4.0
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			Name "Rain"
			Tags { "LightMode" = "ForwardBase" }

			Blend SrcAlpha OneminusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			//sampler2D _MainTex;
			//float4 _MainTex_ST;

			fixed4 _Color;
			half _Speed;
			half _Scale;

			// return [0,1]
			float nhash11(float n)
			{
				return frac(sin(n)*43758.5453);
			}

			// conver [a,b] to [0,1]
			float remap(float t, float a, float b)
			{
				return clamp((t-a)/(b-a), 0, 1);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				fixed rnd = nhash11(fmod(v.vertex.z, 512.0));
				float timer = _Time.w * _Speed * remap(0.7, 1.0, rnd);
				o.vertex = v.vertex;
				o.vertex.y -= fmod(-v.vertex.y + timer, _Scale) + v.vertex.y - _Scale * 0.5;

				o.vertex = UnityObjectToClipPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}
