Shader "InfiniteOcean/InfiniteOcean"
{
	Properties
	{
		[HideInInspector] _WaveTex ("Wave", 2D) = "gray" {}
		[HideInInspector] _RefTex  ("Ref", 2D) = "black" {}
		[HideInInspector] _Center  ("Center", Vector) = (0,0,0,0)

		[NoScaleOffset] _BumpTex  ("Bump", 2D) = "bump" {}
		_BumpAmt  ("BumpAmt", Range(0,100)) = 0
		_VertexDisplacementScale("Displacement Scale", Float) = 0.5
		_NormalScaleFactor("Normal Scale Factor", Float) = 1

		_WaterColor ("WaterColor", Color) = (1,1,1,1)

	    _Specularity ("Specularity", Range(0.01,8)) = 0.3
		_SpecPower("Specularity Power", Range(0,1)) = 1

		[NoScaleOffset] _Foam("Foam (RGB)", 2D) = "white" {}
		[NoScaleOffset] _FoamBump ("Foam Bump(RGB)", 2D) = "bump" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
		LOD 100

		ZWrite On
		Cull back

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "Assets\WaterWave\Lib\Wave.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT; 
			};

			struct v2f
			{
				float2 uvWave : TEXCOORD0;
				float4 ref : TEXCOORD1;

				float4 uvWater : TEXCOORD2;
				float4 uvFoam : TEXCOORD3;

				half3 lightDir : TEXCOORD4;
				half3 viewDir : TEXCOORD5;
				float4 vertex : SV_POSITION;
			};


			sampler2D _WaveTex;
			float4 _WaveTex_TexelSize;
			sampler2D _RefTex;
			float4 _RefTex_TexelSize;
			sampler2D _BumpTex;
			float4 _BumpTex_ST;
			float4x4 _RefW;
			float4x4 _RefVP;			

			float4 _Center;

			float _BumpAmt;
			float _VertexDisplacementScale;
			float _NormalScaleFactor;

			fixed4 _WaterColor;

			half _Specularity;
			half _SpecPower;

			sampler2D _Foam;
			sampler2D _FoamBump;

			inline half Foam(float4 _uv)
			{
				return clamp(tex2D(_Foam, -_uv.xy).r * tex2D(_Foam, _uv.zy).r -0.15, 0, 1);
			}

			inline half Fresnel(half3 _viewDir, half3 _norm)
			{
				return 1.0 - saturate(dot(_viewDir, _norm));
			}
			
			v2f vert (appdata v)
			{
				v2f o;

				float2 worldPos = mul(unity_ObjectToWorld, v.vertex).xz;

				float2 uv = (worldPos - _Center.xz) * _Center.w * 0.5 + 0.5;
				v.vertex.y += WaveHeight(_WaveTex, uv) * _VertexDisplacementScale;		

				o.vertex = UnityObjectToClipPos(v.vertex);				

				o.uvWave = uv;
				o.ref = mul(_RefVP, mul(_RefW, v.vertex));

				o.uvFoam.xy = worldPos*0.1;
				o.uvFoam.z = 0.5;
				o.uvFoam.w = _SinTime.y * 0.5;
				o.uvWater = float4(o.uvFoam.x + _CosTime.x * 0.2, o.uvFoam.y + _SinTime.x *0.3, o.uvFoam.x + _CosTime.y * 0.04, o.uvFoam.y + o.uvFoam.w);

				half3 objSpaceViewDir = ObjSpaceViewDir(v.vertex);
    			half3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) );
				half3x3 rotation = half3x3( v.tangent.xyz, binormal, v.normal );

				o.viewDir = normalize(mul(rotation, objSpaceViewDir));
    			o.lightDir = mul(rotation, float3(-0.16,-0.36,-0.92));

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half foam = Foam(i.uvWater) * i.uvFoam.z;
				half3 tangentNormal0 = (tex2D(_BumpTex, i.uvWater.xy) * 2) +
				                       (tex2D(_BumpTex, i.uvWater.zw) * 2) - 
									   2 + 
									   (tex2D(_FoamBump, i.uvFoam.xy)*4 - 1) * foam +
									   WaveNormal(_WaveTex, i.uvWave, _WaveTex_TexelSize.xy * _NormalScaleFactor);

				half3 tangentNormal = normalize(tangentNormal0);

				half fresnelTerm = Fresnel(i.viewDir, tangentNormal);

				half3 floatVec = normalize(i.viewDir - normalize(i.lightDir));
				half specular = pow(max(dot(floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) * _SpecPower * (1.2 - foam);

				float2 offset = tangentNormal * _BumpAmt - _WaveTex_TexelSize.xy;
				i.ref.xy = offset * i.ref.z + i.ref.xy;
				float4 ref = tex2D(_RefTex, i.ref.xy / i.ref.w * 0.5 + 0.5);

				half3 col = _WaterColor.rgb * _LightColor0.rgb;
				col += lerp(ref.rgb, half3(0,0,0), fresnelTerm) + clamp(foam, 0.0, 1.0)*_LightColor0.b + specular*_LightColor0.rgb;

				return fixed4(col.rgb,1);
			}
			ENDCG
		}
	}
}
