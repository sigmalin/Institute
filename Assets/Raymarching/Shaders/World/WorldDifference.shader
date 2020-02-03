Shader "Raymarching/World/Difference"
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
				float3 posWorld : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            float dist_func(float3 pos)
			{
				float3 torusCenter = float3(0,0,0);
				float2 torusSize = float2(1.75, 0.25);

				float3 boxCenter = float3(0,0,0);
				float3 boxSize = float3(4,0.5,4);

				float d_Torus = sdTorus_XY(pos - torusCenter, torusSize);
				float d_Box = sdBox(pos - boxCenter, boxSize);
				//return max(-d_Torus, d_Box);
				return max( d_Torus,-d_Box);			
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
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float4 col = 0;

				float3 cameraPos = _WorldSpaceCameraPos.xyz;
				float3 worldPos = i.posWorld.xyz;
				float3 lightDirection = _WorldSpaceLightPos0.xyz;
				
				float3 ray = normalize(worldPos - cameraPos);
				float3 cur = cameraPos;
				

				for(int i = 0; i < 32; ++i)
				{
					float D = dist_func(cur);
					if(D < 0.0001)
					{
						float3 normalDirection = getNormal(cur);
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
