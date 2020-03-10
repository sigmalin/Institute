Shader "Raymarching/SceneView/ScreenSceneView"
{
    Properties
    {
		_DataTex ("Ray marching Data", 2D) = "white" {}
		_DataCount ("Count Ray marching Data", INT) = 4
		_DataIteration ("Iteration Of Count Ray marching Data", FLOAT) = 0.25
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

			sampler2D _DataTex; 
			int _DataCount;
			float _DataIteration;

			float4x4  _ClipToWorld;

			float getDistance(float3 pos, float4 center, int index)
			{
				float Dis = 1000;
				int type = mod(index, 4);

				if(type == 0) Dis = sdBox(pos - center.xyz, float3(center.w, center.w, center.w));
				else if(type == 1) Dis = sdTorus_XZ(pos - center.xyz, float2(center.w + 2, center.w));
				else if(type == 2) Dis = sdSphere(pos - center.xyz, center.w);
				else if(type == 3) Dis = sdCylinder(pos - center.xyz, float2(center.w + 2, center.w));
				return Dis;
			}

            float dist_func(float3 pos)
			{
				float sample = 0;

				fixed4 center = tex2Dlod(_DataTex, float4(sample,0,0,0));
				float len = getDistance(pos, center, 0);

				[loop]
				for(int i = 1; i < _DataCount; ++i)
				{
					sample += _DataIteration;
					fixed4 center = tex2Dlod(_DataTex, float4(sample,0,0,0));
					float Dis = getDistance(pos, center, i);
					len = min(len, Dis);				
				}
				
				return len;
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
				float3 cur = cameraPos;
				
				for(int i = 0; i < 256; ++i)
				{
					float D = dist_func(cur);
					if(D < 0.0001)
					{
						float3 normalDirection = getNormal(cur);
						float NdotL = dot(normalDirection, lightDirection);
						col.rgb = (NdotL + 1) * 0.5;
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
