Shader "Raymarching/Screen/Sphere"
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
				float3 worldDirection : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

			float4x4  _ClipToWorld;

            float dist_func(float3 pos, float size)
			{
				return length(pos) - size;
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
				
				float sphereSize = 1;

				for(int i = 0; i < 16; ++i)
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
