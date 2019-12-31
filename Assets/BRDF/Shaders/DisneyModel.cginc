#ifndef __DISNEY_MODEL_
#define __DISNEY_MODEL_
/// https://qiita.com/mebiusbox2/items/8db00cdcaf263992a5ce#ggx-%E5%88%86%E5%B8%83
/// Disney Model

float D_GTR(float NdotH, float roughness)
{
	float a = roughness * roughness;
	float a2 = a * a;

	float nom    = a2 - 1;
	float denom  = UNITY_PI * log(a2) * (1 + (a2 - 1) * NdotH * NdotH);

	return nom / denom;
}

float D_GTR2_Ansio(float NdotH, float roughness, float anisotropic, float HdotX, float HdotY)
{
	float aspect = sqrt(1.0h - anisotropic * 0.9h);

	float a = roughness * roughness;
	float a2 = a * a;

	float ax = max(0.001, a2/aspect);
    float ay = max(0.001, a2*aspect);

	float HdotX_aX  = (HdotX * HdotX) / (ax * ax);
	float HdotY_aY  = (HdotY * HdotY) / (ay * ay);
	float NdotH2 = NdotH*NdotH;

	float denom  = HdotX_aX + HdotY_aY + NdotH2;
	denom = UNITY_PI * ax * ay * (denom * denom);

	return 1 / denom;
}

float G_GGX(float NdotV, float roughness)
{
	float a = (0.5 + roughness * 0.5);
	float a2 = a * a; 
	float a4 = a2 * a2; 
	float NdotV2 = NdotV * NdotV;

	float denom  = NdotV + sqrt(a4 + NdotV2 - a4*NdotV2);
	return 1 / denom;
}

float G_GGX_Ansio(float NdotV, float roughness, float anisotropic, float VdotX, float VdotY)
{
	float aspect = sqrt(1.0h - anisotropic * 0.9h);

	float a = roughness * roughness;
	float a2 = a * a;

	float ax = max(0.001, a2/aspect);
    float ay = max(0.001, a2*aspect);

	float VdotX_aX  = VdotX * ax;
	float HdotY_aY  = VdotY * ay;
	float NdotV2 = NdotV*NdotV;

	float denom  = NdotV + sqrt(VdotX_aX*VdotX_aX + HdotY_aY*HdotY_aY + NdotV2);

	return 1 / denom;
}

float Schlick(float _value)
{
	float p = clamp(1 - _value, 0, 1);
	float p2 = p * p;
	float p5 = p2 * p2 * p;

	return p5;
}
#endif // __DISNEY_MODEL_
