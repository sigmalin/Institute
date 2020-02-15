#ifndef __PERLIN_WORLEY_NOISE_
#define __PERLIN_WORLEY_NOISE_
//https://www.shadertoy.com/view/4ddyzj

// 2D voronoi noise.
float r(float n)
{
 	return frac(cos(n*89.42)*343.42);
}

float2 r(float2 n)
{
 	return float2(r(n.x*23.62-300.0+n.y*34.35),r(n.x*45.13+256.0+n.y*38.89)); 
}

float voronoi2D(in float2 n)
{
    float dis = 2.0;
    for (int y= -1; y <= 1; y++) 
    {
        for (int x= -1; x <= 1; x++) 
        {
            // Neighbor place in the grid
            float2 p = floor(n) + float2(x,y);

            float d = length(r(p) + float2(x, y) - frac(n));
            if (dis > d)
            {
             	dis = d;   
            }
        }
    }
    
    return 1.0 - dis;
}


//#define MOD3 float3(.1031,.11369,.13787)

float3 hash( float3 p )
{
	p = float3( dot(p,float3(127.1,311.7, 74.7)),
			  dot(p,float3(269.5,183.3,246.1)),
			  dot(p,float3(113.5,271.9,124.6)));

	return -1.0 + 2.0*frac(sin(p)*43758.5453123);
}

// 3D Gradient noise by iq.
float noise3D( in float3 p )
{
    float3 i = floor( p );
    float3 f = frac( p );
	
	float3 u = f*f*(3.0-2.0*f);

    return lerp( lerp( lerp( dot( hash( i + float3(0.0,0.0,0.0) ), f - float3(0.0,0.0,0.0) ), 
                          dot( hash( i + float3(1.0,0.0,0.0) ), f - float3(1.0,0.0,0.0) ), u.x),
                     lerp( dot( hash( i + float3(0.0,1.0,0.0) ), f - float3(0.0,1.0,0.0) ), 
                          dot( hash( i + float3(1.0,1.0,0.0) ), f - float3(1.0,1.0,0.0) ), u.x), u.y),
                lerp( lerp( dot( hash( i + float3(0.0,0.0,1.0) ), f - float3(0.0,0.0,1.0) ), 
                          dot( hash( i + float3(1.0,0.0,1.0) ), f - float3(1.0,0.0,1.0) ), u.x),
                     lerp( dot( hash( i + float3(0.0,1.0,1.0) ), f - float3(0.0,1.0,1.0) ), 
                          dot( hash( i + float3(1.0,1.0,1.0) ), f - float3(1.0,1.0,1.0) ), u.x), u.y), u.z );
}

float PerlinWorley(in float3 pos)
{
	float w = (1.0 + noise3D(pos)) + 
              ((1.0 + voronoi2D(pos)) + 
              (0.5 * voronoi2D(pos * 2.)) + 
              (0.25 * voronoi2D(pos * 4.)));

	return w * 0.25;
}

#endif // __PERLIN_WORLEY_NOISE_
