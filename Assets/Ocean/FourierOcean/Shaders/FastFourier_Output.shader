//https://github.com/Scrawk/Phillips-Ocean/blob/master/Assets/PhillipsOcean/Scripts/Ocean.cs
Shader "FourierOcean/FFT_Output"
{
    Properties
    {
        _Height ("Height", 2D) = "white" {}
		_Slope ("Slope", 2D) = "white" {}
		_Displacement ("Dispersion", 2D) = "white" {}

		_FourierSize ("FourierSize (Power Of 2)", INT) = 128
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
			#include "FastFourierOcean.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uvNormal : TEXCOORD0;
				float3 uvDisplacement : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

			struct FragmentOutput
			{
				fixed4 normal : SV_Target0;
				fixed4 displacement : SV_Target1;
			};


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

				float2 uv = v.uv.xy;
				float sign = v.uv.z;

				float  h;
				float2 d;
				float3 n;
                
				FFT_Output(uv, sign, h, d, n);

				float lambda = -1.0;

				o.uvNormal = n;
				o.uvDisplacement.x = lambda * d.x;
				o.uvDisplacement.y = lambda * d.y;
				o.uvDisplacement.z = h;

                return o;
            }

            FragmentOutput frag (v2f i)
            {
				FragmentOutput o;
				
				float3 nor = normalize(i.uvNormal);
				o.normal = fixed4(((nor + 1) * 0.5).rbg, 1);
				
				//o.displacement = fixed4(i.uvDisplacement, 0);

				float2 noise = 0.05 * o.normal.rg;
				float2 dDdx = ddx(i.uvDisplacement.xy);
				float2 dDdy = ddy(i.uvDisplacement.xy);
				float jacobian = (1 + dDdx.x) * (1 + dDdy.y) - dDdx.y * dDdy.x;
				float turb = max(0, 1.1 - jacobian + length(noise)); // jacobian threshold
				float xx = 1 + 3 * smoothstep(1.2, 1.8, turb);
				xx = min(turb, 1);
				xx = smoothstep(0, 1, turb);
				o.displacement = fixed4(i.uvDisplacement, xx);

                return o;
            }
            ENDCG
        }
    }
}
