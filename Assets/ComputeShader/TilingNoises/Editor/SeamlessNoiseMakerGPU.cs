using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class SeamlessNoiseMakerGPU : EditorWindow
{
    static SeamlessNoiseMakerGPU window;

    int mTexSize = 64;

    enum eNoiseType
    {
        None,
        Perlin,
        Voronoi,
        Cellular,
        PerlinWorley,
        WhiteNoise,
    };

    enum eNoiseDim
    {
        Dimension2,
        Dimension3,
    };

    eNoiseDim mDim = eNoiseDim.Dimension2;

    class NoiseModule
    {
        public eNoiseType eType;
        public bool bFBM;
        public int iPeriod;
        public int iOctaves;
        public float fPersistence;
        public float fScale;
        public float fRandomSeed;

        public NoiseModule()
        {
            eType = eNoiseType.Perlin;
            bFBM = false;
            iPeriod = 4;
            iOctaves = 4;
            fPersistence = 0.5f;
            fScale = 2f;
            fRandomSeed = 114514.1515f;
        }
    }

    NoiseModule[] noises;

    [MenuItem("sigmalin/Institute/Noise/SeamlessNoiseMaker(GPU)")]
    static void OpenSeamlessNoiseMakerGPU()
    {
        window = (SeamlessNoiseMakerGPU)EditorWindow.GetWindow(typeof(SeamlessNoiseMakerGPU));
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnGUI()
    {
        if (window == null)
        {
            GUILayout.Label("Missing window reference!");
            return;
        }

        if (noises == null)
        {
            noises = new NoiseModule[4];
            for (int i = 0; i < 4; ++i)
                noises[i] = new NoiseModule();
        }

        EditorGUILayout.BeginVertical("Box");
        IntPow2Field("Noise Texture Size", ref mTexSize);
        EditorGUILayout.EndVertical();
        
        mDim = (eNoiseDim)EditorGUILayout.EnumPopup("Texture Dimension:", mDim);

        SettingNoise("Channel (R)", noises[0]);
        SettingNoise("Channel (G)", noises[1]);
        SettingNoise("Channel (B)", noises[2]);
        SettingNoise("Channel (A)", noises[3]);

        if (GUILayout.Button("Generate"))
        {
            switch (mDim)
            {
                case eNoiseDim.Dimension2:
                    GenerateNoise_2D();
                    break;

                case eNoiseDim.Dimension3:
                    GenerateNoise_3D();
                    break;
            }
        }
    }

    void SettingNoise(string _Title, NoiseModule _noise)
    {
        if (_noise == null) return;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(_Title);

        EditorGUILayout.BeginVertical("Box");
        _noise.eType = (eNoiseType)EditorGUILayout.EnumPopup("Noise Type:", _noise.eType);
        _noise.iPeriod = EditorGUILayout.IntSlider("Noise Period", _noise.iPeriod, 2, 40);
        
        EditorGUILayout.BeginHorizontal();
        _noise.fRandomSeed = EditorGUILayout.FloatField("Random Seed:", _noise.fRandomSeed);
        if (GUILayout.Button("Random"))
        {
            _noise.fRandomSeed = UnityEngine.Random.Range(0.001f, 200000f);
        }
        EditorGUILayout.EndHorizontal();

        if(EnableFBM(_noise.eType) == true)
        {
            _noise.bFBM = EditorGUILayout.Toggle("Use fBm", _noise.bFBM);
            if (_noise.bFBM == true)
            {
                _noise.iOctaves = EditorGUILayout.IntSlider("fBm Octaves", _noise.iOctaves, 2, 20);
                _noise.fPersistence = EditorGUILayout.Slider("fBm Persistence", _noise.fPersistence, 0f, 1f);
                _noise.fScale = EditorGUILayout.Slider("fBm Scale", _noise.fScale, 1f, 10f);
            }
        }
        else
        {
            _noise.bFBM = false;
        }        
        EditorGUILayout.EndVertical();
    }

    bool EnableFBM(eNoiseType _type)
    {
        bool enabledFBM = true;

        switch(_type)
        {
            case eNoiseType.None:
            case eNoiseType.WhiteNoise:
                enabledFBM = false;
                break;
        }

        return enabledFBM;
    }

    void GenerateNoise_2D()
    {
        Texture2D tex2D = new Texture2D(mTexSize, mTexSize, TextureFormat.RGBA32, false, true);
        Color[] cols = tex2D.GetPixels();

        var buffer = new ComputeBuffer(mTexSize * mTexSize, sizeof(float));
        float[] datas = new float[mTexSize * mTexSize];

        if (SetNoiseCS_2D(noises[0], buffer) == true)
        {            
            buffer.GetData(datas);

            for (int i = 0; i < datas.Length; ++i)
                cols[i].r = datas[i];
        }
        else
        {
            for (int i = 0; i < datas.Length; ++i)
                cols[i].r = 0.0f;
        }

        if (SetNoiseCS_2D(noises[1], buffer) == true)
        {
            buffer.GetData(datas);

            for (int i = 0; i < datas.Length; ++i)
                cols[i].g = datas[i];
        }
        else
        {
            for (int i = 0; i < datas.Length; ++i)
                cols[i].g = 0.0f;
        }

        if (SetNoiseCS_2D(noises[2], buffer) == true)
        {
            buffer.GetData(datas);

            for (int i = 0; i < datas.Length; ++i)
                cols[i].b = datas[i];
        }
        else
        {
            for (int i = 0; i < datas.Length; ++i)
                cols[i].b = 0.0f;
        }

        if (SetNoiseCS_2D(noises[3], buffer) == true)
        {
            buffer.GetData(datas);

            for (int i = 0; i < datas.Length; ++i)
                cols[i].a = datas[i];
        }
        else
        {
            for (int i = 0; i < datas.Length; ++i)
                cols[i].a = 0.0f;
        }

        buffer.Release();

        tex2D.SetPixels(cols);
        tex2D.Apply();

        SaveTexture<Texture2D>(tex2D);
    }

    void GenerateNoise_3D()
    {
        Texture3D tex3D = new Texture3D(mTexSize, mTexSize, mTexSize, TextureFormat.RGBA32, false);
        Color[] cols = tex3D.GetPixels();

        var buffer = new ComputeBuffer(mTexSize * mTexSize * mTexSize, sizeof(float));
        float[] datas = new float[mTexSize * mTexSize * mTexSize];


        if (SetNoiseCS_3D(noises[0], buffer) == true)
        {
            buffer.GetData(datas);

            for (int i = 0; i < datas.Length; ++i)
                cols[i].r = datas[i];
        }
        else
        {
            for (int i = 0; i < datas.Length; ++i)
                cols[i].r = 0f;
        }

        if (SetNoiseCS_3D(noises[1], buffer) == true)
        {
            buffer.GetData(datas);

            for (int i = 0; i < datas.Length; ++i)
                cols[i].g = datas[i];
        }
        else
        {
            for (int i = 0; i < datas.Length; ++i)
                cols[i].g = 0f;
        }

        if (SetNoiseCS_3D(noises[2], buffer) == true)
        {
            buffer.GetData(datas);

            for (int i = 0; i < datas.Length; ++i)
                cols[i].b = datas[i];
        }
        else
        {
            for (int i = 0; i < datas.Length; ++i)
                cols[i].b = 0f;
        }

        if (SetNoiseCS_3D(noises[3], buffer) == true)
        {
            buffer.GetData(datas);

            for (int i = 0; i < datas.Length; ++i)
                cols[i].a = datas[i];
        }
        else
        {
            for (int i = 0; i < datas.Length; ++i)
                cols[i].a = 0f;
        }

        buffer.Release();

        tex3D.SetPixels(cols);
        tex3D.Apply();

        SaveTexture<Texture3D>(tex3D);
    }

    #region Random Table
    delegate ComputeBuffer RandomTable(ComputeShader _cs, int _kanel, float _seed);

    ComputeBuffer SetPerlinRandomTable(ComputeShader _cs, int _kanel, float _seed)
    {
        int[] perm = {
            151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

        UnityEngine.Random.InitState(Mathf.FloorToInt(_seed));

        int[] shuffle = new int[256];
        for (int i = 0; i < 256; ++i)
            shuffle[i] = i;

        for (int i = 0; i < 256; ++i)
        {
            int target = UnityEngine.Random.Range(0, 256);
            int swap = shuffle[i];
            shuffle[i] = shuffle[target];
            shuffle[target] = swap;
        }

        int[] p = new int[512];
        for (int i = 0; i < 512; ++i)
        {
            int t = shuffle[i % 256];
            p[i] = perm[t];
        }

        var buffer = new ComputeBuffer(512, sizeof(int));
        buffer.SetData(p);

        _cs.SetBuffer(_kanel, "Perm", buffer);

        return buffer;
    }

    ComputeBuffer SetPerlinWorleyRandomTable(ComputeShader _cs, int _kanel, float _seed)
    {
        SetCommonRandomTable(_cs, _kanel, _seed);
        return SetPerlinRandomTable(_cs, _kanel, _seed);
    }

    ComputeBuffer SetCommonRandomTable(ComputeShader _cs, int _kanel, float _seed)
    {
        _cs.SetFloat("RandomSeed", _seed);
        return null;
    }
    #endregion

    bool SetNoiseCS_2D(NoiseModule _noise, ComputeBuffer _res)
    {
        if (_noise == null || _res == null) return false;

        string path = string.Empty;
        string kanel = string.Empty;
        RandomTable randFunc = null;

        if (GetComputerShader_2D(_noise.eType, out path, out kanel, out randFunc) == false)
            return false;

        ComputeShader cs = Resources.Load<ComputeShader>(path);
        if (cs == null) return false;

        int kID = cs.FindKernel(kanel);

        ComputeBuffer rtBuffer = null;
        if (randFunc != null)
        {
            rtBuffer = randFunc.Invoke(cs, kID, _noise.fRandomSeed);
        }

        cs.SetInt("Octaves", _noise.bFBM ? _noise.iOctaves : 1);
        cs.SetFloat("Persistence", _noise.fPersistence);
        cs.SetFloat("Scale", _noise.fScale);
        cs.SetFloat("Period", _noise.iPeriod);
        cs.SetInt("TexWidth", mTexSize);
        cs.SetInt("TexHeight", mTexSize);
        cs.SetInt("TexDepth", mTexSize);

        cs.SetBuffer(kID, "Result", _res);

        uint sizeX, sizeY, sizeZ;
        cs.GetKernelThreadGroupSizes(
            kID,
            out sizeX,
            out sizeY,
            out sizeZ
        );

        cs.Dispatch(kID, mTexSize / (int)sizeX, mTexSize / (int)sizeY, 1);

        if (rtBuffer != null)
        {
            rtBuffer.Release();
            rtBuffer = null;
        }

        return true;
    }

    bool SetNoiseCS_3D(NoiseModule _noise, ComputeBuffer _res)
    {
        if (_noise == null || _res == null) return false;

        string path = string.Empty;
        string kanel = string.Empty;
        RandomTable randFunc = null;

        if (GetComputerShader_3D(_noise.eType, out path, out kanel, out randFunc) == false)
            return false;

        ComputeShader cs = Resources.Load<ComputeShader>(path);
        if (cs == null) return false;

        int kID = cs.FindKernel(kanel);

        ComputeBuffer rtBuffer = null;
        if (randFunc != null)
        {
            rtBuffer = randFunc.Invoke(cs, kID, _noise.fRandomSeed);
        }

        cs.SetInt("Octaves", _noise.bFBM ? _noise.iOctaves : 1);
        cs.SetFloat("Persistence", _noise.fPersistence);
        cs.SetFloat("Scale", _noise.fScale);
        cs.SetFloat("Period", _noise.iPeriod);
        cs.SetInt("TexWidth", mTexSize);
        cs.SetInt("TexHeight", mTexSize);
        cs.SetInt("TexDepth", mTexSize);

        cs.SetBuffer(kID, "Result", _res);

        uint sizeX, sizeY, sizeZ;
        cs.GetKernelThreadGroupSizes(
            kID,
            out sizeX,
            out sizeY,
            out sizeZ
        );

        cs.Dispatch(kID, mTexSize / (int)sizeX, mTexSize / (int)sizeY, mTexSize / (int)sizeZ);

        if (rtBuffer != null)
        {
            rtBuffer.Release();
            rtBuffer = null;
        }

        return true;
    }

    bool GetComputerShader_2D(eNoiseType _type, out string _path, out string _kanel, out RandomTable _randFunc)
    {
        bool res = false;

        _path = string.Empty;
        _kanel = string.Empty;
        _randFunc = null;

        switch (_type)
        {
            case eNoiseType.Perlin:
                _path = "TiliedPerlinNoise2D";
                _kanel = "CS_PerlinNoise2D";
                _randFunc = SetPerlinRandomTable;
                res = true;
                break;

            case eNoiseType.Voronoi:
                _path = "TiliedVoronoi2D";
                _kanel = "CS_Voronoi2D";
                _randFunc = SetCommonRandomTable;
                res = true;
                break;

            case eNoiseType.Cellular:
                _path = "TiliedCellularNoise2D";
                _kanel = "CS_CellularNoise2D";
                _randFunc = SetCommonRandomTable;
                res = true;
                break;

            case eNoiseType.PerlinWorley:
                _path = "TiliedPerlinWorley2D";
                _kanel = "CS_PerlinWorley2D";
                _randFunc = SetPerlinWorleyRandomTable;
                res = true;
                break;

            case eNoiseType.WhiteNoise:
                _path = "WhiteNoise2D";
                _kanel = "CS_WhiteNoise2D";
                _randFunc = SetCommonRandomTable;
                res = true;
                break;
        }

        return res;
    }

    bool GetComputerShader_3D(eNoiseType _type, out string _path, out string _kanel, out RandomTable _randFunc)
    {
        bool res = false;

        _path = string.Empty;
        _kanel = string.Empty;
        _randFunc = null;

        switch (_type)
        {
            case eNoiseType.Perlin:
                _path = "TiliedPerlinNoise3D";
                _kanel = "CS_PerlinNoise3D";
                _randFunc = SetPerlinRandomTable;
                res = true;
                break;

            case eNoiseType.Voronoi:
                _path = "TiliedVoronoi3D";
                _kanel = "CS_Voronoi3D";
                _randFunc = SetCommonRandomTable;
                res = true;
                break;

            case eNoiseType.Cellular:
                _path = "TiliedCellularNoise3D";
                _kanel = "CS_CellularNoise3D";
                _randFunc = SetCommonRandomTable;
                res = true;
                break;

            case eNoiseType.PerlinWorley:
                _path = "TiliedPerlinWorley3D";
                _kanel = "CS_PerlinWorley3D";
                _randFunc = SetPerlinWorleyRandomTable;
                res = true;
                break;

            case eNoiseType.WhiteNoise:
                _path = "WhiteNoise3D";
                _kanel = "CS_WhiteNoise3D";
                _randFunc = SetCommonRandomTable;
                res = true;
                break;
        }

        return res;
    }

    
    bool IntPow2Field(string _label, ref int data, params GUILayoutOption[] par)
    {
        int bak = data;
        data = EditorGUILayout.IntField(_label, bak);

        data = Mathf.Max(32, data);

        if (Mathf.IsPowerOfTwo(data) == false)
            data = Mathf.ClosestPowerOfTwo(data);

        return bak != data;
    }

    void SaveTexture<T>(T _Tex) where T : Texture
    {
        if (_Tex == null) return;

        string path = "Assets/sigmalin/Institute/";

        if (System.IO.Directory.Exists(path) != true)
        {
            System.IO.Directory.CreateDirectory(path);
        }

        string output = string.Format("{0}R({1})_G({2})_B({3})_A({4}))_{5}_{6}.asset", path,
            noises[0].eType,
            noises[1].eType,
            noises[2].eType,
            noises[3].eType,
            mTexSize, mDim);

        if (System.IO.File.Exists(output) == true)
        {
            System.IO.File.Delete(output);
            AssetDatabase.Refresh();
        }

        T clone = GameObject.Instantiate<T>(_Tex);

        UnityEditor.AssetDatabase.CreateAsset(clone, output);

        AssetDatabase.SaveAssets();

        UnityEditor.AssetDatabase.ImportAsset(output);

        AssetDatabase.Refresh();

    }

    bool ObjectField<T>(string name, ref T data, bool allowSceneObjects, params GUILayoutOption[] par) where T : UnityEngine.Object
    {
        System.Type t;
        if (data == null)
        {
            t = typeof(UnityEngine.Object);
        }
        else
        {
            t = data.GetType();
        }

        T newData = EditorGUILayout.ObjectField(name, data, t, allowSceneObjects, par) as T;

        if (newData != data)
        {
            data = newData;
            return true;
        }

        return false;
    }
}
