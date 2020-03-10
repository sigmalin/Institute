#ifndef __ImportanceSample_
#define __ImportanceSample_

#ifndef PI
#define PI 3.1415926536898
#endif

float3 ImportanceSampleGGX(float2 Xi, float roughness, float3 N) 
{
    float alphaRoughness = roughness * roughness;

	float Phi = 2.0 * PI * Xi.x;
	float CosTheta = sqrt((1.0 - Xi.y) / (1.0 + (alphaRoughness*alphaRoughness - 1.0) * Xi.y));
	float SinTheta = sqrt(1.0 - CosTheta * CosTheta);

	float3 H;
	H.x = SinTheta * cos(Phi);
	H.y = SinTheta * sin(Phi);
	H.z = CosTheta;

	float3 UpVector = abs(N.z) < 0.999 ? float3(0.0,0.0,1.0) : float3(1.0,0.0,0.0);
	float3 TangentX = normalize(cross(UpVector, N));
	float3 TangentY = cross(N, TangentX);

    return TangentX * H.x + TangentY * H.y + N * H.z;
}

#endif // __ImportanceSample_
