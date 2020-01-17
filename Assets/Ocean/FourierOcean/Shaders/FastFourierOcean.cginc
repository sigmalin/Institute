#ifndef __FAST_FOURIER_OCEAN_
#define __FAST_FOURIER_OCEAN_
//https://www.keithlantz.net/2011/11/ocean-simulation-part-two-using-the-fast-fourier-transform/

sampler2D _HTidle0;
sampler2D _Dispersion;

sampler2D _Height;
sampler2D _Slope;
sampler2D _Displacement;

int _FourierSize;

float2 hTilde(float t, float2 uv2)
{
	float size = _FourierSize - 1;
	float4 uv4 = float4(uv2, 0, 0);
	float4 h0 = tex2Dlod(_HTidle0, uv4);
	float omega_t = tex2Dlod(_Dispersion, uv4).r * t;

	float cos_ = cos(omega_t);
	float sin_ = sin(omega_t);

	//(h.x + i*h.y)*(cos + i*sin) + (h_conj.x + i*h_conj.y)*(cos - i*sin)
	//
	// = (h.x*cos + i*h.x*sin + i*h.y*cos - h.y*sin) + (h_conj.x*cos - i*h_conj.x*sin + i*h_conj.y*cos + h_conj.y*sin)
	// = ((h.x + h_conj.x) * cos - (h.y - h_conj.y) * sin) + i * ((h.x - h_conj.x) * sin + (h.y + h_conj.y) * cos)

	float2 res = float2(0,0);
	res.x = (h0.x + h0.z) * cos_ - (h0.y - h0.w) * sin_;
	res.y = (h0.x - h0.z) * sin_ + (h0.y + h0.w) * cos_;

	return res;
}

void FFT_Spectrum(float2 uv, float t, out float2 height, out float4 slope, out float4 displacement)
{
	height = float2(0,0);
	slope = float4(0,0,0,0);
	displacement = float4(0,0,0,0);

	float n_prime = uv.x * (_FourierSize-1);
	float m_prime = uv.y * (_FourierSize-1);

	float2 k;
	float kx, kz, k_len;
	
	kx = UNITY_PI * (2.0 * n_prime - _FourierSize) / _FourierSize;
	kz = UNITY_PI * (2.0 * m_prime - _FourierSize) / _FourierSize;	
	k = float2(kx, kz);
	k_len = length(k);

	// height = hTilde
	height = hTilde(t, uv);

	//
	
	float2 xz = float2(n_prime, m_prime);
	float k_dot_xz = dot(k, xz);
	float2 c = float2(cos(k_dot_xz), sin(k_dot_xz));
	height.x = height.x * c.x - height.y * c.y;
	height.y = height.x * c.y + height.y * c.x;
	
	// slope = i*k*hTilde
	// = i*(kx,kz)*(height.x + i*height.y)
	// = i*(kx*height.x + i*kx*height.y,  kz*height.x + i*kz*height.y)
	// = (-kx*height.y, i*kx*height.x, -kz*height.y, i*kz*height.x)
	slope.x = -kx*height.y;
	slope.y = kx*height.x;
	slope.z = -kz*height.y;
	slope.w = kz*height.x;

	// displacement = -i*(k/k_len)*hTilde = -i*(kx_L, kz_L)*hTilde
	// = -i*(kx_L,kx_L)*(height.x + i*height.y)
	// = -i*(kx_L*height.x + i*kx_L*height.y,  kz_L*height.x + i*kz_L*height.y)
	// = (kx_L*height.y, -i*kx_L*height.x, kz_L*height.y, -i*kz_L*height.x)
	if (k_len < 0.000001)
	{
		displacement.x = 0;
		displacement.y = 0;
		displacement.z = 0;
		displacement.w = 0;
	}
	else
	{
		displacement.x = kx*height.y/k_len;
		displacement.y = -kx*height.x/k_len;
		displacement.z = kz*height.y/k_len;
		displacement.w = -kz*height.x/k_len;
	}
}

void FFT_Output(float2 uv, float sign, out float h, out float2 d, out float3 n)
{
	float4 uv4 = float4(uv, 0, 0);

	int n_prime = uv.x * (_FourierSize-1);
	int m_prime = uv.y * (_FourierSize-1);	

	h = tex2Dlod(_Height, uv4).x * sign;
	d = tex2Dlod(_Displacement, uv4).xz * sign;

	float2 slope = tex2Dlod(_Slope, uv4).xz;
	n = float3(-slope.x * sign, 1.0f, -slope.y * sign);
	n = normalize(n);
}

#endif // __FAST_FOURIER_OCEAN_
