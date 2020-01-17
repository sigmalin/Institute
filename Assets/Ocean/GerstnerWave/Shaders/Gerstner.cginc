#ifndef __GERSTNER_
#define __GERSTNER_
/// http://karanokan.info/2019/05/25/post-3026/

float3 GerstnerWave(float2 amp, float freq, float steep, float speed, float2 dir, float2 v, float time)
{
	float3 p;
	float2 d = normalize(dir.xy);
	float q = steep;
	float f = (dot(d, v) * freq + time * speed);
	p.xz = q * amp * d.xy * cos(f);
	p.y = amp * sin(f);

	return p;
}

float3 CalcBinormal(float2 amp, float freq, float steep, float speed, float2 dir, float2 v, float time)
{
	float2 d = normalize(dir.xy);
	float q = steep;
	float wa = freq * amp;

	float pf = dot(d, v) * freq + time * speed;
	float sp = sin(pf);
	float cp = cos(pf);

	float3 binormal;
	binormal.xz = -q * d.x * d.xy * wa * sp;
	binormal.y = d.x * wa * cp;
				
	return binormal;
}

float3 CalcTangent(float2 amp, float freq, float steep, float speed, float2 dir, float2 v, float time)
{
	float2 d = normalize(dir.xy);
	float q = steep;
	float wa = freq * amp;

	float pf = dot(d, v) * freq + time * speed;
	float sp = sin(pf);
	float cp = cos(pf);
				
	float3 tangent;
	tangent.xz = -q * d.xy * d.y * wa * sp;
	tangent.y = d.y * wa * cp;

	return tangent;
}

float3 CalcNormal(float2 amp, float freq, float steep, float speed, float2 dir, float3 v, float time)
{
	float3 n;
	float3 d = normalize(float3(dir.x, 0.0, dir.y));
	float q = steep;
	float f = (dot(d, v) * freq + time * speed);
	n.xz = amp * freq * d.xz * cos(f);
	n.y = q * freq * amp * sin(f);

	return n;
}

#endif // __GERSTNER_
