//https://slsdo.github.io/volumetric-cloud/
Shader "Raymarching/Fog/VolumeSphereFog"
{
    Properties
    {
        _FogColor ("Color Of Fog", Color) = (0.65, 0.7, 0.75)
		_A ("Weight of Perlin", Range(0, 1)) = 0.5
		_B ("Weight of Inversed Worley", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

		Blend One OneMinusSrcAlpha

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
			#include "..\Hash.cginc"
			#include "..\PerlinNoise.cginc"
			#include "..\WorleyNoise.cginc"
			
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

			float4 _FogColor;
			float _A;
			float _B;

            float4x4  _ClipToWorld;
						
			float noise(float3 pos)
			{
				float p = FractalPerlin(pos * 0.5);

				float2 uv = pos.xy + pos.z * float2(0.7071,0.7071);
				float w1 = 1 - Worley(uv * 1, 1);

				return p * _A  + w1 * _B;
			}

			float density_func(float3 pos)
			{
				float D = clamp((5 - length(pos))*0.2, 0, 1);
				return D * noise(pos);
			}
									
            v2f vert (appdata v)
            {
                v2f o;

				//o.vertex = v.vertex * float4(2, 2, 1, 1) - float4(1, 1, 0, 0);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				//float4 clip = float4((v.uv.xy * 2.0f - 1.0f) * float2(1, -1), 0.0f, 1.0f);
				float4 clip = float4(o.vertex.xy, 0.0, 1.0);
				o.worldDirection = mul(_ClipToWorld, clip) -_WorldSpaceCameraPos;                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 cameraPos = _WorldSpaceCameraPos.xyz;				
				
				float3 ray = normalize(i.worldDirection);
				float3 cur = cameraPos;
				
				const int nbSample = 20;

				float3 bgCol = 0;

				float len = max(length(cur) - 5, 0);

				float Transmittance = 1;
				const float step = 0.5;
				float4 res = 0;

				[loop]
				for(int i = 0; i < nbSample; ++i)
				{
					cur = cameraPos + len * ray;

					float density = density_func(cur);

					if(0.0 < density)
					{
						const float extinction = 0.003;

						float3 col = _FogColor.rgb;
						float deltaT = exp(-extinction * step * density);
						float T = Transmittance;
						Transmittance *= deltaT;

						if(Transmittance < 1e-6) break;

						res.rgb += (1-deltaT) / extinction * col * T * Transmittance;
					}

					len += step;
				}

				res.rgb += bgCol * (Transmittance);	
				res.a = 1 - Transmittance;
                return res;
            }
            ENDCG
        }
    }
}
