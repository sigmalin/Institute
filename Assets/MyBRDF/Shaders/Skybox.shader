Shader "MyBRDF/Skybox"
{
    Properties
    {
        [Linear] _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
		_Rotation ("Rotation", Range(0, 360)) = 0
		[NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		Cull Off ZWrite Off

        Pass
        {
			Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_fwdbase_fullshadows		
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "MyPBR.cginc"
			
            samplerCUBE _Tex;
			half4 _Tex_HDR;
			half4 _Tint;
			float _Rotation;

			float3 RotateAroundYInDegrees (float3 vertex, float degrees)
			{
				float alpha = degrees * UNITY_PI / 180.0;
				float sina, cosa;
				sincos(alpha, sina, cosa);
				float2x2 m = float2x2(cosa, -sina, sina, cosa);
				return float3(mul(m, vertex.xz), vertex.y).xzy;
			}

			struct appdata_t {
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

            v2f vert (appdata_t v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
				o.vertex = UnityObjectToClipPos(rotated);
				o.texcoord = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float4 tex = texCUBE (_Tex, i.texcoord);
				//float3 col = DecodeHDR (tex, _Tex_HDR);				
				//col = col * _Tint.rgb * unity_ColorSpaceDouble.rgb;
				float3 col = Inv_Reinhard_tone_mapping (tex.rgb * _Tint.rgb);
				
				col = ACES_tone_mapping(col);

				#ifdef UNITY_COLORSPACE_GAMMA
				col = LinearToGammaSpace (col);
				#endif

                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
