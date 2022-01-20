using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OpenSimplexNoise
{
    long seed;

    GraphicsBuffer perm;
    GraphicsBuffer permGrad2;

    QuadTreeSetting Setting;

    public OpenSimplexNoise(QuadTreeSetting setting)
    {
        Setting = setting;
    }

    public void Initialize()
    {
        if (Setting != null)
        {
            InitGraphicsBuffer();

            Generate(Setting.NoiseSeed);
        }
    }

    public void Release()
    {
        ReleaseGraphicsBuffer();
    }

    bool isValid()
    {
        return Setting != null &&
                perm != null &&
                permGrad2 != null;
    }

    void InitGraphicsBuffer()
    {
        ReleaseGraphicsBuffer();

        int size = Setting.SimplexNoiseSize;

        perm = new GraphicsBuffer(GraphicsBuffer.Target.Structured, size, sizeof(int));
        permGrad2 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, size, sizeof(float) * 2);
    }

    void ReleaseGraphicsBuffer()
    {
        if (perm != null)
        {
            perm.Release();
            perm.Dispose();
            perm = null;
        }

        if (permGrad2 != null)
        {
            permGrad2.Release();
            permGrad2.Dispose();
            permGrad2 = null;
        }
    }

    void CreateGradient(out Vector2[] _gradient)
    {
        Vector2[] grad2 = {
            new Vector2( 0.130526192220052f,  0.99144486137381f),
            new Vector2( 0.38268343236509f,   0.923879532511287f),
            new Vector2( 0.608761429008721f,  0.793353340291235f),
            new Vector2( 0.793353340291235f,  0.608761429008721f),
            new Vector2( 0.923879532511287f,  0.38268343236509f),
            new Vector2( 0.99144486137381f,   0.130526192220051f),
            new Vector2( 0.99144486137381f,  -0.130526192220051f),
            new Vector2( 0.923879532511287f, -0.38268343236509f),
            new Vector2( 0.793353340291235f, -0.60876142900872f),
            new Vector2( 0.608761429008721f, -0.793353340291235f),
            new Vector2( 0.38268343236509f,  -0.923879532511287f),
            new Vector2( 0.130526192220052f, -0.99144486137381f),
            new Vector2(-0.130526192220052f, -0.99144486137381f),
            new Vector2(-0.38268343236509f,  -0.923879532511287f),
            new Vector2(-0.608761429008721f, -0.793353340291235f),
            new Vector2(-0.793353340291235f, -0.608761429008721f),
            new Vector2(-0.923879532511287f, -0.38268343236509f),
            new Vector2(-0.99144486137381f,  -0.130526192220052f),
            new Vector2(-0.99144486137381f,   0.130526192220051f),
            new Vector2(-0.923879532511287f,  0.38268343236509f),
            new Vector2(-0.793353340291235f,  0.608761429008721f),
            new Vector2(-0.608761429008721f,  0.793353340291235f),
            new Vector2(-0.38268343236509f,   0.923879532511287f),
            new Vector2(-0.130526192220052f,  0.99144486137381f)
        };

        const float N2 = 7.69084574549313f;

        int size = Setting.SimplexNoiseSize;

        _gradient = new Vector2[size];

        for (int i = 0; i < grad2.Length; ++i)
        {
            grad2[i].x /= N2;
            grad2[i].y /= N2;
        }

        for (int i = 0; i < size; i++)
        {
            _gradient[i] = grad2[i % grad2.Length];
        }
    }

    void Generate(long _seed)
    {
        seed = _seed;

        int size = Setting.SimplexNoiseSize;

        int[] source = new int[size];
        for (int i = 0; i < size; ++i)
            source[i] = i;

        Vector2[] GRADIENTS_2D;
        CreateGradient(out GRADIENTS_2D);

        int[] rawPerm = new int[size];
        Vector2[] rawGrad2 = new Vector2[size];

        for(int i = size - 1; 0 <= i; --i)
        {
            _seed = _seed * 6364136223846793005L + 1442695040888963407L;
            int r = (int)((seed + 31) % (i + 1));
            if (r < 0)
            {
                r += (i + 1);
            }
            rawPerm[i] = source[r];
            rawGrad2[i] = GRADIENTS_2D[rawPerm[i]];
            source[r] = source[i];
        }

        perm.SetData(rawPerm);
        permGrad2.SetData(rawGrad2);
    }

    public bool Apply(Material _mat)
    {
        if (isValid() == false) return false;

        if (seed != Setting.NoiseSeed)
        {
            Generate(Setting.NoiseSeed);
        }

        _mat.SetInt(Shader.PropertyToID("PSIZE"), Setting.SimplexNoiseSize);
        _mat.SetInt(Shader.PropertyToID("PMASK"), Setting.SimplexNoiseMask);

        _mat.SetBuffer(Shader.PropertyToID("perm"), perm);
        _mat.SetBuffer(Shader.PropertyToID("permGrad2"), permGrad2);

        return true;
    }
}
