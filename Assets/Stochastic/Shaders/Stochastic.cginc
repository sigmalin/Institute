#ifndef _STOCHASTIC_
#define _STOCHASTIC_

sampler2D _Tex;
float4 _Tex_ST;

sampler2D _invTex;
float4 _invTex_ST;

float _Blend;
float4 _CompressionScalers;
float4 _InputSize;
float4 _ColorSpaceOrigin;
float4 _ColorSpaceVector1;
float4 _ColorSpaceVector2;
float4 _ColorSpaceVector3;

float4 Stochastic(float2 uv)
{
	float4 res = 0;				
	float2 uvScaled = uv * 3.464; // 2 * sqrt(3)

	const float2x2 gridToSkewedGrid = float2x2(1.0, 0.0, -0.57735027, 1.15470054);
	float2 skewedCoord = mul(gridToSkewedGrid, uvScaled);

	int2 baseId = int2(floor(skewedCoord));
	float3 temp = float3(frac(skewedCoord), 0);
	temp.z = 1.0 - temp.x - temp.y;

	float w1, w2, w3;
	int2 vertex1, vertex2, vertex3;
	if (temp.z > 0.0)
	{
        w1 = temp.z;
        w2 = temp.y;
        w3 = temp.x;
        vertex1 = baseId;
        vertex2 = baseId + int2(0, 1);
		vertex3 = baseId + int2(1, 0);
	}
	else
	{
        w1 = -temp.z;
        w2 = 1.0 - temp.y;
        w3 = 1.0 - temp.x;
		vertex1 = baseId + int2(1, 1);
		vertex2 = baseId + int2(1, 0);
		vertex3 = baseId + int2(0, 1);
	}

	float2 uv1 = uv + frac(sin(mul(float2x2(127.1, 311.7, 269.5, 183.3), (float2)vertex1)) * 43758.5453);
	float2 uv2 = uv + frac(sin(mul(float2x2(127.1, 311.7, 269.5, 183.3), (float2)vertex2)) * 43758.5453);
	float2 uv3 = uv + frac(sin(mul(float2x2(127.1, 311.7, 269.5, 183.3), (float2)vertex3)) * 43758.5453);

	float2 duvdx = ddx(uv);
	float2 duvdy = ddy(uv);

	float4 G1 = tex2Dgrad(_Tex, uv1, duvdx, duvdy);
	float4 G2 = tex2Dgrad(_Tex, uv2, duvdx, duvdy);
	float4 G3 = tex2Dgrad(_Tex, uv3, duvdx, duvdy);

	float exponent = 1.0 + _Blend * 15.0;
	w1 = pow(w1, exponent);
	w2 = pow(w2, exponent);
	w3 = pow(w3, exponent);
	float sum = w1 + w2 + w3;
	w1 = w1 / sum;
	w2 = w2 / sum;
	w3 = w3 / sum;

	float4 G = w1 * G1 + w2 * G2 + w3 * G3;
	G = G - 0.5;
	G = G * rsqrt(w1 * w1 + w2 * w2 + w3 * w3);
	G = G * _CompressionScalers;
	G = G + 0.5;

	duvdx *= _InputSize.xy;
	duvdy *= _InputSize.xy;
	float delta_max_sqr = max(dot(duvdx, duvdx), dot(duvdy, duvdy));
	float mml = 0.5 * log2(delta_max_sqr);
	float LOD = max(0, mml) /_InputSize.z;

	res.r = tex2Dlod(_invTex, float4(G.r, LOD, 0, 0)).r;
	res.g = tex2Dlod(_invTex, float4(G.g, LOD, 0, 0)).g;
	res.b = tex2Dlod(_invTex, float4(G.b, LOD, 0, 0)).b;
	res.a = tex2Dlod(_invTex, float4(G.a, LOD, 0, 0)).a;       	
				
	return res;
}

float4 StochasticForColor(float2 uv)
{
	float4 res = Stochastic(uv);
	res.rgb = _ColorSpaceOrigin.xyz + _ColorSpaceVector1.xyz * res.r + _ColorSpaceVector2.xyz * res.g + _ColorSpaceVector3.xyz * res.b;
	return res;
}

float4 StochasticForNormal(float2 uv)
{
	float4 res = StochasticForColor(uv);
	res.rgb = UnpackNormalmapRGorAG(res);  
	return res;
}

#endif // _STOCHASTIC_