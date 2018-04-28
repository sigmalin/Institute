Shader "VolumetricCloud/VolumetricCloud"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Cloud Top Color", Color) = (1,1,1,1)
		_ColorB ("Cloud Botton Color", Color) = (1,1,1,1)
		_Factor ("Alpha Factor", Range(0,1)) = 0.4
		_Wind ("Wind", Vector) = (1,0,0,1)
	}
	SubShader
	{
		Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 texcoord2 : TEXCOORD1;
			};

			struct v2f
			{
				half4 pos : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _ColorB;
			fixed _Factor;

			fixed4 _Wind;

			float  _CurrentTime;
			float  _LifeTime;
			
			v2f vert (appdata v)
			{
				float elapsed = _CurrentTime - v.texcoord2.w;

				float4 tv = v.vertex;
				tv.xyz += _Wind.xyz * elapsed;

				half sizeX = v.texcoord2.x;
				half sizeY = v.texcoord2.y;
/*								
				half3 side = UNITY_MATRIX_IT_MV[1].xyz;//UNITY_MATRIX_V[1].xyz;//
				half3 eye = normalize(ObjSpaceViewDir(tv));
				half3 up = cross(side, eye);
*/					
				half3 eye = normalize(ObjSpaceViewDir(tv));
				half3 up = UNITY_MATRIX_V[0].xyz;//UNITY_MATRIX_IT_MV[0].xyz
				half3 side = cross(eye, up);
	
				// rotate
				half3 vec = (v.texcoord.x)*side*sizeX + (v.texcoord.y)*up*sizeY;
				half3 n = eye;
				half theta = v.texcoord2.z;
				/* rotate matrix for an arbitrary axis
				 * Vx*Vx*(1-cos) + cos  	Vx*Vy*(1-cos) - Vz*sin	Vz*Vx*(1-cos) + Vy*sin;
				 * Vx*Vy*(1-cos) + Vz*sin	Vy*Vy*(1-cos) + cos 	Vy*Vz*(1-cos) - Vx*sin;
				 * Vz*Vx*(1-cos) - Vy*sin	Vy*Vz*(1-cos) + Vx*sin	Vz*Vz*(1-cos) + cos;
				 */
				half s, c;
				sincos(theta, s, c);
				half3 n1c = n * (1-c);
				half3 ns = n * s;
				half3x3 mat = {
					(n.x*n1c.x + c),   (n.x*n1c.y - ns.z), (n.z*n1c.x + ns.y),
					(n.x*n1c.y + ns.z), (n.y*n1c.y + c),   (n.y*n1c.z - ns.x),
					(n.z*n1c.x - ns.y), (n.y*n1c.z + ns.x),   (n.z*n1c.z + c),
				};
				half3 rvec = mul(mat, vec);
				tv.xyz += rvec;

            	v2f o;
			    o.pos = mul(UNITY_MATRIX_VP, float4(tv.xyz,1));//UnityObjectToClipPos(float4(tv.xyz,1));
				o.texcoord = MultiplyUV(UNITY_MATRIX_TEXTURE0,
										float4(v.texcoord.xy, 0, 0)) + half2(0.5,0.5);

				
				o.color = lerp(_ColorB, _Color, ((rvec.y + sizeY*0.5)/(sizeY)));

				fixed halfLife = _LifeTime * 0.5;
				halfLife = abs((halfLife - elapsed) / halfLife);
				o.color.a = 1 - saturate(halfLife - 0.9) * 10;
				
            	return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
				fixed a = col.a * _Factor * i.color.a;
				//return fixed4(_Color.rgb,a);
				return fixed4(i.color.rgb,a);
			}
			ENDCG
		}
	}
}
