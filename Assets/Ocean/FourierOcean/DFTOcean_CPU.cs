using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DFTOcean_CPU : MonoBehaviour
{
    const int GRID = 32;
    const int GRID_MINUS_ONE = GRID - 1;

    const float GRAVITY = 9.8f;
    Vector2 Wind = new Vector2(11f,8f);
    const float Amp = 0.0009765625f;// 1 / (GRID^2)

    List<Vector3> mVertices;
    List<Vector3> mNormal;
    List<int> mIndices;

    List<Vector4> mH0;
    List<float> mOmega;

    Mesh mesh;

    public MeshFilter mf;

    // Start is called before the first frame update
    void Start()
    {
        InitMeshData();

        InitPhillips();

        mf.sharedMesh = mesh;

        UpdateMesh(1f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }

    void UpdateMesh(float _delta)
    {
        int idx = 0;

        for (int y = 0; y < GRID; ++y)
        {
            for (int x = 0; x < GRID; ++x)
            {
                Vector2 xz = new Vector2(x,y);
                Vector2 h, d;
                Vector3 n;

                DFT(xz, _delta, out h, out d, out n);

                float lambda = 0f;// -1.0f;

                mVertices[idx] = new Vector3(x + lambda * d.x, h.x, y + lambda * d.y);
                mNormal[idx] = n;

                ++idx;
            }
        }

        mesh.Clear();
        mesh.SetVertices(mVertices);
        mesh.SetNormals(mNormal);
        mesh.SetTriangles(mIndices, 0);
        //mesh.RecalculateTangents();

        mesh.UploadMeshData(false);
    }

    void InitMeshData()
    {
        mesh = new Mesh();
        mesh.MarkDynamic();

        mVertices = new List<Vector3>(GRID * GRID);
        mNormal = new List<Vector3>(GRID * GRID);
        mIndices = new List<int>(GRID * GRID * 6);

        for (int y = 0; y < GRID; ++y)
        {
            for (int x = 0; x < GRID; ++x)
            {
                mVertices.Add(new Vector3(x, 0f, y));
                mNormal.Add(Vector3.up);
            }
        }

        for (int y = 0; y < GRID_MINUS_ONE; ++y)
        {
            for (int x = 0; x < GRID_MINUS_ONE; ++x)
            {
                mIndices.Add(x + y * GRID);
                mIndices.Add(x + (y + 1) * GRID);
                mIndices.Add(x + 1 + y * GRID);
                mIndices.Add(x + 1 + (y + 1) * GRID);
                mIndices.Add(x + 1 + y * GRID);
                mIndices.Add(x + (y + 1) * GRID);
            }
        }
    }

    #region Phillips
    void InitPhillips()
    {
        mH0 = new List<Vector4>(GRID * GRID);
        mOmega = new List<float>(GRID * GRID);

        for (int m = 0; m < GRID; ++m)
        {
            for (int n = 0; n < GRID; ++n)
            {
                Vector2 hTilde0 = H_Tilde0(n, m);
                Vector2 hTilde0_conj = H_Tilde0(-n, -m);
                hTilde0_conj.y *= -1f;

                Vector4 h0 = Vector4.zero;
                h0.x = hTilde0.x;
                h0.y = hTilde0.y;
                h0.z = hTilde0_conj.x;
                h0.w = hTilde0_conj.y;

                mH0.Add(h0);

                float dispersion = Dispersion(n, m);
                mOmega.Add(dispersion);
            }
        }
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
        Vector2 k = new Vector2(Mathf.PI * (2 * _n_prime - GRID) / GRID, Mathf.PI * (2 * _m_prime - GRID) / GRID);
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

    float Dispersion(int _n_prime, int _m_prime)
    {
        // w(k) = (gk)^0.5
        float w_0 = 2.0f * Mathf.PI / 200.0f;
        float kx = Mathf.PI * (2 * _n_prime - GRID) / GRID;
        float kz = Mathf.PI * (2 * _m_prime - GRID) / GRID;

        return Mathf.Floor(Mathf.Sqrt(GRAVITY * Mathf.Sqrt(kx * kx + kz * kz)) / w_0) * w_0;
        //return Mathf.Sqrt(GRAVITY * Mathf.Sqrt(kx * kx + kz * kz));
    }
    #endregion

    #region DFT
    Vector2 hTilde(float t, int n_prime, int m_prime)
    {
        int idx = n_prime + m_prime * GRID;
        Vector4 h0 = mH0[idx];
        float omega_t = mOmega[idx] * t;

        float cos_ = Mathf.Cos(omega_t);
        float sin_ = Mathf.Sin(omega_t);

        //(h.x + i*h.y)*(cos + i*sin) + (h_conj.x + i*h_conj.y)*(cos - i*sin)
        //
        // = (h.x*cos + i*h.x*sin + i*h.y*cos - h.y*sin) + (h_conj.x*cos - i*h_conj.x*sin + i*h_conj.y*cos + h_conj.y*sin)
        // = ((h.x + h_conj.x) * cos - (h.y - h_conj.y) * sin) + i * ((h.x - h_conj.x) * sin + (h.y + h_conj.y) * cos)

        Vector2 res = new Vector2(0, 0);
        res.x = (h0.x + h0.z) * cos_ - (h0.y - h0.w) * sin_;
        res.y = (h0.x - h0.z) * sin_ + (h0.y + h0.w) * cos_;

        return res;
    }

    void DFT(Vector2 xz, float t, out Vector2 h, out Vector2 d, out Vector3 n)
    {
        h = new Vector2(0, 0);
        d = new Vector2(0, 0);
        n = new Vector3(0, 0, 0);

        float kx, kz, k_len, k_dot_xz;
        Vector2 k, hTilde_t, hTilde_c, c;

        for (int m_prime = 0; m_prime < GRID; ++m_prime)
        {
            kz = (float)(2.0 * Mathf.PI * (m_prime - GRID / 2.0) / GRID);

            for (int n_prime = 0; n_prime < GRID; ++n_prime)
            {
                kx = (float)(2.0 * Mathf.PI * (n_prime - GRID / 2.0) / GRID);

                k = new Vector2(kx, kz);
                k_len = k.magnitude;
                k_dot_xz = Vector2.Dot(k, xz);

                c = new Vector2(Mathf.Cos(k_dot_xz), Mathf.Sin(k_dot_xz));

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
                n += new Vector3(-kx * hTilde_c.y, 0.0f, -kz * hTilde_c.y);
                if (k_len < 0.000001) continue;

                // -i*(k/k_len)*h(k,t)*exp(idot(k,x))
                // = -i*(k/k_len)*hTilde_c
                // = -i*(kx/k_len, kz/k_len)*(hTilde_c.x + i*hTilde_c.y)
                // = -i*(kx/k_len*hTilde_c.x + i*kx/k_len*hTilde_c.y, kz/k_len*hTilde_c.x, i*kz/k_len*hTilde_c.y)
                // = (kx/k_len*hTilde_c.y - i*kx/k_len*hTilde_c.x, kz/k_len*hTilde_c.y - i*kz/k_len*hTilde_c.x)
                d += new Vector2(kx / k_len * hTilde_c.y, kz / k_len * hTilde_c.y);
            }
        }

        n = (new Vector3(0.0f, 1.0f, 0.0f) - n).normalized;
    }
    #endregion
}
