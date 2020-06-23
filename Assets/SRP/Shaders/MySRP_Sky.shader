Shader "SRP/Skybox/Cubemap"
{
    Properties
    {
		_Cubemap ("Cube", Cube) = "black" {}
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#pragma target 3.5
			
            #include "MyUnlit.hlsl"

			TEXTURECUBE(_Cubemap);
			SAMPLER(sampler_Cubemap);

			struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
            };


            v2f vert (appdata v)
            {
                v2f o;
				
				float4 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = mul(unity_MatrixVP, worldPos);

				o.texcoord = v.vertex.xyz;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
				float4 col = SAMPLE_TEXTURECUBE (_Cubemap, sampler_Cubemap, i.texcoord);
				
				#ifdef UNITY_COLORSPACE_GAMMA
				col.rgb = pow(abs(col.rgb) * 2, 2.2);
				#else
				col.rgb *= pow(2.0, 2.2);
				#endif
				
				return col;
            }
            ENDHLSL
        }
    }
}
