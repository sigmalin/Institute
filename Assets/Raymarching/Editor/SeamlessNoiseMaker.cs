using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class SeamlessNoiseMaker : EditorWindow
{
    static SeamlessNoiseMaker window;

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

    [MenuItem("sigmalin/Institute/Noise/SeamlessNoiseMaker")]
    static void OpenSeamlessNoiseMaker()
    {
        window = (SeamlessNoiseMaker)EditorWindow.GetWindow(typeof(SeamlessNoiseMaker));
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
        if(GUILayout.Button("Random"))
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
        if(mFBM == true)
        {
            mOctaves = EditorGUILayout.IntSlider("fBm Octaves", mOctaves, 2, 10);
            mPersistence = EditorGUILayout.Slider("fBm Persistence", mPersistence, 0f, 1f);
            mScale = EditorGUILayout.Slider("fBm Scale", mScale, 1f, 10f);
        }
        EditorGUILayout.EndVertical();
        
        if (GUILayout.Button("Generate"))
        {
            switch(mDim)
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
        NoiseUtility.RandomSeed = mRandomSeed;
        NoiseUtility.Octaves = mOctaves;
        NoiseUtility.Persistence = mPersistence;
        NoiseUtility.Scale = mScale;

        NoiseUtility.Noise2D func = null;

        switch(mType)
        {
            case eNoiseType.Perlin:
                NoiseUtility.InitPerlinTable();
                func = NoiseUtility.TiliedPerlinNoise2D;
                break;

            case eNoiseType.Voronoi:
                func = NoiseUtility.TiliedVoronoi2D;
                break;

            case eNoiseType.Cellular:
                func = NoiseUtility.TiliedCellularNoise2D;
                break;
        }

        Texture2D tex2D;
        NoiseUtility.CreateTexture(mTexSize, mTexPeriod, func, mFBM, out tex2D);

        SaveTexture<Texture2D>(tex2D);
    }

    void GenerateNoise3D()
    {
        NoiseUtility.RandomSeed = mRandomSeed;
        NoiseUtility.Octaves = mOctaves;
        NoiseUtility.Persistence = mPersistence;
        NoiseUtility.Scale = mScale;

        NoiseUtility.Noise3D func = null;

        switch (mType)
        {
            case eNoiseType.Perlin:
                NoiseUtility.InitPerlinTable();
                func = NoiseUtility.TiliedPerlinNoise3D;
                break;

            case eNoiseType.Voronoi:
                func = NoiseUtility.TiliedVoronoi3D;
                break;

            case eNoiseType.Cellular:
                func = NoiseUtility.TiliedCellularNoise3D;
                break;
        }

        Texture3D tex3D;
        NoiseUtility.CreateTexture(mTexSize, mTexPeriod, func, mFBM, out tex3D);

        SaveTexture<Texture3D>(tex3D);
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
