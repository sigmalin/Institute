Shader "Ocean/GerstnerWave/Wave"
{
    Properties
    {
        _Amplitude ("Amplitude", Vector) = (0.8, 0.8, 0.4, 0.9)
		_Frequency ("Frequency", Vector) = (0.4, 1.8, 1.0, 1.2)
		_Steepness ("Steepness", Vector) = (0.2, 0.3, 0.7, 0.4)
		_Speed ("Speed", Vector) = (20, 30, 10, 30)
		_DirectionA ("Wave A(X,Y) and B(Z,W)", Vector) = (0.47, 0.35, -0.96, 0.23)
		_DirectionB ("C(X,Y) and D(Z,W)", Vector) = (0.77, -1.47, -0.3, -0.2)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
			#include "Gerstner.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                
                float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 worldPos : TEXCOORD0;
				UNITY_FOG_COORDS(1)				
            };

            float4 _Amplitude;
			float4 _Frequency;
			float4 _Steepness;
			float4 _Speed;
			float4 _DirectionA;
			float4 _DirectionB;

            v2f vert (appdata v)
            {
                v2f o;
                
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                
				float3 pos = float3(0.0, 0.0, 0.0);
				float3 binormal = float3(1.0, 0.0, 0.0);
				float3 tangent = float3(0.0, 0.0, 1.0); 

				float time = _Time.x;				

				pos += GerstnerWave(_Amplitude.x, _Frequency.x, _Steepness.x, _Speed.x, _DirectionA.xy, worldPos.xz, time);
				pos += GerstnerWave(_Amplitude.y, _Frequency.y, _Steepness.y, _Speed.y, _DirectionA.zw, worldPos.xz, time);
				pos += GerstnerWave(_Amplitude.z, _Frequency.z, _Steepness.z, _Speed.z, _DirectionB.xy, worldPos.xz, time);
				pos += GerstnerWave(_Amplitude.w, _Frequency.w, _Steepness.w, _Speed.w, _DirectionB.zw, worldPos.xz, time);

				binormal += CalcBinormal(_Amplitude.x, _Frequency.x, _Steepness.x, _Speed.x, _DirectionA.xy, worldPos.xz, time);
				binormal += CalcBinormal(_Amplitude.y, _Frequency.y, _Steepness.y, _Speed.y, _DirectionA.zw, worldPos.xz, time);
				binormal += CalcBinormal(_Amplitude.z, _Frequency.z, _Steepness.z, _Speed.z, _DirectionB.xy, worldPos.xz, time);
				binormal += CalcBinormal(_Amplitude.w, _Frequency.w, _Steepness.w, _Speed.w, _DirectionB.zw, worldPos.xz, time);

				tangent += CalcTangent(_Amplitude.x, _Frequency.x, _Steepness.x, _Speed.x, _DirectionA.xy, worldPos.xz, time);
				tangent += CalcTangent(_Amplitude.y, _Frequency.y, _Steepness.y, _Speed.y, _DirectionA.zw, worldPos.xz, time);
				tangent += CalcTangent(_Amplitude.z, _Frequency.z, _Steepness.z, _Speed.z, _DirectionB.xy, worldPos.xz, time);
				tangent += CalcTangent(_Amplitude.w, _Frequency.w, _Steepness.w, _Speed.w, _DirectionB.zw, worldPos.xz, time);

				o.normal = normalize(cross(tangent, binormal));
				o.worldPos = worldPos.xyz + pos;
				o.vertex = mul(UNITY_MATRIX_VP, float4(o.worldPos, 1.0));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(1,1,1,1);
            }
            ENDCG
        }
    }
}
