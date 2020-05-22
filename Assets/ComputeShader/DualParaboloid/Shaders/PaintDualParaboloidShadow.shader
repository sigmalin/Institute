Shader "DualParaboloid/Shadow//PaintDualParaboloidShadow"
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

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float ClipDepth : TEXCOORD1;
				float Depth : TEXCOORD2;
            };

			float4 mapDepthToARGB32(float value)
			{
				const float4 bitSh = float4(256.0 * 256.0 * 256.0, 256.0 * 256.0, 256.0, 1.0);
				const float4 mask = float4(0.0, 1.0 / 256.0, 1.0 / 256.0, 1.0 / 256.0);
				float4 res = frac(value * bitSh);
				res -= res.xxyz * mask;
				return res;
			}


            v2f vert (appdata v)
            {
                v2f o;

                o.vertex.xyz = UnityObjectToViewPos(v.vertex.xyz);
				o.vertex.z = -o.vertex.z; 
				
				float L = length(o.vertex.xyz);

				o.vertex.xyz /= L;
				o.ClipDepth = o.vertex.z;

				o.vertex.xy /= 1 + o.vertex.z;
				o.vertex.y = -o.vertex.y; // for DX render texture

				o.vertex.z = (L - 0.1) / (100-0.1);
				o.vertex.w = 1;
								
				o.vertex.xy *= 1.01;
				o.Depth = o.vertex.z;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                clip(i.ClipDepth);

                //return mapDepthToARGB32(i.Depth);
				return i.Depth;
            }
            ENDCG
        }
    }
}
