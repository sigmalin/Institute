Shader "Raymarching/SceneView/ScreenSceneViewCloud"
{
    Properties
    {
		_FogColor ("Color Of Fog", Color) = (0.65, 0.7, 0.75)

		_MaxDistance ("Max Distance Of Fog", FLOAT) = 100

        _Perlin ("Perlin Noise", 3D) = "" {}
		_Voronoi ("Voronoi Noise", 3D) = "" {}
		_A ("Weight of Perlin", Range(0, 1)) = 0.5
		_B ("Weight of Voronoi", Range(0, 1)) = 0.5
		_TA ("Tiling of Perlin", Range(0, 0.1)) = 0.015625
		_TB ("Tiling of Voronoi", Range(0, 0.1)) = 0.00390625

		_DensityMultiplier ("Multiplier of Density", Range(0.1, 10)) = 4
		_DensityThreshold ("Threshold of Density", Range(0, 1)) = 0.1
		_StepDistance ("Distance of Step", Range(0.1, 2)) = 0.25
		
		_PhaseVal ("Phase Value", Range(0, 1)) = 1
		_LightAbsorption ("Light Absorption", Range(0, 1)) = 0.02
		_DarknessThreshold ("Threshold Of Darkness", Range(0, 1)) = 0.02

		_DataTex ("Ray marching Data", 2D) = "white" {}
		_DataCount ("Count Ray marching Data", INT) = 4
		_DataIteration ("Iteration Of Count Ray marching Data", FLOAT) = 0.25
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

		Blend One One

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

			float4 _FogColor;

			sampler3D _Perlin;
			sampler3D _Voronoi;
			float _A;
			float _B;
			float _TA;
			float _TB;

			float _MaxDistance;

			float _DensityMultiplier;
			float _DensityThreshold;
			float _StepDistance;
			float _PhaseVal;
			float _LightAbsorption;
			float _DarknessThreshold;

			sampler2D _DataTex; 
			int _DataCount;
			float _DataIteration;

			float4x4  _ClipToWorld;

            float dist_func(float3 pos)
			{
				float sample = 0;
				float len = 1000.0;

				[loop]
				for(int i = 0; i < _DataCount; ++i)
				{
					fixed4 cloud = tex2Dlod(_DataTex, float4(sample,0,0,0));
					float cloud_Dis = sdSphere(pos - cloud.xyz, cloud.w);
					len = min(len, cloud_Dis);

					sample += _DataIteration;
				}
				
				return len;
			}

			float density_func(float3 pos, float4 cloud)
			{
				float dis = length(pos - cloud.xyz);
				dis = clamp(dis / cloud.w, 0.5, 1);
				float Weight = (1 - dis) * 2;
				
				fixed p = tex3Dlod(_Perlin, float4(pos * _TA,0.0)).r;
				fixed v = tex3Dlod(_Voronoi,float4(pos * _TB,0.0)).r;

				float a = p * _A  + v * _B;

				a *= Weight;

				return max(0, a - _DensityThreshold) * _DensityMultiplier;
			}

			float3 getDensity(float3 pos)
			{
				float sample = 0;
				float D = 0;

				[loop]
				for(int i = 0; i < _DataCount; ++i)
				{
					fixed4 cloud = tex2Dlod(_DataTex, float4(sample,0,0,0));
					D += density_func(pos, cloud);

					sample += _DataIteration;
				}

				return D;
			}

			float lightmarch(float3 pos) 
			{
                float3 lightDirection = _WorldSpaceLightPos0.xyz;
                
                float stepSize = _StepDistance;
                float totalDensity = 0;

				//[loop]
				//for(int i = 0; i < 1; ++i)
				//{
					pos += lightDirection * stepSize;
					totalDensity += max(0, getDensity(pos) * stepSize);
				//}

                float transmittance = exp(-totalDensity * _LightAbsorption);
                return _DarknessThreshold + transmittance * (1-_DarknessThreshold);
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
				float4 col = 0;
				
				float3 cameraPos = _WorldSpaceCameraPos.xyz;
				float3 lightDirection = _WorldSpaceLightPos0.xyz;
				float3 ray = normalize(i.worldDirection);

				float dstLimit = _MaxDistance;
				float dstTravelled = min(dist_func(cameraPos), _MaxDistance);
				float step = _StepDistance;	

				float transmittance = 1;
				float lightEnergy = 0;

				[loop]
				while(dstTravelled < dstLimit)
				{
					float3 cur = cameraPos + dstTravelled * ray;

					float density = getDensity(cur);
					if(0 < density)
					{
						float lightTransmittance = lightmarch(cur);

						lightEnergy += density * step * transmittance * lightTransmittance * _PhaseVal;
						transmittance *= exp(-density * step * _LightAbsorption);

						if (transmittance < 0.1) {
                            break;
                        }
						/*
						if(1 < lightEnergy) {
							break;
						}
						*/
					}
					
					dstTravelled += step;
				}
				
				float3 cloudCol = lightEnergy * _FogColor;
								
                return float4(cloudCol,transmittance);
				//return float4(cloudCol,1);
            }
            ENDCG
        }
    }
}
