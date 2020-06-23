Shader "SRP/PostEffect/Tonemapping"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		_Exposure  ("Exposure", Range(0.1,5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#pragma target 3.5
			
            #include "MyUnlit.hlsl"
			#include "ToneMapping.hlsl"

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

			CBUFFER_START(UnityPerMaterial)
				half _Exposure;
			CBUFFER_END

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			float LinearRgbToLuminance(float3 linearRgb)
			{
				return dot(linearRgb, float3(0.2126729,  0.7151522, 0.0721750));
			}

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = mul(unity_MatrixVP, worldPos);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);				
				col.rgb = ACES_tone_mapping(col.rgb, _Exposure);
				
				col.a = LinearRgbToLuminance(col.rgb);
				
				#ifdef UNITY_COLORSPACE_GAMMA
				col.rgb = pow(col.rgb, 0.454545);
				#endif

				return col;
            }
            ENDHLSL
        }
    }
}
