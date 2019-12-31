#ifndef __Diffuse_Term_
#define __Diffuse_Term_
/// https://zhuanlan.zhihu.com/p/35878843
/// https://zhuanlan.zhihu.com/p/34473064
/// Diffuse Function

#define BRDF_DIFFUSE_TERM 0

float3 BRDF(float3 _color)
{
#if BRDF_DIFFUSE_TERM
	return _color / UNITY_PI;
#else
	return _color;
#endif	
}

float3 Lambert(float3 _diffuse)
{
	return BRDF(_diffuse);
}

float3 Burley(float3 _diffuse, float _roughness, float _NdotV, float _NdotL, float _VdotH)
{
	float fresnelDiffuse90 = 0.5 + 2 * _VdotH * _VdotH * _roughness;
	float FdotV = 1 + (fresnelDiffuse90 - 1) * pow(1 - _NdotV, 5);
	float FdotL = 1 + (fresnelDiffuse90 - 1) * pow(1 - _NdotL, 5);

	return BRDF(_diffuse * FdotV * FdotL);
}

float3 OrenNayar(float3 _diffuse, float _roughness, float _NdotV, float _NdotL, float _VdotH)
{
	float a = _roughness;
	float a2 = a * a;
	float s = a2; // 1.29 + 0.5 * a2;
	float s2 = s * s;
	float VdotL = 2 * _VdotH * _VdotH - 1;
	float Cosri = VdotL - _NdotV * _NdotL;
	float C1 = 1 - 0.5 * s2 / (s2 + 0.33);
	float C2 = 0.45 * s2 / (s2 + 0.09) * Cosri * (Cosri >= 0 ? 1 / max(0.001, max(_NdotV, _NdotL)) : 1);

	return BRDF(_diffuse) * (C1 + C2) * (1 + _roughness * 0.5);
}

float3 Gotanda(float3 _diffuse, float _roughness, float _NdotV, float _NdotL, float _VdotH)
{
	float a = _roughness * _roughness;
	float a2 = a * a;
	float F0 = 0.04;
	float VdotL = 2 * _VdotH * _VdotH - 1;
	float Cosri = VdotL - _NdotV * _NdotL;

	float a2_13 = a2 + 1.36053;
	float Fr = ( 1 - ( 0.542026*a2 + 0.303573*a ) / a2_13 ) * ( 1 - pow( 1 - _NdotV, 5 - 4*a2 ) / a2_13 ) * ( ( -0.733996*a2*a + 1.50912*a2 - 1.16402*a ) * pow( 1 - _NdotV, 1 + 1/(39*a2*a2+1) ) + 1 );
	//float Fr = ( 1 - 0.36 * a ) * ( 1 - pow( 1 - _NdotV, 5 - 4*a2 ) / a2_13 ) * ( -2.5 * _roughness * ( 1 - _NdotV ) + 1 );
	float Lm = ( max( 1 - 2*a, 0 ) * ( 1 - Pow5( 1 - _NdotL ) ) + min( 2*a, 1 ) ) * ( 1 - 0.5*a * (_NdotL - 1) ) * _NdotL;
	float Vd = ( a2 / ( (a2 + 0.09) * (1.31072 + 0.995584 * _NdotV) ) ) * ( 1 - pow( 1 - _NdotL, ( 1 - 0.3726732 * _NdotV * _NdotV ) / ( 0.188566 + 0.38841 * _NdotV ) ) );
	float Bp = Cosri < 0 ? 1.4 * _NdotV * _NdotL * Cosri : Cosri;
	float Lr = (21.0 / 20.0) * (1 - F0) * ( Fr * Lm + Vd + Bp );
	return BRDF(_diffuse) * Lr;

}

#endif // __Diffuse_Term_
