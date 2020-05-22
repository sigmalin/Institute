Shader "DualParaboloid/DualParaboloid"
{
    Properties
    {
        _DualParaboloid ("DualParaboloid", 2D) = "white" {}
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
                float3 normalDir : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _DualParaboloid;

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
                float3 normalDirection = normalize(i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

				float NdotV = clamp(dot(normalDirection, viewDirection), 0, 1);
				float3 R = 2.0 * NdotV * normalDirection - viewDirection;
				
				float2 uvF = R.xy / (R.z + 1);
				uvF = uvF * 0.5 + 0.5;
				uvF.x *= 0.5;

				float2 uvR = R.xy / (1 - R.z);
				uvR = uvR * 0.5 + 0.5;
				uvR.x = (1 - uvR.x) * 0.5 + 0.5;
				
				float2 uv = lerp(uvF, uvR, R.z < 0);
						
				return tex2D(_DualParaboloid, uv);
            }
            ENDCG
        }
    }
}
