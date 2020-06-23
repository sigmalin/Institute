Shader "SRP/PostEffect/Bloom"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}

		_Filter ("Parameter Of Filiter", Vector) = (0,0,0,0)

		_Intensity ("Intensity", float) = 1
    }

	HLSLINCLUDE
		#include "MyUnlit.hlsl"

		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);

		CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_TexelSize;
			float4 _Filter;
			float _Intensity;
		CBUFFER_END

		real3 SampleTexture (float2 uv) 
		{
			return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
		}

		real3 SampleTexture (float2 uv, float offset) 
		{
			float4 o = _MainTex_TexelSize.xyxy * float2(-offset, offset).xxyy;
			real3 col = 
				SampleTexture(uv + o.xy) + SampleTexture(uv + o.zy) +
				SampleTexture(uv + o.xw) + SampleTexture(uv + o.zw);
			
			return col * 0.25;
		}

		real3 Prefilter (real3 c) 
		{
			real brightness = max(c.r, max(c.g, c.b));
			real soft = brightness - _Filter.y;
			soft = clamp(soft, 0, _Filter.z);
			soft = soft * soft * _Filter.w;
			real contribution = max(soft, brightness - _Filter.x);		
			contribution /= max(brightness, 0.00001);
			return c * contribution;
		}

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

		v2f vert (appdata v)
		{
			v2f o;
				
			float4 worldPos = mul(UNITY_MATRIX_M, v.vertex);
			o.vertex = mul(unity_MatrixVP, worldPos);
			o.uv = v.uv;
			return o;
		}
	ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass	// 0 - filiter
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#pragma target 3.5
			//#pragma prefer_hlslcc gles

			half4 frag (v2f i) : SV_Target
			{
				return half4(Prefilter(SampleTexture(i.uv, 1)), 1);
			}
            ENDHLSL
        }

		Pass	// 1 - down sample
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#pragma target 3.5
			//#pragma prefer_hlslcc gles

			half4 frag (v2f i) : SV_Target
			{
				return half4(SampleTexture(i.uv, 1), 1);
			}
            ENDHLSL
        }

		Pass	// 2 - up sample
        {
			Blend One One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#pragma target 3.5
			//#pragma prefer_hlslcc gles

			half4 frag (v2f i) : SV_Target
			{
				return half4(SampleTexture(i.uv, 0.5), 1);
			}
            ENDHLSL
        }

		Pass	// 3 - final
        {
			Blend One One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#pragma target 3.5
			//#pragma prefer_hlslcc gles

			half4 frag (v2f i) : SV_Target
			{
				return half4(SampleTexture(i.uv, 0.5) * _Intensity, 0);
			}
            ENDHLSL
        }
    }
}
