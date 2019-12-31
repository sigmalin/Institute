#ifndef __TorranceSparrow_SGSF_
#define __TorranceSparrow_SGSF_
/// http://graphicrants.blogspot.com/2013/08/specular-brdf-reference.html
/// https://www.jordanstevenstechart.com/physically-based-rendering
/// Smith base Geometric Shadowing Function

float _Walter(float _cosThelta, float k)
{
	float cosThelta2 = _cosThelta * _cosThelta;
	float nom   = 2;
	float denom = 1 + sqrt(1 + k * (1-cosThelta2)/max(0.001,cosThelta2));

	return nom / denom;
}

float Walter(float NdotV, float NdotL, float roughness)
{
	float a2 = roughness * roughness;

	float smithL = _Walter(NdotL, a2);
	float smithV = _Walter(NdotV, a2);

	return smithL * smithV;
}

//

float _SmithBeckman1(float _cosThelta, float k)
{
	float cosThelta2 = _cosThelta * _cosThelta;
	float nom   = _cosThelta;
	float denom = k * sqrt(1 - cosThelta2);

	return nom / denom;
}

float _SmithBeckman2(float _calulation)
{
	float calulation2 = _calulation * _calulation;
	float nom   = (3.535 * _calulation) + (2.181 * calulation2);
	float denom = 1 + (2.276 * _calulation) + (2.577 * calulation2);

	return nom / denom;
}

float SmithBeckman(float NdotV, float NdotL, float roughness)
{
	float a2 = roughness * roughness;

	float calulationL = _SmithBeckman1(NdotL, a2);
	float calulationV = _SmithBeckman1(NdotV, a2);

	float smithL = calulationL < 1.6 ? _SmithBeckman2(calulationL) : 1;
	float smithV = calulationV < 1.6 ? _SmithBeckman2(calulationV) : 1;

	return smithL * smithV;
}

//

float _GeometryGGX(float _cosThelta, float k)
{
	float cosThelta2 = _cosThelta * _cosThelta;
	float nom   = 2 * _cosThelta;
	float denom = _cosThelta + sqrt(k + (1-k) * cosThelta2);

	return nom / denom;

}

float GeometryGGX(float NdotV, float NdotL, float roughness)
{
	float a2 = roughness * roughness;

	float smithL = _GeometryGGX(NdotL, a2);
	float smithV = _GeometryGGX(NdotV, a2);

	return smithL * smithV;
}

//

float _Schlick(float _cosThelta, float k)
{
	float nom   = _cosThelta;
	float denom = _cosThelta * (1.0 - k) + k;

	return nom / denom;
}

float Schlick(float NdotV, float NdotL, float roughness)
{
	float k_direct = (roughness+1) * (roughness+1) / 8;
	//float k_IBL = roughness * roughness / 2;

	float smithL = _Schlick(NdotV, k_direct);
	float smithV = _Schlick(NdotL, k_direct);

	return smithL * smithV;
}

float SchlickBeckman(float NdotV, float NdotL, float roughness)
{
	float k = roughness * roughness * 0017978845602865;

	float smithL = _Schlick(NdotV, k);
	float smithV = _Schlick(NdotL, k);

	return smithL * smithV;
}

float SchlickGGX(float NdotV, float NdotL, float roughness)
{
	float k = roughness * 0.5;

	float smithL = _Schlick(NdotV, k);
	float smithV = _Schlick(NdotL, k);

	return smithL * smithV;
}

#endif // __TorranceSparrow_SGSF_
