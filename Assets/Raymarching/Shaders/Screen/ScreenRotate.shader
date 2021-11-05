﻿Shader "Raymarching/Screen/Rotate"
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

            float dist_func(float3 pos)
			{
				float3 q = rotate(pos, Radians(_Time.y * 10.0), float3(1, 0.5, 0));
				
				float3 torusCenter = float3(0,0,0);
				float2 torusSize = float2(1.5, 0.25);

				float3 boxCenter = float3(0,0,0);
				float3 boxSize = float3(2,0.1,0.5);

				float3 cylinderCenter = float3(0,0,0);
				float2 cylinderSize = float2(0.75,0.25);

				float d_Torus = sdTorus_XY(q - torusCenter, torusSize);
				float d_Box = sdBox(q - boxCenter, boxSize);
				float d_Cylinder = sdCylinder(q - cylinderCenter, cylinderSize);
				
				return smoothMin(smoothMin(d_Torus, d_Box, 16.0), d_Cylinder, 16.0);
				//return min(min(d_Torus, d_Box), d_Cylinder);
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
