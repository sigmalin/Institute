Shader "Raymarching/Noise/VolumeRender"
{
    Properties
    {
		_Volume("Volume", 3D) = "" {}
		_Iteration("Iteration", Int) = 10
		_Intensity("Intensity", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
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

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
				float4 posLocal : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

			sampler3D _Volume;
			int _Iteration;
			fixed _Intensity;
			
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.posLocal = v.vertex;
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 cameraPos = _WorldSpaceCameraPos.xyz;
				float3 worldPos = i.posWorld.xyz;
				
				float3 wdir = normalize(worldPos - cameraPos);
				float3 ldir = normalize(mul(unity_WorldToObject, wdir));
				float3 lstep = ldir / _Iteration;
				float3 lpos = i.posLocal;

				fixed res = 0;
				
				[loop]
				for(int i = 0; i < _Iteration; ++i)
				{
					fixed D = 1 - tex3D(_Volume, lpos * 0.5).r;
					res += (1 - res) * D * _Intensity;
					lpos += lstep;
				}

                return res;
            }
            ENDCG
        }
    }
}
