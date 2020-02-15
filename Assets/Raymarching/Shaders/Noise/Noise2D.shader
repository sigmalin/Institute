Shader "Raymarching/Noise/2D"
{
    Properties
    {
		_Perlin ("Perlin Noise", 2D) = "black" {}
		_Voronoi ("Voronoi Noise", 2D) = "black" {}
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uvPerlin : TEXCOORD0;
				float2 uvVoronoi : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

			sampler2D _Perlin;
			float4 _Perlin_ST;

			sampler2D _Voronoi;
			float4 _Voronoi_ST;

			float _A;
			float _B;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvPerlin = TRANSFORM_TEX(v.uv, _Perlin);
				o.uvVoronoi = TRANSFORM_TEX(v.uv, _Voronoi);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed p = tex2D(_Perlin, i.uvPerlin).r;
				fixed v = tex2D(_Voronoi, i.uvVoronoi).r;
				
				return (p * _A) + (v * _B);
            }
            ENDCG
        }
    }
}
