Shader "Raymarching/Screen/Shadow"
{
    Properties
    {
		_ShadowFade ("Shadow Fade", FLOAT) = 16
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
			#include "..\Raymarching.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float3 worldDirection : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

			float4x4  _ClipToWorld;

			float _ShadowFade;

			float SpecialSphere(float3 pos)
			{
				float3 sphereCenter = float3(0,0.5,0);
				float sphereSize = 1;

				float3 boxCenter = float3(0,0,0);
				float3 boxSize = float3(0.0625,0.0625,0.0625);

				float d_Sphere = sdSphere(pos - sphereCenter, sphereSize);
				float d_Box = sdBox(mod(pos, 0.25) - 0.125, boxSize);

				return max(d_Sphere, d_Box);
			}

            float dist_func(float3 pos)
			{
				float d_Sphere = SpecialSphere(pos);
				float d_Floor = sdFloor(pos);

				return min(d_Sphere, d_Floor);	
			}

			float genShadow(float3 pos, float3 light)
			{
				float h = 0.0;
				float c = 0.001;
				float r = 1.0;
				float shadowCoef = 0.5;

				for(float t = 0.0; t < 50.0; ++t)
				{
					h = dist_func(pos + light * c);
					if(h < 0.001)
					{
						return shadowCoef;
					}

					r = min(r, h * _ShadowFade / c);
					c += h;
				}

				return 1.0 - shadowCoef + r * shadowCoef;			
			}

			float3 getNormal(float3 pos)
			{
				float ep = 0.0001;

				float D = dist_func(pos);

				return normalize(
					float3 (
						D - dist_func(pos - float3(ep,0,0)),
						D - dist_func(pos - float3(0,ep,0)),
						D - dist_func(pos - float3(0,0,ep))
					)
				);
			}

            v2f vert (appdata v)
            {
                v2f o;

				//o.vertex = v.vertex * float4(2, 2, 1, 1) - float4(1, 1, 0, 0);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				//float4 clip = float4((v.uv.xy * 2.0f - 1.0f) * float2(1, -1), 0.0f, 1.0f);
				#if UNITY_UV_STARTS_AT_TOP  
                float4 clip = float4(o.vertex.xy * float2(1,-1), 0.0, 1.0);
#else
                float4 clip = float4(o.vertex.xy, 0.0, 1.0);
#endif
				o.worldDirection = mul(_ClipToWorld, clip) -_WorldSpaceCameraPos;                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float4 col = 0;
				
				float3 cameraPos = _WorldSpaceCameraPos.xyz;
				float3 lightDirection = _WorldSpaceLightPos0.xyz;
				
				float3 ray = normalize(i.worldDirection);
				float3 cur = cameraPos;
								
				for(int i = 0; i < 256; ++i)
				{
					float D = dist_func(cur);
					if(D < 0.0001)
					{
						float3 normalDirection = getNormal(cur);
						float NdotL = max(0.1, dot(normalDirection, lightDirection));
						col.rgb = NdotL;

						float shadow = genShadow(cur + normalDirection * 0.001, lightDirection);
						col.rgb *= shadow;
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
