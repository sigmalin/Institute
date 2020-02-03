Shader "Raymarching/Ray"
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
			Name "FORWARD"

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
			
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 worldPos = float3(i.uv * 2 - 1, 0.1);

				float3 forward = float3(0,0,1);
				float3 up = float3(0,1,0);
				float3 side = cross(up, forward);

				float3 ray = normalize(side * worldPos.x + up * worldPos.y + forward * worldPos.z);

                return fixed4(max(ray,0), 1);
            }
            ENDCG
        }
    }
}
