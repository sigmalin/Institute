using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class SeamlessNoiseMakerGPU : EditorWindow
{
    static SeamlessNoiseMakerGPU window;

    float mRandomSeed = 114514.1515f;
    int mTexSize = 64;
    int mTexPeriod = 5;

    enum eNoiseType
    {
        Perlin,
        Voronoi,
        Cellular,
    };

    enum eNoiseDim
    {
        Dimension2,
        Dimension3,
    };

    eNoiseType mType = eNoiseType.Perlin;
    eNoiseDim mDim = eNoiseDim.Dimension2;

    bool mFBM = false;
    int mOctaves = 4;
    float mPersistence = 0.5f;
    float mScale = 2f;

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

        EditorGUILayout.BeginVertical("Box");
        IntPow2Field("Noise Texture Size", ref mTexSize);
        mTexPeriod = EditorGUILayout.IntSlider("Noise Texture Period", mTexPeriod, 2, 10);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal("Box");
        EditorGUILayout.LabelField("Random Seed:", mRandomSeed.ToString());
        if (GUILayout.Button("Random"))
        {
            mRandomSeed = UnityEngine.Random.Range(0.001f, 200000f);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("Box");
        mType = (eNoiseType)EditorGUILayout.EnumPopup("Noise Type:", mType);
        mDim = (eNoiseDim)EditorGUILayout.EnumPopup("Noise Dimension:", mDim);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("Box");
        mFBM = EditorGUILayout.Toggle("Use fBm", mFBM);
        if (mFBM == true)
        {
            mOctaves = EditorGUILayout.IntSlider("fBm Octaves", mOctaves, 2, 10);
            mPersistence = EditorGUILayout.Slider("fBm Persistence", mPersistence, 0f, 1f);
            mScale = EditorGUILayout.Slider("fBm Scale", mScale, 1f, 10f);
        }
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Generate"))
        {
            switch (mDim)
            {
                case eNoiseDim.Dimension2:
                    GenerateNoise2D();
                    break;

                case eNoiseDim.Dimension3:
                    GenerateNoise3D();
                    break;
            }
        }
    }

    void GenerateNoise2D()
    {
        var buffer = new ComputeBuffer(mTexSize * mTexSize, sizeof(float));
               
        switch (mType)
        {
            case eNoiseType.Perlin:
                SetNoiseCS("TiliedPerlinNoise2D", "CS_PerlinNoise2D", buffer, SetPerlinRandomTable);
                break;

            case eNoiseType.Voronoi:
                SetNoiseCS("TiliedVoronoi2D", "CS_Voronoi2D", buffer, SetCommonRandomTable);
                break;

            case eNoiseType.Cellular:
                SetNoiseCS("TiliedCellularNoise2D", "CS_CellularNoise2D", buffer, SetCommonRandomTable);
                break;
        }

        Texture2D tex2D = new Texture2D(mTexSize, mTexSize);

        float[] datas = new float[mTexSize * mTexSize];
        buffer.GetData(datas);
        buffer.Release();

        Color[] cols = tex2D.GetPixels();

        for (int i = 0; i < datas.Length; ++i)
        {
            float d = datas[i];
            cols[i] = new Color(d, d, d, d);
        }

        tex2D.SetPixels(cols);
        tex2D.Apply();        

        SaveTexture<Texture2D>(tex2D);
    }

    void GenerateNoise3D()
    {
        var buffer = new ComputeBuffer(mTexSize * mTexSize * mTexSize, sizeof(float));

        switch (mType)
        {
            case eNoiseType.Perlin:
                SetNoiseCS("TiliedPerlinNoise3D", "CS_PerlinNoise3D", buffer, SetPerlinRandomTable);
                break;

            case eNoiseType.Voronoi:
                SetNoiseCS("TiliedVoronoi3D", "CS_Voronoi3D", buffer, SetCommonRandomTable);
                break;

            case eNoiseType.Cellular:
                SetNoiseCS("TiliedCellularNoise3D", "CS_CellularNoise3D", buffer, SetCommonRandomTable);
                break;
        }

        Texture3D tex3D = new Texture3D(mTexSize, mTexSize, mTexSize, TextureFormat.RGBA32, true);

        float[] datas = new float[mTexSize * mTexSize * mTexSize];
        buffer.GetData(datas);
        buffer.Release();

        Color[] cols = tex3D.GetPixels();

        for (int i = 0; i < datas.Length; ++i)
        {
            float d = datas[i];
            cols[i] = new Color(d, d, d, d);
        }

        tex3D.SetPixels(cols);
        tex3D.Apply();

        SaveTexture<Texture3D>(tex3D);
    }

    #region Random Table
    delegate void RandomTable(ComputeShader _cs, int _kanel);

    void SetPerlinRandomTable(ComputeShader _cs, int _kanel)
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

        UnityEngine.Random.InitState(Mathf.FloorToInt(mRandomSeed));

        int[] p = new int[512];
        for (int i = 0; i < 512; ++i)
        {
            p[i] = perm[i % 256];
        }

        var buffer = new ComputeBuffer(512, sizeof(int));
        buffer.SetData(p);

        _cs.SetBuffer(_kanel, "Perm", buffer);
    }

    void SetCommonRandomTable(ComputeShader _cs, int _kanel)
    {
        _cs.SetFloat("RandomSeed", mRandomSeed);
    }
    #endregion

    bool SetNoiseCS(string _path, string _kanel, ComputeBuffer _res, RandomTable _randFunc = null)
    {
        ComputeShader cs = Resources.Load<ComputeShader>(_path);
        if (cs == null) return false;

        int kanel = cs.FindKernel(_kanel);

        if(_randFunc != null) _randFunc.Invoke(cs, kanel);

        cs.SetInt("Octaves", mOctaves);
        cs.SetFloat("Persistence", mPersistence);
        cs.SetFloat("Scale", mScale);
        cs.SetFloat("Period", mTexPeriod);
        cs.SetInt("TexWidth", mTexSize);
        cs.SetInt("TexHeight", mTexSize);
        cs.SetInt("TexDepth", mTexSize);

        cs.SetBuffer(kanel, "Result", _res);

        uint sizeX, sizeY, sizeZ;
        cs.GetKernelThreadGroupSizes(
            kanel,
            out sizeX,
            out sizeY,
            out sizeZ
        );

        cs.Dispatch(kanel, mTexSize / (int)sizeX, mTexSize / (int)sizeY, mTexSize / (int)sizeZ);

        return true;
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

        string output = string.Format("{0}{1}_{2}_{3}.asset", path, mType, mTexSize, mDim);

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
