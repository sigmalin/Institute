Shader "GeometryGrass/GeometryGrassShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 4.0  // for geometry shader support

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#ifndef UNITY_PI
#define UNITY_PI 3.1415926
#endif

#ifndef UNITY_HALF_PI
#define UNITY_HALF_PI (UNITY_PI * 0.5)
#endif

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 vertex : POSITION;
            };

            struct g2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = mul(UNITY_MATRIX_M, v.vertex);
                return o;
            }

            [maxvertexcount(8)]
            void geom(point v2g input[1], inout TriangleStream<g2f> triStream)
            {
                g2f o;

                float3 camRight = UNITY_MATRIX_V[0].xyz; // = mul(UNITY_MATRIX_V, float4(1,0,0,0))
                float3 camUp = UNITY_MATRIX_V[1].xyz; // = mul(UNITY_MATRIX_V, float4(0,1,0,0))
                float3 objUp = float3(0,1,0);

                float theta = dot(camUp, objUp);

                const float halfWidth = 0.5;
                const float halfHeight = 5.0 * theta;

                float4 vert = input[0].vertex;

                const float oscillateDelta = 0.05;
                float random = sin(UNITY_HALF_PI * frac(vert.x) + UNITY_HALF_PI * frac(vert.z));

                float dir = 1.0;
                int level = 4;
                float invLevel = 1.0 / level;
                float3 width = camRight * halfWidth;
                float3 height = camUp * invLevel * halfHeight;

                for (int i = 0; i < level; ++i) {
                    
                    float2 wind = float2(sin(_Time.x * UNITY_PI * 5), sin(_Time.x * UNITY_PI * 5));
                    wind.x += (sin(_Time.x + vert.x / 25) + sin((_Time.x + vert.x / 15) + 50)) * 0.5;
                    wind.y += cos(_Time.x + vert.z / 80);
                    wind *= lerp(0.7, 1.0, 1.0 - random);

                    float oscillationStrength = 2.5f;
                    float sinSkewCoeff = random;
                    float lerpCoeff = (sin(oscillationStrength * _Time.x + sinSkewCoeff) + 1.0) / 2;
                    float2 leftWindBound = wind * (1.0 - oscillateDelta);
                    float2 rightWindBound = wind * (1.0 + oscillateDelta);

                    wind = lerp(leftWindBound, rightWindBound, lerpCoeff);

                    float randomAngle = lerp(-UNITY_PI, UNITY_PI, random);
                    float randomMagnitude = lerp(0, 1., random);
                    float2 randomWindDir = float2(sin(randomAngle), cos(randomAngle));
                    wind += randomWindDir * randomMagnitude;

                    float windForce = length(wind);

                    float3 bone = vert.xyz;
                    bone.xz += wind.xy * (i * invLevel);
                    bone.y -= windForce * (i * invLevel) * 0.8;

                    o.vertex = mul(unity_MatrixVP, float4(bone + width + (i * height), 1));
                    o.uv = float2(1, i * invLevel);
                    triStream.Append(o);

                    o.vertex = mul(unity_MatrixVP, float4(bone - width + (i * height), 1));
                    o.uv = float2(0, i * invLevel);
                    triStream.Append(o);
                }                

                //triStream.RestartStrip();
            }

            half4 frag (g2f i) : SV_Target
            {
                return 1;
            }
            ENDHLSL
        }
    }
}
