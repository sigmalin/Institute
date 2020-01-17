//https://github.com/Scrawk/Phillips-Ocean/blob/master/Assets/PhillipsOcean/Scripts/Ocean.cs
Shader "FourierOcean/FFT_Spectrum"
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
			#include "FastFourierOcean.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

			struct FragmentOutput
			{
				fixed4 height : SV_Target0;
				fixed4 slope : SV_Target1;
				fixed4 displacement : SV_Target2;
			};


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            FragmentOutput frag (v2f i)
            {
				FragmentOutput o;

				float2 height;
				float4 slope;
				float4 displacement;

				FFT_Spectrum(i.uv, _Time.y, height, slope, displacement);
				
				o.height = fixed4(height,0,0);
				o.slope = slope;
				o.displacement = displacement;

                return o;
            }
            ENDCG
        }
    }
}
