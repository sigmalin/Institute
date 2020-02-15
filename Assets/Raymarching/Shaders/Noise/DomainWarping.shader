//https://thebookofshaders.com/13/
Shader "Raymarching/Noise/DomainWarping"
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

			#define OCTAVES 5
			float fBm(in float2 st)
			{
				float value = 0.0;
				float amplitude = 0.5;
				float2 shift = 100.0;

				float2x2 rot =  float2x2(
					cos(0.5), sin(0.5),
					-sin(0.5), cos(0.5)
				);

				for(int i = 0; i < OCTAVES; ++i)
				{
					value += amplitude * noise(st);				
					st = mul(rot, st) * 2.0 + shift;
					amplitude *= 0.5;
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
				float2 st = i.uv * 3.0;

				float2 q = 0;
				q.x = fBm(st);
				q.y = fBm(st + float2(1,0));

				float2 r = 0;
				r.x = fBm(st + 1.0*q + float2(1.7,9.2) + 0.15*_Time.y);
				r.y = fBm(st + 1.0*q + float2(8.3,2.8) + 0.126*_Time.y);

				float f = fBm(st + r);

				float3 col = 0;

				col = lerp(	float3(0.101961, 0.619608, 0.666667),
							float3(0.666667, 0.666667, 0.498039),
							clamp((f*f*4),0,1));

				col = lerp(	col,
							float3(0, 0, 0.167406),
							clamp(length(q),0,1));

				col = lerp(	col,
							float3(0.666667, 1, 1),
							clamp(length(r.x),0,1));

				col *= f*f*f + 0.6*f*f + 0.5*f;

				return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
