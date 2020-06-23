Shader "SRP/Skybox/DualParaboloid"
{
    Properties
    {
		_Skymap ("Sky", 2D) = "black" {}
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
			#include "DualParaboloid.hlsl"
			#include "ToneMapping.hlsl"

			TEXTURE2D(_Skymap);
			SAMPLER(sampler_Skymap);

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
				float4 col = SAMPLE_DUALPARBOLOID_LOD (_Skymap, sampler_Skymap, i.texcoord, 0);								
				col.rgb = Inv_Reinhard_tone_mapping(col.rgb);

				#if defined(UNITY_COLORSPACE_GAMMA)
				col.rgb = pow(col.rgb,  0.454545);
				#endif
				
				return col;
            }
            ENDHLSL
        }
    }
}
