Shader "Raymarching/Noise/Tex3D"
{
    Properties
    {
		_Volume ("Main Texture", 3D) = "white" {}
		_Scale ("Scale", Range(0.001, 10)) = 1
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
            };

            struct v2f
            {
				float3 posWorld : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

			sampler3D _Volume;

			float _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 st = i.posWorld * _Scale;

				return tex3D(_Volume, st);
            }
            ENDCG
        }
    }
}
