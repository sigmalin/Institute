using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourierOcean : MonoBehaviour
{
    public Material mMatFourierOcean;

    IFourierOceanCore mFourierOceanCore;

    Texture2D mSpectrum0;
    Texture2D mOmega;

    RenderTexture mNormalRT;
    RenderTexture mDisplacementRT;

    int mFourierSize;
    float mRatio;

    int ShaderID_Displacement;
    int ShaderID_Normal;
    int ShaderID_Ratio;

    [Range(4, 7)]
    public int FourierLevel = 6;

    public float Amp;
    public Vector2 Wind;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        mFourierSize = 1 << FourierLevel;
        
        InitPhillips(mFourierSize);

        InitMaterials(mFourierSize);

        InitRenderTextures(mFourierSize);

        InitShaderParameters(mFourierSize);
    }

    // Update is called once per frame
    void Update()
    {
        if (Camera.main == null) return;
        
        Vector3 lookAt;
        LookAtPlane(Camera.main, out lookAt);

        lookAt.x = Mathf.Floor(lookAt.x);
        lookAt.z = Mathf.Floor(lookAt.z);

        mMatFourierOcean.SetTexture(ShaderID_Displacement, mDisplacementRT);
        mMatFourierOcean.SetTexture(ShaderID_Normal, mNormalRT);
        mMatFourierOcean.SetFloat(ShaderID_Ratio, mRatio);

        Draw(lookAt, mMatFourierOcean);
    }

    protected virtual void Draw(Vector3 _lookAt, Material _drawer)
    {

    }

    protected virtual void UpdateDisplacement(RenderTexture _DisplacementRT, float _Ratio)
    {

    }

    protected void FixedUpdate()
    {
        if (mFourierOceanCore != null)
        {
            mFourierOceanCore.Perform(mSpectrum0, mOmega, out mNormalRT, out mDisplacementRT);

            UpdateDisplacement(mDisplacementRT, mRatio);
        }
    }

    void InitPhillips(int _fourierSize)
    {
        PhillipsSpectrum spectrum = new PhillipsSpectrum();
        spectrum.Amp = Amp * 1f / (_fourierSize * _fourierSize);
        spectrum.Wind = Wind;
        spectrum.N = spectrum.M = _fourierSize;
        spectrum.LenN = spectrum.LenM = _fourierSize;

        spectrum.Calculate(out mSpectrum0, out mOmega);
    }

    void InitMaterials(int _fourierSize)
    {
        if (mMatFourierOcean == null)
        {
            mMatFourierOcean = new Material(Shader.Find("FourierOcean/FourierOcean"));
            mMatFourierOcean.hideFlags = HideFlags.HideAndDontSave;
            mMatFourierOcean.enableInstancing = true;
        }     
    }

    void InitRenderTextures(int _fourierSize)
    {
        mNormalRT = new RenderTexture(_fourierSize << 1, _fourierSize << 1, 0, RenderTextureFormat.Default);
        mNormalRT.filterMode = FilterMode.Bilinear;
        mNormalRT.wrapMode = TextureWrapMode.Repeat;

        mDisplacementRT = new RenderTexture(_fourierSize << 1, _fourierSize << 1, 0, FourierTextureFormat.GetFourierRenderTextureFormat());
        mDisplacementRT.filterMode = FilterMode.Bilinear;
        mDisplacementRT.wrapMode = TextureWrapMode.Repeat;
    }

    void InitShaderParameters(int _fourierSize)
    {
        ShaderID_Displacement = Shader.PropertyToID("_Displacement");
        ShaderID_Normal = Shader.PropertyToID("_Normal");
        ShaderID_Ratio = Shader.PropertyToID("_Ratio");
        mRatio = 1f / (_fourierSize << 1);
    }

    void LookAtPlane(Camera _cam, out Vector3 _lookat)
    {
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        _lookat = Vector3.zero;

        if (ray.direction.y != 0f)
        {
            Vector3 dirNorm = ray.direction / ray.direction.y;
            _lookat = ray.origin - dirNorm * ray.origin.y;
        }
        else
        {
            _lookat = ray.origin;
        }
    }

    protected void InitFourierOceanCore(IFourierOceanCore _core)
    {
        mFourierOceanCore = _core;
        mFourierOceanCore.Init(mFourierSize);
    }
    
}
