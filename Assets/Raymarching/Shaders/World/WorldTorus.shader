Shader "Raymarching/World/Torus"
{
    Properties
    {

    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
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
				float3 center : TEXCOORD1;
				float3 posWorld : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            float dist_func(float3 pos, float3 center, float2 size)
			{
				return sdTorus_XY(pos - center, size);
			}

			float3 getNormal(float3 pos, float3 center, float2 size)
			{
				float ep = 0.0001;

				float D = dist_func(pos, center, size);

				return normalize(
					float3 (
						D - dist_func(pos - float3(ep,0,0), center, size),
						D - dist_func(pos - float3(0,ep,0), center, size),
						D - dist_func(pos - float3(0,0,ep), center, size)
					)
				);
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.center = 0;//mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
				o.posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float4 col = 0;

				float3 cameraPos = _WorldSpaceCameraPos.xyz;
				float3 worldPos = i.posWorld.xyz;
				float3 centerPos = i.center.xyz;
				float3 lightDirection = _WorldSpaceLightPos0.xyz;
				
				float3 ray = normalize(worldPos - cameraPos);
				float3 cur = cameraPos;
				
				float2 torusSize = float2(0.75, 0.25);

				for(int i = 0; i < 16; ++i)
				{
					float D = dist_func(cur, centerPos, torusSize);
					if(D < 0.0001)
					{
						float3 normalDirection = getNormal(cur, centerPos, torusSize);
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
