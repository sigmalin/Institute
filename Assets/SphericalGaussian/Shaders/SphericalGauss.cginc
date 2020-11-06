#ifndef _SPHERICAL_GAUSS_H
#define _SPHERICAL_GAUSS_H

#ifndef PI
#define PI 3.1415926536898
#endif

struct SG
{
	float3 Amplitude;
	float3 Axis;
	float Sharpness;
};

float3 EvaluateSG(in SG _sg, in float3 _direction)
{
	float cosAngle = dot(_direction, _sg.Axis);
	return _sg.Amplitude * exp(_sg.Sharpness * (cosAngle - 1));
}

SG SGProduct(in SG _x, in SG _y)
{
	float3 um = (_x.Sharpness * _x.Axis + _y.Sharpness * _y.Axis) /
	            (_x.Sharpness + _y.Sharpness);
	float umLength = length(um);
	float lm = _x.Sharpness + _y.Sharpness;

	SG res;
	res.Axis = um * (1.0 / umLength);
	res.Sharpness = lm * umLength;
	res.Amplitude = _x.Amplitude * _y.Amplitude * exp(lm * (umLength - 1.0));

	return res;
}

float3 SGIntegral(in SG _sg)
{
	float expTerm = 1.0 - exp(-2.0 * _sg.Sharpness);
	return 2 * PI * (_sg.Amplitude / _sg.Sharpness) * expTerm;
}

float3 ApproximateSGIntegral(in SG _sg)
{
	// when 3 <= _sg.Sharpness, expTerm-> 1
	return 2 * PI * (_sg.Amplitude / _sg.Sharpness);
}

float3 SGInnerProduct(in SG _x, in SG _y)
{
	float umLength = length(_x.Sharpness * _x.Axis + _y.Sharpness * _y.Axis);
	float3 expo = exp(umLength - _x.Sharpness - _y.Sharpness) * _x.Amplitude * _y.Amplitude;
	float other = 1.0 - exp(-2.0 * umLength);
	return (2.0 * PI * expo * other) / umLength;
}

// Computes an SG sharpness value such that all values within theta radians of the SG axis have a value greater than epsilon
float SGSharpnessFromThreshold(float _amp, float _epsilon, float _cosTheta)
{
	return (log(_epsilon) - log(_amp)) / (_cosTheta - 1.0f);
}

SG CosineLobeSG(in float3 _dir)
{
	// Jiaping Wang, All-Frequency Rendering of Dynamic, Spatially-Varing Reflectance
	SG cosineLobe;
	cosineLobe.Axis = _dir;
	cosineLobe.Sharpness = 2.133;
	cosineLobe.Amplitude = 1.17;
	return cosineLobe;
}


SG PointLightSG(in float3 _direction, in float _radius, in float3 _intensity, in float _dist)
{
	//"All-Frequency Rendering of Dynamic, Spatially-Varying Reflectance¡¨ Jiaping Wang, Peiran Ren, Minmin Gong, John Snyder, Baining Guo. SIGGRAPH ASIA 2009
	SG lightingLobe;

	float r2 = _radius * _radius;
	float d2 = _dist * _dist;

	const float lne = -2.230258509299; // = ln(0.1)
	lightingLobe.Axis = _direction;
	lightingLobe.Sharpness = (-lne * d2) / r2;
	lightingLobe.Amplitude = _intensity / max(d2, 0.001);

	return lightingLobe;
}

SG DirectionalLightSG(in float3 _direction, in float _radius, in float3 _intensity)
{
	//"All-Frequency Rendering of Dynamic, Spatially-Varying Reflectance¡¨ Jiaping Wang, Peiran Ren, Minmin Gong, John Snyder, Baining Guo. SIGGRAPH ASIA 2009
	SG lightingLobe;

	float r2 = _radius * _radius;

	const float lne = -2.230258509299; // = ln(0.1)
	lightingLobe.Axis = _direction;
	lightingLobe.Sharpness = -lne / r2;
	lightingLobe.Amplitude = _intensity;

	return lightingLobe;
}

// Returns an SG with a particular sharpness that integrates to 1
SG NormalizedSG(in float3 _axis, in float _sharpness)
{
    SG sg;
    sg.Axis = _axis;
    sg.Sharpness = _sharpness;
    sg.Amplitude = rcp(ApproximateSGIntegral(sg));

    return sg;
}

#endif // _SPHERICAL_GAUSS_H