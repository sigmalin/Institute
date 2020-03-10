Shader "Raymarching/Blit/BlitSkyPlusFog"
{
    Properties
    {
        _BackGround ("Back Ground", 2D) = "black" {}
		_Sky ("Sky", 2D) = "black" {}
		_Fog ("Fog", 2D) = "black" {}
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _BackGround;
			sampler2D _Sky;
			sampler2D _Fog;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float4 sky = tex2D(_Sky, i.uv);
				float4 fog = tex2D(_Fog, i.uv);

				sky.rgb += sky.a * fog.rgb;
				sky.a = min(fog.a, sky.a);

				float4 bg = tex2D(_BackGround, i.uv);
				
                return fixed4(sky.rgb + sky.a * bg.rgb, 1);
            }
            ENDCG
        }
    }
}
