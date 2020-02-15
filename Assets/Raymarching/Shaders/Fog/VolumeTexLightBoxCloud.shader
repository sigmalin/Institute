//https://www.youtube.com/watch?v=4QOcCGI6xOU
Shader "Raymarching/Fog/VolumeTexLightBoxCloud"
{
    Properties
    {
		_FogColor ("Color Of Fog", Color) = (0.65, 0.7, 0.75)

        _Perlin ("Perlin Noise", 3D) = "" {}
		_Voronoi ("Voronoi Noise", 3D) = "" {}
		_A ("Weight of Perlin", Range(0, 1)) = 0.5
		_B ("Weight of Voronoi", Range(0, 1)) = 0.5
		_TA ("Tiling of Perlin", Range(0, 0.1)) = 0.015625
		_TB ("Tiling of Voronoi", Range(0, 0.1)) = 0.00390625

		_DensityMultiplier ("Multiplier of Density", Range(0.1, 10)) = 4
		_DensityThreshold ("Threshold of Density", Range(0, 1)) = 0.1
		_NumSteps ("Number of Ray marching", Range(16, 100)) = 16
		
		_PhaseVal ("Phase Value", Range(0, 1)) = 1
		_LightAbsorption ("Light Absorption", Range(0, 1)) = 0.02
		_DarknessThreshold ("Threshold Of Darkness", Range(0, 1)) = 0.02

		_BoundsMin ("Min Bounds Of Box", Vector) = (-5.0, -2.0, -4.0, 0.0)
		_BoundsMax ("Max Bounds Of Box", Vector) = ( 5.0,  2.0,  4.0, 0.0)
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

			float _DensityMultiplier;
			float _DensityThreshold;
			float _NumSteps;
			float _PhaseVal;
			float _LightAbsorption;
			float _DarknessThreshold;

			float4 _BoundsMin;
			float4 _BoundsMax;

            float4x4  _ClipToWorld;
						
			// Returns (dstToBox, dstInsideBox). If ray misses box, dstInsideBox will be zero
            float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRaydir) {
                // Adapted from: http://jcgt.org/published/0007/03/04/
                float3 t0 = (boundsMin - rayOrigin) * invRaydir;
                float3 t1 = (boundsMax - rayOrigin) * invRaydir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                
                float dstA = max(max(tmin.x, tmin.y), tmin.z);
                float dstB = min(tmax.x, min(tmax.y, tmax.z));

                // CASE 1: ray intersects box from outside (0 <= dstA <= dstB)
                // dstA is dst to nearest intersection, dstB dst to far intersection

                // CASE 2: ray intersects box from inside (dstA < 0 < dstB)
                // dstA is the dst to intersection behind the ray, dstB is dst to forward intersection

                // CASE 3: ray misses box (dstA > dstB)

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInsideBox);
            }

			float density_func(float3 pos)
			{
				// avoid Shader warning 
				// gradient instruction used in a loop with varying iteration; 
				// partial derivatives may have undefined value
				//fixed p = tex3D(_Perlin, pos * _TA).r;
				//fixed v = tex3D(_Voronoi, pos * _TB).r;
				fixed p = tex3Dlod(_Perlin, float4(pos * _TA,0.0)).r;
				fixed v = tex3Dlod(_Voronoi,float4(pos * _TB,0.0)).r;

				float a = p * _A  + v * _B;
				return max(0, a - _DensityThreshold) * _DensityMultiplier;
			}

			float lightmarch(float3 pos) 
			{
                float3 lightDirection = _WorldSpaceLightPos0.xyz;
				float dstInsideBox = rayBoxDst(_BoundsMin, _BoundsMax, pos, 1.0 / lightDirection).y;
                
                float stepSize = dstInsideBox * 0.25;
                float totalDensity = 0;

				//[loop]
				//for(int i = 0; i < 1; ++i)
				//{
					pos += lightDirection * stepSize;
					totalDensity += max(0, density_func(pos) * stepSize);
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
				float3 cameraPos = _WorldSpaceCameraPos.xyz;				
				
				float3 ray = normalize(i.worldDirection);

				float4 res = 0;

				float2 rayBoxInfo = rayBoxDst(_BoundsMin, _BoundsMax, cameraPos, 1.0 / ray);
				float dstToBox = rayBoxInfo.x;
				float dstInsideBox = rayBoxInfo.y;

				if(dstInsideBox == 0) return fixed4(0,0,0,1);
				
				float dstTravelled = 0.0;
				float step = dstInsideBox / _NumSteps;
				float dstLimit = dstInsideBox;

				float transmittance = 1;
				float lightEnergy = 0;
				
				[loop]
				while(dstTravelled < dstLimit)
				{
					float3 cur = cameraPos + (dstToBox + dstTravelled) * ray;

					float density = density_func(cur);
					if(0 < density)
					{
						float lightTransmittance = lightmarch(cur);

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
