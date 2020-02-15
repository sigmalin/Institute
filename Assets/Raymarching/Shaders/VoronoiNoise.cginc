#ifndef __VORONOI_NOISE_
#define __VORONOI_NOISE_

float3 hash33w(float3 p3)
{
	p3 = frac(p3 * float3(0.1031f, 0.1030f, 0.0973f));
    p3 += dot(p3, p3.yxz+19.19f);
    return frac((p3.xxy + p3.yxx)*p3.zyx);
}

float worley(in float3 x)
{
    float3 p = floor(x);
    float3 f = frac(x);
    
    float result = 1.0f;
    
    for(int k = -1; k <= 1; ++k)
    {
        for(int j = -1; j <= 1; ++j)
        {
            for(int i = -1; i <= 1; ++i)
            {
                float3 b = float3(float(i), float(j), float(k));
                float3 r = b - f + hash33w(p + b);
                float d = dot(r, r);
                
                result = min(d, result);
            }
        }
    }
    
    return sqrt(result);
}

float worleyFbm(float3 pos)
{
    float final        = 0.0;

	final  = worley(pos)*0.500;  pos *= 2.02;
	final += worley(pos)*0.250;  pos *= 2.03;
	final += worley(pos)*0.125;  pos *= 2.01;

	return final / (0.5+0.25+0.125);
}

#endif // __VORONOI_NOISE_
