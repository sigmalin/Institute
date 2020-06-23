Shader "SRP/PostEffect/FXAA"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}

		// Trims the algorithm from processing darks.
		//   0.0833 - upper limit (default, the start of visible unfiltered edges)
		//   0.0625 - high quality (faster)
		//   0.0312 - visible limit (slower)		
		_ContrastThreshold ("ContrastThreshold", float) = 0.0625

		// The minimum amount of local contrast required to apply algorithm.
		//   0.333 - too little (faster)
		//   0.250 - low quality
		//   0.166 - default
		//   0.125 - high quality 
		//   0.063 - overkill (slower)
		_RelativeThreshold ("RelativeThreshold", float) = 0.333
		
		_SubpixelBlending ("SubpixelBlending", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#pragma target 3.5
			//#pragma prefer_hlslcc gles
			
            #include "MyUnlit.hlsl"

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_TexelSize;
				float _ContrastThreshold;
				float _RelativeThreshold;
				float _SubpixelBlending;
			CBUFFER_END

			#define FXAA_SPAN_MAX	8.0
			#define FXAA_REDUCE_MUL 1.0/8.0
			#define FXAA_REDUCE_MIN 1.0/128.0

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

			struct LuminanceData 
			{
				float m, n, e, s, w;
				float ne, nw, se, sw;
				float highest, lowest, contrast;
			};

			struct EdgeData 
			{
				bool isHorizontal;
				float pixelStep;
				float oppositeLuminance, gradient;
			};

			float4 SampleTexture (float2 uv) 
			{
				return SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, uv, 0);
			}

			float4 SampleTexture (float2 uv, float2 offset) 
			{
				uv += _MainTex_TexelSize.xy * offset;
				return SampleTexture(uv);
			}

			LuminanceData SampleLuminanceNeighborhood (float2 uv) 
			{
				LuminanceData l;
				l.m = SampleTexture(uv).a;
				l.n = SampleTexture(uv, float2( 0,  1)).a;
				l.e = SampleTexture(uv, float2( 1,  0)).a;
				l.s = SampleTexture(uv, float2( 0, -1)).a;
				l.w = SampleTexture(uv, float2(-1,  0)).a;

				l.ne = SampleTexture(uv, float2( 1,  1)).a;
				l.nw = SampleTexture(uv, float2(-1,  1)).a;
				l.se = SampleTexture(uv, float2( 1, -1)).a;
				l.sw = SampleTexture(uv, float2(-1, -1)).a;

				l.highest = max(max(max(max(l.n, l.e), l.s), l.w), l.m);
				l.lowest = min(min(min(min(l.n, l.e), l.s), l.w), l.m);
				l.contrast = l.highest - l.lowest;
				return l;
			}

			bool ShouldSkipPixel (LuminanceData l) 
			{
				float threshold =
					max(_ContrastThreshold, _RelativeThreshold * l.highest);
				return l.contrast < threshold;
			}

			float DeterminePixelBlendFactor (LuminanceData l) 
			{
				float filter = 2 * (l.n + l.e + l.s + l.w);
				filter += l.ne + l.nw + l.se + l.sw;
				filter *= 1.0 / 12;
				filter = abs(filter - l.m);
				filter = saturate(filter / l.contrast);

				float blendFactor = smoothstep(0, 1, filter);
				return blendFactor * blendFactor * _SubpixelBlending;
			}

			EdgeData DetermineEdge (LuminanceData l) 
			{
				EdgeData e;
				float horizontal =
					abs(l.n + l.s - 2 * l.m) * 2 +
					abs(l.ne + l.se - 2 * l.e) +
					abs(l.nw + l.sw - 2 * l.w);
				float vertical =
					abs(l.e + l.w - 2 * l.m) * 2 +
					abs(l.ne + l.nw - 2 * l.n) +
					abs(l.se + l.sw - 2 * l.s);
				e.isHorizontal = horizontal >= vertical;

				float pLuminance = e.isHorizontal ? l.n : l.e;
				float nLuminance = e.isHorizontal ? l.s : l.w;
				float pGradient = abs(pLuminance - l.m);
				float nGradient = abs(nLuminance - l.m);

				e.pixelStep =
					e.isHorizontal ? _MainTex_TexelSize.y : _MainTex_TexelSize.x;
			
				if (pGradient < nGradient) {
					e.pixelStep = -e.pixelStep;
					e.oppositeLuminance = nLuminance;
					e.gradient = nGradient;
				}
				else {
					e.oppositeLuminance = pLuminance;
					e.gradient = pGradient;
				}

				return e;
			}

			#if defined(LOW_QUALITY)
				#define EDGE_STEP_COUNT 4
				#define EDGE_STEPS 1, 1.5, 2, 4
				#define EDGE_GUESS 12
			#else
				#define EDGE_STEP_COUNT 10
				#define EDGE_STEPS 1, 1.5, 2, 2, 2, 2, 2, 2, 2, 4
				#define EDGE_GUESS 8
			#endif

			static const float edgeSteps[EDGE_STEP_COUNT] = { EDGE_STEPS };

			float DetermineEdgeBlendFactor (LuminanceData l, EdgeData e, float2 uv) 
			{
				float2 uvEdge = uv;
				float2 edgeStep;
				if (e.isHorizontal) {
					uvEdge.y += e.pixelStep * 0.5;
					edgeStep = float2(_MainTex_TexelSize.x, 0);
				}
				else {
					uvEdge.x += e.pixelStep * 0.5;
					edgeStep = float2(0, _MainTex_TexelSize.y);
				}

				float edgeLuminance = (l.m + e.oppositeLuminance) * 0.5;
				float gradientThreshold = e.gradient * 0.25;

				float2 puv = uvEdge + edgeStep * edgeSteps[0];
				float pLuminanceDelta = SampleTexture(puv).a - edgeLuminance;
				bool pAtEnd = abs(pLuminanceDelta) >= gradientThreshold;

				UNITY_UNROLL
				for (int i = 1; i < EDGE_STEP_COUNT && !pAtEnd; i++) {
					puv += edgeStep * edgeSteps[i];
					pLuminanceDelta = SampleTexture(puv).a - edgeLuminance;
					pAtEnd = abs(pLuminanceDelta) >= gradientThreshold;
				}
				if (!pAtEnd) {
					puv += edgeStep * EDGE_GUESS;
				}

				float2 nuv = uvEdge - edgeStep * edgeSteps[0];
				float nLuminanceDelta = SampleTexture(nuv).a - edgeLuminance;
				bool nAtEnd = abs(nLuminanceDelta) >= gradientThreshold;

				UNITY_UNROLL
				for (int j = 1; j < EDGE_STEP_COUNT && !nAtEnd; j++) {
					nuv -= edgeStep * edgeSteps[j];
					nLuminanceDelta = SampleTexture(nuv).a - edgeLuminance;
					nAtEnd = abs(nLuminanceDelta) >= gradientThreshold;
				}
				if (!nAtEnd) {
					nuv -= edgeStep * EDGE_GUESS;
				}

				float pDistance, nDistance;
				if (e.isHorizontal) {
					pDistance = puv.x - uv.x;
					nDistance = uv.x - nuv.x;
				}
				else {
					pDistance = puv.y - uv.y;
					nDistance = uv.y - nuv.y;
				}

				float shortestDistance;
				bool deltaSign;
				if (pDistance <= nDistance) {
					shortestDistance = pDistance;
					deltaSign = pLuminanceDelta >= 0;
				}
				else {
					shortestDistance = nDistance;
					deltaSign = nLuminanceDelta >= 0;
				}

				if (deltaSign == (l.m - edgeLuminance >= 0)) {
					return 0;
				}
				return 0.5 - shortestDistance / (pDistance + nDistance);
			}

			float4 ApplyFXAA (float2 uv) 
			{
				float4 col = 0;
				LuminanceData l = SampleLuminanceNeighborhood(uv);
				if (ShouldSkipPixel(l)) 
				{
					col = SampleTexture(uv);
				}
				else
				{
					float pixelBlend = DeterminePixelBlendFactor(l);
					EdgeData e = DetermineEdge(l);
					float edgeBlend = DetermineEdgeBlendFactor(l, e, uv);
					float finalBlend = max(pixelBlend, edgeBlend);

					if (e.isHorizontal) {
						uv.y += e.pixelStep * finalBlend;
					}
					else {
						uv.x += e.pixelStep * finalBlend;
					}
					col = float4(SampleTexture(uv).rgb, l.m);
				}

				return col;				
			}


            v2f vert (appdata v)
            {
                v2f o;
				
				float4 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = mul(unity_MatrixVP, worldPos);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
				return ApplyFXAA(i.uv);
            }
            ENDHLSL
        }
    }
}
