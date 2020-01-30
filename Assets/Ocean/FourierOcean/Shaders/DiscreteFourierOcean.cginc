#ifndef __DISCRETE_FOURIER_OCEAN_
#define __DISCRETE_FOURIER_OCEAN_
//https://www.keithlantz.net/2011/10/ocean-simulation-part-one-using-the-discrete-fourier-transform/

sampler2D _HTidle0;
sampler2D _Dispersion;

int _FourierSize;

float2 hTilde(float t, int n_prime, int m_prime)
{
	float size = _FourierSize - 1;
	float4 uv = float4(n_prime/size, m_prime/size, 0, 0);
	float4 h0 = tex2Dlod(_HTidle0, uv);
	float omega_t = tex2Dlod(_Dispersion, uv).r * t;

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

void DFT(float2 xz, float t, out float2 h, out float2 d, out float3 n)
{
	h = float2(0,0);
	d = float2(0,0);
	n = float3(0,0,0);

	float kx, kz, k_len, k_dot_xz;
	float2 k, hTilde_t, hTilde_c, c;

	for(int m_prime = 0; m_prime < _FourierSize; ++m_prime)
	{
		kz = 2.0 * UNITY_PI * (m_prime - _FourierSize / 2.0) / _FourierSize;

		for(int n_prime = 0; n_prime < _FourierSize; ++n_prime)
		{
			kx = 2.0 * UNITY_PI * (n_prime - _FourierSize / 2.0) / _FourierSize;

			k = float2(kx, kz);
			k_len = length(k);
			k_dot_xz = dot(k, xz);

			c = float2(cos(k_dot_xz), sin(k_dot_xz)); // c = exp(i*dot(k,x))

			hTilde_t = hTilde(t, n_prime, m_prime);

			// (hTilde_t.x + i * hTilde_t.y) * (c.x + i * c.y)
			// = hTilde_t.x * c.x + i * hTilde_t.x * c.y + i * hTilde_t.y * c.x - hTilde_t.y * c.y
			// = (hTilde_t.x * c.x - hTilde_t.y * c.y) + i * (hTilde_t.x * c.y + hTilde_t.y * c.x)
			hTilde_c.x = hTilde_t.x * c.x - hTilde_t.y * c.y;
			hTilde_c.y = hTilde_t.x * c.y + hTilde_t.y * c.x;

			// hTilde_c = h(k,t)*exp(idot(k,x))						
			h += hTilde_c;

			// i*k*h(k,t)*exp(idot(k,x))
			// = i*k*hTilde_c
			// = i*(kx, kz)*(hTilde_c.x + i*hTilde_c.y)
			// = i*(kx*hTilde_c.x + i*kx*hTilde_c.y, kz*hTilde_c.x, i*kz*hTilde_c.y)
			// = (-kx*hTilde_c.y + i*kx*hTilde_c.x, -kz*hTilde_c.y + i*kz*hTilde_c.x)
			n += float3(-kx * hTilde_c.y, 0.0, -kz * hTilde_c.y);
			if (k_len < 0.000001) continue;

			// -i*(k/k_len)*h(k,t)*exp(idot(k,x))
			// = -i*(k/k_len)*hTilde_c
			// = -i*(kx/k_len, kz/k_len)*(hTilde_c.x + i*hTilde_c.y)
			// = -i*(kx/k_len*hTilde_c.x + i*kx/k_len*hTilde_c.y, kz/k_len*hTilde_c.x, i*kz/k_len*hTilde_c.y)
			// = (kx/k_len*hTilde_c.y - i*kx/k_len*hTilde_c.x, kz/k_len*hTilde_c.y - i*kz/k_len*hTilde_c.x)
			d += float2(kx / k_len * hTilde_c.y, kz / k_len * hTilde_c.y);
		}				
	}

	n = normalize(float3(0.0,1.0,0.0) - n);
}

#endif // __DISCRETE_FOURIER_OCEAN_
