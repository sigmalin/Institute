//https://www.keithlantz.net/2011/10/ocean-simulation-part-one-using-the-discrete-fourier-transform/
Shader "FourierOcean/DiscreteFourier"
{
    Properties
    {
		_HTidle0 ("HTidle 0", 2D) = "white" {}
		_Dispersion ("Dispersion", 2D) = "white" {}

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
			#include "DiscreteFourierOcean.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
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

				float2 xz = v.uv;

				float2 h, d;
				float3 n;

				DFT(xz, _Time.y, h, d, n);
				//DFT(xz, 0, h, d, n);

				float lambda = -1.0;

				o.uvNormal = n;
				o.uvDisplacement.x = lambda * d.x;
				o.uvDisplacement.y = lambda * d.y;
				o.uvDisplacement.z = h.x;

                return o;
            }

            FragmentOutput frag (v2f i)
            {
				FragmentOutput o;

				float3 nor = normalize(i.uvNormal);
				o.normal = fixed4(((nor + 1) * 0.5).rbg, 1);

				o.displacement = fixed4(i.uvDisplacement, 0);
                return o;
            }
            ENDCG
        }
    }
}
