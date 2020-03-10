//https://www.youtube.com/watch?v=4QOcCGI6xOU
Shader "ProceduralPlanets/PlanetSphere"
{
    Properties
    {
		_FogColor ("Color Of Fog", Color) = (0.65, 0.7, 0.75)

		_PlanetRadius ("Radius Of Planet", FLOAT) = 100
		_SkyThickness ("Thickness Of Sky", FLOAT) = 5

        _Perlin ("Perlin Noise", 3D) = "" {}
		_Voronoi ("Voronoi Noise", 3D) = "" {}
		_A ("Weight of Perlin", Range(0, 1)) = 0.5
		_B ("Weight of Voronoi", Range(0, 1)) = 0.5
		_TA ("Tiling of Perlin", Range(0, 1)) = 1
		_TB ("Tiling of Voronoi", Range(0, 1)) = 1

		_DensityMultiplier ("Multiplier of Density", Range(0.1, 10)) = 4
		_DensityThreshold ("Threshold of Density", Range(0, 1)) = 0.1
		_NumSteps ("Number of Ray marching", Range(16, 100)) = 16
		
		_PhaseVal ("Phase Value", Range(0, 1)) = 1
		_LightAbsorption ("Light Absorption", Range(0, 1)) = 0.02
		_DarknessThreshold ("Threshold Of Darkness", Range(0, 1)) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float3 center : TEXCOORD1;
				float3 posWorld : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

			float4 _FogColor;

			sampler3D _Perlin;
			sampler3D _Voronoi;
			float _A;
			float _B;
			float _TA;
			float _TB;

			float _PlanetRadius;
			float _SkyThickness;

			float _DensityMultiplier;
			float _DensityThreshold;
			float _NumSteps;
			float _PhaseVal;
			float _LightAbsorption;
			float _DarknessThreshold;
			
			// http://viclw17.github.io/2018/07/16/raytracing-ray-sphere-intersection/
			float2 hit_sphere(float3 r_origin, float3 r_dir, float3 s_center, float s_radius)
			{
				float3 oc = r_origin - s_center;
				float a = dot(r_dir, r_dir);
				float b = 2.0 * dot(oc, r_dir);
				float c = dot(oc, oc) - s_radius*s_radius;
				float discriminant = b*b - 4*a*c;
				if(discriminant < 0) return float2(0,0);
				
				float s = sqrt(discriminant);
				float minDis = (-b-s) / (2*a);
				float insideDis = (s / a) + min(0, minDis);
				return float2(max(0, minDis), insideDis);
			}
									
			float density_func(float3 pos, float3 center)
			{
				float3 dir = normalize(pos - center);
				float D = length(pos - center);
				dir *= D / (_PlanetRadius + _SkyThickness + _SkyThickness);

				float weight = abs(D - (_PlanetRadius + _SkyThickness)) / _SkyThickness;
				weight = 1 - clamp(weight, 0, 1);
				weight *= 2;

				fixed p = tex3Dlod(_Perlin, float4(dir * _TA,0.0)).r;
				fixed v = tex3Dlod(_Voronoi,float4(dir * _TB,0.0)).r;

				float a = p * _A  + v * _B;
				a *= weight;

				return max(0, a - _DensityThreshold) * _DensityMultiplier;
			}

			float lightmarch(float3 pos, float3 center) 
			{
                float3 lightDirection = _WorldSpaceLightPos0.xyz;

				float2 hitInfo = hit_sphere(pos, lightDirection, center, _PlanetRadius + _SkyThickness + _SkyThickness);
                
                float stepSize = hitInfo.y * 0.25;
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
                o.uv = v.uv;                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 cameraPos = _WorldSpaceCameraPos.xyz;	
				float3 worldPos = i.posWorld.xyz;
				float3 centerPos = i.center.xyz;
				
				float3 ray = normalize(worldPos - cameraPos);

				float2 hitInfo = hit_sphere(worldPos, ray, centerPos, _PlanetRadius);
				
				float dstLimit = hitInfo.y;
				float dstTravelled = 0.0;
				float step = dstLimit / _NumSteps;	
				
				cameraPos = worldPos;

				float transmittance = 1;
				float lightEnergy = 0;
				
				[loop]
				while(dstTravelled < dstLimit)
				{
					float3 cur = cameraPos + dstTravelled * ray;

					float density = density_func(cur, centerPos);
					if(0 < density)
					{
						float lightTransmittance = lightmarch(cur, centerPos);

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
