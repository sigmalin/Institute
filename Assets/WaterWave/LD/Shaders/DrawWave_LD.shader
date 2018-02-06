Shader "Unlit/DrawWave2"
{
	Properties
	{
		_InputTex ("Input", 2D) = "white" {}
		_PrevTex ("Prev Result", 2D) = "white" {}
		_PrevPrevTex ("Prev Prev Result", 2D) = "white" {}
		_Stride ("Stride", Float) = 0.5
		_Parameter ("Parameter", float) = 0.1
		_Attenuation ("Attenuation", float) = 0.96
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100

		ZWrite Off
		ZTest Always
		//Cull Off
		//Blend SrcAlpha OneMinusSrcAlpha

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
				float4 vertex : SV_POSITION;
			};

			sampler2D _InputTex;
			sampler2D _PrevTex;
			sampler2D _PrevPrevTex;
			float4 _PrevTex_TexelSize;

			float _Stride;
			float _Parameter;
			float _Attenuation;

			half4 Remap(float4 _value)
			{
				return (_value*2)-1;
			}

			float4 InvRemap(float4 _value)
			{
				return (_value+1)*0.5;
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 stride = float2(_Stride, _Stride) * _PrevTex_TexelSize.xy;
				half4 prev  = Remap(tex2D(_PrevTex, i.uv));
				half4 prevL = Remap(tex2D(_PrevTex, half2(i.uv.x-stride.x,i.uv.y)));
				half4 prevR = Remap(tex2D(_PrevTex, half2(i.uv.x+stride.x,i.uv.y)));
				half4 prevT = Remap(tex2D(_PrevTex, half2(i.uv.x,i.uv.y-stride.y)));
				half4 prevB = Remap(tex2D(_PrevTex, half2(i.uv.x,i.uv.y+stride.y)));
				half4 prev2 = Remap(tex2D(_PrevPrevTex, i.uv));

				half valueR = prev.r*2 - prev2.r + (prev.g + prevL.g + prev.b + prevT.b - prev.r*4) * _Parameter;
				half valueG = prev.g*2 - prev2.g + (prevR.r + prev.r + prev.a + prevT.a - prev.g*4) * _Parameter;
				half valueB = prev.b*2 - prev2.b + (prev.a + prevL.a + prevB.r + prev.r - prev.b*4) * _Parameter;
				half valueA = prev.a*2 - prev2.a + (prevR.b + prev.b + prevB.g + prev.g - prev.a*4) * _Parameter;

				float4 value = float4(valueR,valueG,valueB,valueA);

				value -= tex2D(_InputTex, i.uv);
				value *= _Attenuation;
				value = InvRemap(value);

				return value;
			}
			ENDCG
		}
	}
}
