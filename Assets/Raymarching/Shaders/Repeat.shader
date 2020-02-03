Shader "Raymarching/Repeat"
{
    Properties
    {

    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
			Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

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
			
			float3 mod(float3 a, float3 b)   
			{   
				return a - b*floor(a / b);
			}

			float3 trans(float3 pos)
			{
				return mod(pos, 4) - 2;
			}

            float dist_func(float3 pos, float size)
			{
				return length(trans(pos)) - size;
			}

			float3 getNormal(float3 pos, float size)
			{
				float ep = 0.0001;

				float D = dist_func(pos, size);

				return normalize(
					float3 (
						D - dist_func(pos - float3(ep,0,0), size),
						D - dist_func(pos - float3(0,ep,0), size),
						D - dist_func(pos - float3(0,0,ep), size)
					)
				);
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
				float4 col = 0;

				float3 cameraPos = float3(0,0,-10);
				float3 worldPos = float3(i.uv * 2 - 1,0);
				float3 lightDirection = _WorldSpaceLightPos0.xyz;
				
				float3 ray = normalize(worldPos - cameraPos);
				float3 cur = cameraPos;
				
				float sphereSize = 0.5;

				for(int i = 0; i < 128; ++i)
				{
					float D = dist_func(cur, sphereSize);
					if(D < 0.0001)
					{
						float3 normalDirection = getNormal(cur, sphereSize);
						float NdotL = dot(normalDirection, lightDirection);
						col.rgb = NdotL + 0.1;
						col.a = 1;
						break;
					}
					cur += ray * D;
				}

                return col;
            }
            ENDCG
        }
    }
}
