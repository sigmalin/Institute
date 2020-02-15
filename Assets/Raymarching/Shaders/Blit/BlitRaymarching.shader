Shader "Raymarching/Blit/BlitRaymarching"
{
    Properties
    {
        _BackGround ("Back Ground", 2D) = "black" {}
		_Raymarching ("Ray Marching", 2D) = "black" {}
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
			sampler2D _Raymarching;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float4 bg = tex2D(_BackGround, i.uv);
				float4 marching = tex2D(_Raymarching, i.uv);
                return fixed4(marching.rgb + marching.a * bg.rgb, 1);
            }
            ENDCG
        }
    }
}
