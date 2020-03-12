#ifndef __TilingPerlinNoise_
#define __TilingPerlinNoise_

StructuredBuffer<int> Perm;

int inc(float x, float period)
{
	x += 1.0;
	x = modulo(x, period);
	return FloorToInt(x);
}

float Fade(float t)
{
	return t * t * t * (t * (t * 6 - 15) + 10);
}

float Grad(int hash, float x, float y)
{
	return ((hash & 1) == 0 ? x : -x) + ((hash & 2) == 0 ? y : -y);
}

float Grad(int hash, float x, float y, float z)
{
	int h = hash & 15;
	float u = h < 8 ? x : y;
	float v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
	return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
}

float TiliedPerlinNoise2D(float2 st, float period)
{
	st *= period;
        
	st.x = modulo(st.x, period);
	st.y = modulo(st.y, period);

	float x = st.x;
	float y = st.y;

	int X = FloorToInt(x) & 0xff;
	int Y = FloorToInt(y) & 0xff;
	x -= floor(x);
	y -= floor(y);
	float u = Fade(x);
	float v = Fade(y);

	int aaa, aba, baa, bba;
	aaa = Perm[Perm[X] + Y];
	aba = Perm[Perm[X] + inc(Y, period)];
	baa = Perm[Perm[inc(X, period)] + Y];
	bba = Perm[Perm[inc(X, period)] + inc(Y, period)];

	float x1, x2, y1;
	x1 = lerp(
			Grad(aaa, x, y),
			Grad(baa, x - 1, y),
			u
			);
	x2 = lerp(
			Grad(aba, x, y - 1),
			Grad(bba, x - 1, y - 1),
			u
			);

	y1 = lerp(x1, x2, v);
	return (y1 + 1) * 0.5f;
}

float TiliedPerlinNoise3D(float3 st, float period)
{
	st *= period;
	
	float x = modulo(st.x, period);
	float y = modulo(st.y, period);
	float z = modulo(st.z, period);
		   
	int X = FloorToInt(x) & 0xff;
	int Y = FloorToInt(y) & 0xff;
	int Z = FloorToInt(z) & 0xff;
	
	x -= floor(x);
	y -= floor(y);
	z -= floor(z);
	float u = Fade(x);
    float v = Fade(y);
    float w = Fade(z);	

	int aaa, aba, aab, abb, baa, bba, bab, bbb;
	aaa = Perm[Perm[Perm[X] + Y] + Z];
	aba = Perm[Perm[Perm[X] + inc(Y, period)] + Z];
	aab = Perm[Perm[Perm[X] + Y] + inc(Z, period)];
	abb = Perm[Perm[Perm[X] + inc(Y, period)] + inc(Z, period)];
	baa = Perm[Perm[Perm[inc(X, period)] + Y] + Z];
	bba = Perm[Perm[Perm[inc(X, period)] + inc(Y, period)] + Z];
	bab = Perm[Perm[Perm[inc(X, period)] + Y] + inc(Z, period)];
	bbb = Perm[Perm[Perm[inc(X, period)] + inc(Y, period)] + inc(Z, period)];

	float x1, x2, y1, y2;
	x1 = lerp(
			Grad(aaa, x, y, z),
			Grad(baa, x - 1, y, z),
			u
			);
	x2 = lerp(
			Grad(aba, x, y - 1, z),
			Grad(bba, x - 1, y - 1, z),
			u
			);

	y1 = lerp(x1, x2, v);

	x1 = lerp(
			Grad(aab, x, y, z - 1),
			Grad(bab, x - 1, y, z - 1),
			u
			);
	x2 = lerp(
			Grad(abb, x, y - 1, z - 1),
			Grad(bbb, x - 1, y - 1, z - 1),
			u
			);
	y2 = lerp(x1, x2, v);


	return (lerp(y1, y2, w) + 1) * 0.5f;
}

#endif // __TilingPerlinNoise_
