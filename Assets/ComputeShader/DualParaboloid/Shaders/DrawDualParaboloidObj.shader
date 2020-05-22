// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "DualParaboloid/DrawDualParaboloidObj"
{
    Properties
    {
        _Color  ("Color ", Color) = (0,0,0,0)
		_Bias ("Bias", Range(1, 1.1)) = 1.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		
		Cull Back
		ZWrite On
		ZTest LEQUAL
		
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
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
			float _Bias;

            v2f vert (appdata v)
            {
                v2f o;
                
				o.vertex.xyz = UnityObjectToViewPos(v.vertex.xyz);
				o.vertex.z = -o.vertex.z; 
				
				float L = length(o.vertex.xyz);

				o.vertex.xyz /= L;
				
				o.vertex.xy /= 1 + o.vertex.z;
				o.vertex.y = -o.vertex.y; // for DX render texture
				o.vertex.w = 1;

				o.vertex.xy *= _Bias;
				o.vertex.z = lerp((L - 0.5) / (10-0.5), o.vertex.z, o.vertex.z < 0);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
