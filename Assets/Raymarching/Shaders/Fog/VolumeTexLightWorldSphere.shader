//https://www.youtube.com/watch?v=4QOcCGI6xOU
Shader "Raymarching/Fog/VolumeTexLightWorldSphere"
{
    Properties
    {
		_FogColor ("Color Of Fog", Color) = (0.65, 0.7, 0.75)

		_Radius ("Radius Of Sphere", FLOAT) = 5

        _Perlin ("Perlin Noise", 3D) = "" {}
		_Voronoi ("Voronoi Noise", 3D) = "" {}
		_A ("Weight of Perlin", Range(0, 1)) = 0.5
		_B ("Weight of Voronoi", Range(0, 1)) = 0.5
		_TA ("Tiling of Perlin", Range(0, 0.1)) = 0.125
		_TB ("Tiling of Voronoi", Range(0, 0.1)) = 0.125

		_DensityMultiplier ("Multiplier of Density", Range(0.1, 10)) = 4
		_DensityThreshold ("Threshold of Density", Range(0, 1)) = 0.1
		_NumSteps ("Number of Ray marching", Range(16, 100)) = 16
		
		_PhaseVal ("Phase Value", Range(0, 1)) = 1
		_LightAbsorption ("Light Absorption", Range(0, 1)) = 0.02
		_DarknessThreshold ("Threshold Of Darkness", Range(0, 1)) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100

		Blend One SrcAlpha

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
            };

            struct v2f
            {
                float3 center : TEXCOORD0;
				float3 posWorld : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

			float4 _FogColor;

			float _Radius;

			sampler3D _Perlin;
			sampler3D _Voronoi;
			float _A;
			float _B;
			float _TA;
			float _TB;

			float _DensityMultiplier;
			float _DensityThreshold;
			float _NumSteps;
			float _PhaseVal;
			float _LightAbsorption;
			float _DarknessThreshold;
						
			float density_func(float3 pos, float3 center)
			{
				// avoid Shader warning 
				// gradient instruction used in a loop with varying iteration; 
				// partial derivatives may have undefined value
				//fixed p = tex3D(_Perlin, pos * _TA).r;
				//fixed v = tex3D(_Voronoi, pos * _TB).r;
				fixed p = tex3Dlod(_Perlin, float4(pos * _TA,0.0)).r;
				fixed v = tex3Dlod(_Voronoi,float4(pos * _TB,0.0)).r;

				float a = p * _A  + v * _B;

				float Weight = clamp(length(pos-center) / _Radius, 0.5, 1.0);

				Weight = 1 - (Weight - 0.5) * 2;

				return max(0, a - _DensityThreshold) * _DensityMultiplier * (Weight*Weight);
			}

			float lightmarch(float3 pos, float3 center) 
			{
                float3 lightDirection = _WorldSpaceLightPos0.xyz;
                
                float stepSize = _Radius * 0.25;
                float totalDensity = 0;

				//[loop]
				//for(int i = 0; i < 1; ++i)
				//{
					pos += lightDirection * stepSize;
					totalDensity += max(0, density_func(pos, center) * stepSize);
				//}

                float transmittance = exp(-totalDensity * _LightAbsorption);
                return _DarknessThreshold + transmittance * (1-_DarknessThreshold);
            }
									
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.center = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
				o.posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 curPos = i.posWorld;			
				
				float3 ray = normalize(curPos - _WorldSpaceCameraPos.xyz);

				float4 res = 0;
				
				float dstTravelled = 0.0;				
				float dstLimit = _Radius * 2.0;
				float step = dstLimit / _NumSteps;

				float transmittance = 1;
				float lightEnergy = 0;
				
				[loop]
				while(dstTravelled < dstLimit)
				{
					float3 cur = curPos + dstTravelled * ray;

					float density = density_func(cur, i.center);
					if(0 < density)
					{
						float lightTransmittance = lightmarch(cur, i.center);

						lightEnergy += density * step * transmittance * lightTransmittance * _PhaseVal;
						transmittance *= exp(-density * step * _LightAbsorption);

						if (transmittance < 0.01) {
                            break;
                        }
					}
					
					dstTravelled += step;
				}

				float3 cloudCol = lightEnergy * _FogColor;
								
                return float4(cloudCol,transmittance);
            }
            ENDCG
        }
    }
}
