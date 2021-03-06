﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "DualParaboloid/DrawDualParaboloidCube"
{
    Properties
    {
        _Cubemap ("Cube", Cube) = "grey" {}
		_Bias ("Bias", Range(-0.1, 0)) = -0.001
		_Near ("Near Plane", FLOAT) = 0.2
		_Far  ("Far Plane", FLOAT) = 10
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
				float3 texcoord : TEXCOORD0;
            };

            samplerCUBE _Cubemap;
			float _Bias;
			float _Near;
			float _Far;

            v2f vert (appdata v)
            {
                v2f o;
                
				o.vertex.xyz = UnityObjectToViewPos(v.vertex);
				// Right-handed to Left-handed coordinate system 
				o.vertex.z = -o.vertex.z; 

				float L = length(o.vertex.xyz);
				o.vertex.xyz = o.vertex.xyz / L;
								
				o.vertex.xy /= 1 + o.vertex.z;
				
				//handle upside-down, https://docs.unity3d.com/2020.2/Documentation/Manual/SL-PlatformDifferences.html
				if(_ProjectionParams.x < 0)
					o.vertex.y = -o.vertex.y;
				o.vertex.w = 1;

				o.vertex.z = lerp(((L - _Near) / (_Far-_Near)), -1, o.vertex.z < _Bias);

				o.texcoord = v.vertex.xyz;

				// Convert to Clip Space
				if(UNITY_NEAR_CLIP_VALUE  == 1) // for DirectX (near(1),far(0))
					o.vertex.z = 1 - o.vertex.z;
				else							// for OpenGL (near(-1),far(1))
					o.vertex.z = o.vertex.z - 1;
				
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return texCUBE (_Cubemap, normalize(i.texcoord));
            }
            ENDCG
        }
    }
}
