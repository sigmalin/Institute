Shader "Stochastic/Stochastic"
{
    Properties
    {
        _Tex ("T", 2D) = "white" {}
		_invTex ("invT", 2D) = "white" {}
		_Blend ("Blend", Range(0,1)) = 0

		[HideInInspector] _CompressionScalers ("Compression Scalers", Vector) = (3.968248,3.867461,14.57461,1)
		[HideInInspector] _InputSize ("Size of T", Vector) = (1024,1024,10, 0)
		[HideInInspector] _ColorSpaceOrigin ("Color Space Origin", Vector) = (0.1222864,0.3329407,0.1086722)
		[HideInInspector] _ColorSpaceVector1 ("Color Space Vector 1", Vector) = (0.2486029,0.04106447,0.003807663)
		[HideInInspector] _ColorSpaceVector2 ("Color Space Vector 2", Vector) = (-0.03818024,0.2188785,0.132256)
		[HideInInspector] _ColorSpaceVector3 ("Color Space Vector 3", Vector) = (0.004841275,-0.03477486,0.05894864)
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
			#include "Stochastic.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 WorldSpacePosition : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.WorldSpacePosition = mul(UNITY_MATRIX_M,v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return StochasticForColor(i.WorldSpacePosition.xz);
            }
            ENDCG
        }
    }
}
