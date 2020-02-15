﻿Shader "Raymarching/Noise/FractalPerlinNoise"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

			float random(in float2 st)
			{
				return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
			}

			float noise(in float2 st)
			{
				float2 i = floor(st);
				float2 f = frac(st);

				float a = random(i);
				float b = random(i + float2(1,0));
				float c = random(i + float2(0,1));
				float d = random(i + float2(1,1));

				float2 u = f * f * (3.0 - 2.0 * f);

				return lerp(a, b, u.x) + 
						(c - a) * u.y * (1.0 - u.x) +
						(d - b) * u.x * u.y;
			}

			#define OCTAVES 6
			#define LACUMARITY 2.0
			#define GAIN 0.5
			float fBm(in float2 st)
			{
				float value = 0.0;
				float amplitude = 0.5;
				float frequency = 0.0;

				for(int i = 0; i < OCTAVES; ++i)
				{
					value += amplitude * noise(st);
				
					st *= LACUMARITY;
					amplitude *= GAIN;
				}

				return value;
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
				float2 st = i.uv * 4;
                return fBm(st);
            }
            ENDCG
        }
    }
}
