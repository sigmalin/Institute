Shader "ProceduralPlanets/Planet"
{
    Properties
    {
        _Gradient ("Gradient", 2D) = "white" {}
		_VecMinMax ("Parameters Of Height", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
				float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
            };

            sampler2D _Gradient;
			float4 _VecMinMax;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				o.uv.x = (length(v.vertex.xyz) - _VecMinMax.x) / (_VecMinMax.z);
				o.uv.y = v.uv.x;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col = tex2Dlod(_Gradient, float4(i.uv, 0.0, 0.0));

				float3 lightDirection = _WorldSpaceLightPos0.xyz;
				float NdotL = (clamp(dot(normalize(i.normalDir), lightDirection), 0, 1) + 1) * 0.5;
				col.rgb *= NdotL;
                return col;
            }
            ENDCG
        }
    }
}
