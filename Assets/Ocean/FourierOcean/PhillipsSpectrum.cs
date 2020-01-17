//http://www.lonelywaiting.com/FFT-Ocean-Implement/
//https://www.keithlantz.net/2011/10/ocean-simulation-part-one-using-the-discrete-fourier-transform/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhillipsSpectrum
{
    const float GRAVITY = 9.8f;

    public int N { get; set; }
    public int M { get; set; }
    public float LenN { get; set; }
    public float LenM { get; set; }
    public Vector2 Wind { get; set; }
    public float Amp { get; set; }

    public bool Calculate(out Texture2D _h0, out Texture2D _omega)
    {
        _h0 = null;
        _omega = null;

        if (Mathf.IsPowerOfTwo(N) == false || Mathf.IsPowerOfTwo(M) == false)
            return false;

        _h0 = new Texture2D(N, M, FourierTextureFormat.GetFourierTextureFormat(), false, true);
        _h0.filterMode = FilterMode.Point;
        _h0.wrapMode = TextureWrapMode.Repeat;

        _omega = new Texture2D(N, M, FourierTextureFormat.GetFourierTextureFormat(), false, true);
        _omega.filterMode = FilterMode.Point;
        _omega.wrapMode = TextureWrapMode.Repeat;

        Color[] cols = new Color[N * M];

        ///
        for (int m = 0; m < M; ++m)
        {
            for(int n = 0; n < N; ++n)
            {
                Vector2 hTilde0 = H_Tilde0(n, m);
                Vector2 hTilde0_conj = H_Tilde0(-n, -m);
                hTilde0_conj.y *= -1f;

                int idx = n + m * N;
                cols[idx].r = hTilde0.x;
                cols[idx].g = hTilde0.y;
                cols[idx].b = hTilde0_conj.x;
                cols[idx].a = hTilde0_conj.y;
            }
        }

        _h0.SetPixels(cols);
        _h0.Apply();
        ///
        for (int m = 0; m < M; ++m)
        {
            for (int n = 0; n < N; ++n)
            {
                float dispersion = Dispersion(n, m);

                int idx = n + m * N;
                cols[idx].r = dispersion;
                cols[idx].g = dispersion;
                cols[idx].b = dispersion;
                cols[idx].a = dispersion;
            }
        }

        _omega.SetPixels(cols);
        _omega.Apply();
        ///

        return true;
    }

    float Dispersion(int _n_prime, int _m_prime)
    {
        // w(k) = (gk)^0.5
        float w_0 = 2.0f * Mathf.PI / 200.0f;
        float kx = Mathf.PI * (2 * _n_prime - N) / LenN;
        float kz = Mathf.PI * (2 * _m_prime - N) / LenM;

        return Mathf.Floor(Mathf.Sqrt(GRAVITY * Mathf.Sqrt(kx * kx + kz * kz)) / w_0) * w_0;
        //return Mathf.Sqrt(GRAVITY * Mathf.Sqrt(kx * kx + kz * kz));
    }

    Vector2 H_Tilde0(int _n_prime, int _m_prime)
    {
        Vector2 r = GaussianRandomVariable();
        return r * Mathf.Sqrt(Phillips(_n_prime, _m_prime) / 2.0f);
    }

    Vector2 GaussianRandomVariable()
    {
        float x1, x2, w;
        do
        {
            x1 = 2.0f * UnityEngine.Random.value - 1.0f;
            x2 = 2.0f * UnityEngine.Random.value - 1.0f;
            w = x1 * x1 + x2 * x2;
        }
        while (w >= 1.0f);

        w = Mathf.Sqrt((-2.0f * Mathf.Log(w)) / w);
        return new Vector2(x1 * w, x2 * w);
    }

    float Phillips(int _n_prime, int _m_prime)
    {
        Vector2 k = new Vector2(Mathf.PI * (2 * _n_prime - N) / LenN, Mathf.PI * (2 * _m_prime - M) / LenM);
        float k_length = k.magnitude;
        if (k_length < 0.000001f) return 0.0f;

        float k_length2 = k_length * k_length;
        float k_length4 = k_length2 * k_length2;

        k.Normalize();

        float k_dot_w = Vector2.Dot(k, Wind.normalized);
        float k_dot_w2 = k_dot_w * k_dot_w;

        float w_length = Wind.magnitude;
        float L = w_length * w_length / GRAVITY;
        float L2 = L * L;

        float damping = 0.001f;
        float l2 = L2 * damping * damping;

        //return Amp * Mathf.Exp(-1.0f / (k_length2 * L2)) / k_length4 * k_dot_w2;
        return Amp * Mathf.Exp(-1.0f / (k_length2 * L2)) / k_length4 * k_dot_w2 * Mathf.Exp(-k_length2 * l2);
    }
}
