Shader "Raymarching/Ground/InfiniteGround"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            sampler2D _MainTex;
			fixed4 _MainTex_ST;

			float4x4  _ClipToWorld;

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

				if (ray.y == 0) return 0;

				float distToGround = - cameraPos.y / ray.y;
				if (distToGround < 0) return 0;

				float3 groundPos = cameraPos + distToGround * ray;
				return tex2D(_MainTex, TRANSFORM_TEX(groundPos.xz, _MainTex));
            }
            ENDCG
        }
    }
}
