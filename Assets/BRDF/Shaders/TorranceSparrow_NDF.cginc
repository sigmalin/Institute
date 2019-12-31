#ifndef __TorranceSparrow_NDF_
#define __TorranceSparrow_NDF_
/// http://graphicrants.blogspot.com/2013/08/specular-brdf-reference.html
/// https://www.jordanstevenstechart.com/physically-based-rendering
/// Normal Distribution Function

float GGX_Trowbridge_Reitz(float NdotH, float roughness)
{
	float a      = roughness;			
	float a2     = a*a;
	float NdotH2 = NdotH*NdotH;

	float nom    = a2;
	float denom  = (NdotH2 * (a2 - 1.0) + 1.0);
	denom        = UNITY_PI * denom * denom;

	return nom / denom;
}

float GGX(float NdotH, float roughness)
{
	float a      = roughness;			
	float a2     = a*a;
	float NdotH2 = NdotH*NdotH;
	float TanNdotH2 = (1-NdotH2)/NdotH2;

	float nom    = a / (NdotH2 * (a2 + TanNdotH2)) ;
	float denom  = UNITY_PI;

	return (nom * nom) / denom;
}

float Blinn_Phong(float NdotH, float roughness)
{
	float a      = roughness;			
	float a2     = a*a;
	float n      = 2 / a2 - 2;

	float nom    = (n + 2) * pow(NdotH, n);
	float denom  = 2 * UNITY_PI;

	return nom / denom;
}

float Phong(float NdotH, float specPower, float specGloss)
{
	float nom    = pow(NdotH, specGloss) * specPower * (2+specPower);
	float denom  = UNITY_PI * 2;

	return nom / denom;
}

float Beckmann(float NdotH, float roughness)
{
	float a      = roughness;			
	float a2     = a*a;
	float NdotH2 = NdotH*NdotH;

	return max(0.000001, (1.0 / (UNITY_PI * a2 * NdotH2 * NdotH2)) * exp((NdotH2-1)/(a2*NdotH2)));
}

float Gaussian(float NdotH, float roughness)
{
	float a      = roughness;			
	float a2     = a*a;
	float thetaH = acos(NdotH);

	return exp(-thetaH*thetaH/a2);
}

float Trowbridge_Reitz_Anisotropic(float NdotH, float glossiness, float anisotropic, float HdotX, float HdotY)
{
	float OneMinusGlossiness  = (1 - glossiness);
	float OneMinusGlossiness2  = OneMinusGlossiness * OneMinusGlossiness;
	float NdotH2 = NdotH*NdotH;
	float aspect = sqrt(1.0h - anisotropic * 0.9h);

    float ax = max(0.001, OneMinusGlossiness2/aspect) * 5;
    float ay = max(0.001, OneMinusGlossiness2*aspect) * 5;

	float HdotX_aX  = (HdotX * HdotX) / (ax * ax);
	float HdotY_aY  = (HdotY * HdotY) / (ay * ay);

	float denom  = HdotX_aX + HdotY_aY + NdotH2;
	denom = UNITY_PI * ax * ay * (denom * denom);

	return 1 / denom;
}

float Ward_Anisotropic(float NdotH, float NdotV, float NdotL, float glossiness, float anisotropic, float HdotX, float HdotY)
{
	float OneMinusGlossiness  = (1 - glossiness);
	float OneMinusGlossiness2  = OneMinusGlossiness * OneMinusGlossiness;
	float NdotH2 = NdotH*NdotH;
	float NdotLV = NdotL*NdotV;
	float aspect = sqrt(1.0h - anisotropic * 0.9h);

	float ax = max(0.001, OneMinusGlossiness2/aspect) * 5;
    float ay = max(0.001, OneMinusGlossiness2*aspect) * 5;

	float HdotX_aX  = (HdotX * HdotX) / (ax * ax);
	float HdotY_aY  = (HdotY * HdotY) / (ay * ay);

	float exponent = -(HdotX_aX + HdotY_aY) / NdotH2;
	float denom  = 4 * UNITY_PI * ax * ay * sqrt(NdotLV);

	return (1 / denom) * exp(exponent);
}

#endif // __TorranceSparrow_NDF_