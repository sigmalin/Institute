Shader "DualParaboloid/Shadow/DualParaboloidShadow"
{
    Properties
    {
        _Color  ("Color ", Color) = (0,0,0,0)
		_Front ("_Front", 2D) = "white" {}
		_Rear ("_Rear", 2D) = "white" {}
		_Near ("Near Plane", FLOAT) = 0.2
		_Far  ("Far Plane", FLOAT) = 10
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
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 normalDir : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
            };

			fixed4 _Color;
			sampler2D _Front;
            sampler2D _Rear;
			float _Near;
			float _Far;
			float4x4 _DualParaboloid;

			float getDepthFromARGB32(float4 value)
			{
				const float4 bitSh = float4(1.0 / (256.0 * 256.0 * 256.0), 1.0 / (256.0 * 256.0), 1.0 / 256.0, 1.0);
				return(dot(value, bitSh));
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.normalDir = UnityObjectToWorldNormal(v.normal);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 lightDirection = _WorldSpaceLightPos0.xyz;
				float3 normalDirection = normalize(i.normalDir);

				float3 vPos = mul(_DualParaboloid, i.posWorld);
				float L = length(vPos);

				#if defined(UNITY_REVERSED_Z)
				vPos.z *= -1;
				#endif				

				vPos /= L;

				float2 uvF = vPos.xy / (vPos.z + 1);
				uvF = uvF * 0.5 + 0.5;

				float2 uvR = vPos.xy / (1 - vPos.z);
				uvR = uvR * 0.5 + 0.5;
				uvR.x = 1 - uvR.x;

				//float fDepth = getDepthFromARGB32(lerp(tex2D(_Front, uvF), tex2D(_Rear, uvR), vPos.z < 0));
				float fDepth = lerp(tex2D(_Front, uvF), tex2D(_Rear, uvR), vPos.z < 0);
				float fDistance = (L - _Near) / (_Far-_Near);
				
				float3 col = _Color.rgb * (dot(normalDirection, lightDirection) + 1) * 0.5;


				const float SHADOW_EPSILON = 0.0005;
                //col *= lerp(1, 0, fDepth + SHADOW_EPSILON < fDistance);

				const float ESM_C = 80;
				float diff = max(fDistance - fDepth - SHADOW_EPSILON * 0, 0);
				col *= exp(-ESM_C * diff);
								
				return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
