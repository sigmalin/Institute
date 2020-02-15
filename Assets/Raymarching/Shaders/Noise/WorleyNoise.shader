Shader "Raymarching/Noise/WorleyNoise"
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

				float dist = 1.0;

				for(int y = -1; y <= 1; ++y)
				{
					for(int x = -1; x <= 1; ++x)
					{
						float2 neighbor = float2(float(x), float(y));
						float2 p = random(i + neighbor);
						
						p = 0.5 + 0.5 * sin(6.2831 * p  + _Time.w);

						float2 diff = neighbor + p - f;
						float len = length(diff);

						dist = min(dist, len);
					}
				}

				return dist;
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
				float2 st = i.uv * 3;
                return noise(st);
            }
            ENDCG
        }
    }
}
