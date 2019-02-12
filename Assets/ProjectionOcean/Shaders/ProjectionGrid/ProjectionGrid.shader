Shader "Projection/Grid"
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
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "ProjectionGrid.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;

				float4 worldPos : TEXCOORD1;
			};

			
			v2f vert (appdata v)
			{
				float2 uv = v.uv.xy;

				//Interpolate between frustums world space projection points. p is in world space.
				float4 p = PROJECTION_TO_WORLD(uv);

				//displacement
				float4 dp = float4(0, 0, 0, 0);
				
				v2f OUT;
    			OUT.vertex = mul(UNITY_MATRIX_VP, p+dp);
				OUT.uv = v.uv;
				OUT.worldPos = p + dp;
				return OUT;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(1,0,0,1);
			}
			ENDCG
		}
	}
}
