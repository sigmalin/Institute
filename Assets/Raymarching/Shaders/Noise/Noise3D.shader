Shader "Raymarching/Noise/3D"
{
    Properties
    {
		_Perlin ("Perlin Noise", 3D) = "" {}
		_Voronoi ("Voronoi Noise", 3D) = "" {}
        _A ("Weight of Perlin", Range(0, 1)) = 0.5
		_B ("Weight of Voronoi", Range(0, 1)) = 0.5
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
			#include "..\Hash.cginc"
			#include "..\PerlinNoise.cginc"
			#include "..\WorleyNoise.cginc"
			#include "..\VoronoiNoise.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
				float3 posWorld : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

			sampler3D _Perlin;
			sampler3D _Voronoi;

			float _A;
			float _B;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed p = tex3D(_Perlin, i.posWorld).r;
				fixed v = tex3D(_Voronoi, i.posWorld).r;

				return (p * _A) + (v * _B);
            }
            ENDCG
        }
    }
}
