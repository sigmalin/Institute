using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class PrefilterEnvMapMaker : IBLMaker
{
    static PrefilterEnvMapMaker window;

    public Cubemap cubeEnvironment;

    int mCubeSize = 1024;

    const int MIN_MAP_LEVEL = 5;

    [MenuItem("sigmalin/Institute/IBL/PrefilterEnvMapMaker")]
    static void OpenPrefilterEnvMapMaker()
    {
        window = (PrefilterEnvMapMaker)EditorWindow.GetWindow(typeof(PrefilterEnvMapMaker));
    }

    void OnGUI()
    {
        if (window == null)
        {
            GUILayout.Label("Missing window reference!");
            return;
        }

        ObjectField<Cubemap>("Baked Environment Cube (HDR)", ref cubeEnvironment, false);
        if (cubeEnvironment == null) return;

        IntPow2Field("Cube Size", ref mCubeSize);
        mCubeSize = Mathf.Max(512, mCubeSize);

        TextureReadableField();

        HDRDecoderField();

        if (GUILayout.Button("Generate"))
        {
            ComputePrefilterEnvMap();
        }
    }

    void ComputePrefilterEnvMap()
    {
        ComputeShader cs = Resources.Load<ComputeShader>("PrefilterEnvMap");
        if (cs == null) return ;

        int kanel = cs.FindKernel("CS_PrefilterEnvMap");

        CubemapFace[] faces = new CubemapFace[]
        {
            CubemapFace.PositiveX,
            CubemapFace.NegativeX,
            CubemapFace.PositiveY,
            CubemapFace.NegativeY,
            CubemapFace.PositiveZ,
            CubemapFace.NegativeZ,
        };

        Cubemap clone = new Cubemap(mCubeSize, TextureFormat.RGBA32, MIN_MAP_LEVEL);

        for (int i = 0; i < faces.Length; ++i)
        {
            for (int j = 0; j < MIN_MAP_LEVEL; ++j)
            {
                Color[] cols;
                CreateFace(cs, kanel, i, j, out cols);
                clone.SetPixels(cols, faces[i], j);
            }
        }

        clone.Apply(false, !mOutputTextureReadable);
        SaveTexture<Cubemap>(clone, GetPath());
    }

    void CreateFace(ComputeShader _cs, int _kanel, int _face, int _mipmap, out Color[] _cols)
    {
        _cols = null;
        if (6 <= _face) return;

        int size = Mathf.Max(1, mCubeSize >> _mipmap);

        var buffer = new ComputeBuffer(size * size, sizeof(float) * 4);

        _cs.SetTexture(_kanel, "cubemap", cubeEnvironment);
        _cs.SetBuffer(_kanel, "Result", buffer);
        _cs.SetInt("face", _face);
        _cs.SetInt("cubeSize", size);
        _cs.SetFloat("roughness", ((float)(_mipmap)) / ((float)(MIN_MAP_LEVEL-1)));

        SetHDRDecode(_cs);
        SetColorSpace(_cs);

        uint sizeX, sizeY, sizeZ;
        _cs.GetKernelThreadGroupSizes(
            _kanel,
            out sizeX,
            out sizeY,
            out sizeZ
        );

        _cs.Dispatch(_kanel, size / (int)sizeX, size / (int)sizeY, 1);

        _cols = new Color[size * size];
        buffer.GetData(_cols);
        buffer.Release();
    }

    string GetPath()
    {
        string path = "Assets/sigmalin/Institute/";

        if (System.IO.Directory.Exists(path) != true)
        {
            System.IO.Directory.CreateDirectory(path);
        }

        string output = string.Format("{0}{1}_PrefilterEnvMap_{2}.asset", path, cubeEnvironment.name, mCubeSize);

        if (System.IO.File.Exists(output) == true)
        {
            System.IO.File.Delete(output);
            AssetDatabase.Refresh();
        }

        return output;
    }
}
