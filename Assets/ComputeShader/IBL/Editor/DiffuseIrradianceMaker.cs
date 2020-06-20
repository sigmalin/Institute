using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class DiffuseIrradianceMaker : IBLMaker
{
    static DiffuseIrradianceMaker window;

    public Cubemap cubeEnvironment;

    int mCubeSize = 32;

    [MenuItem("sigmalin/Institute/IBL/DiffuseIrradianceMaker")]
    static void OpenDiffuseIrradianceMaker()
    {
        window = (DiffuseIrradianceMaker)EditorWindow.GetWindow(typeof(DiffuseIrradianceMaker));
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

        HDRDecoderField();

        if (GUILayout.Button("Generate"))
        {
            ComputeIrradianceDiffuse();
        }
    }

    void ComputeIrradianceDiffuse()
    {
        ComputeShader cs = Resources.Load<ComputeShader>("DiffuseIrradiance");
        if (cs == null) return ;

        int kanel = cs.FindKernel("CS_DiffuseIrradiance");

        CubemapFace[] faces = new CubemapFace[]
        {
            CubemapFace.PositiveX,
            CubemapFace.NegativeX,
            CubemapFace.PositiveY,
            CubemapFace.NegativeY,
            CubemapFace.PositiveZ,
            CubemapFace.NegativeZ,
        };

        Cubemap clone = new Cubemap(mCubeSize, TextureFormat.RGBA32, false);

        for (int i = 0; i < faces.Length; ++i)
        {
            Color[] cols;
            CreateFace(cs, kanel, i, out cols);
            clone.SetPixels(cols, faces[i]);
        }

        clone.Apply();
        SaveTexture<Cubemap>(clone, GetPath());
    }

    void CreateFace(ComputeShader _cs, int _kanel, int _face, out Color[] _cols)
    {
        _cols = null;
        if (6 <= _face) return;

        var buffer = new ComputeBuffer(mCubeSize * mCubeSize, sizeof(float) * 4);

        _cs.SetTexture(_kanel, "cubemap", cubeEnvironment);
        _cs.SetBuffer(_kanel, "Result", buffer);
        _cs.SetInt("face", _face);
        _cs.SetInt("cubeSize", mCubeSize);

        SetHDRDecode(_cs);
        SetColorSpace(_cs);

        uint sizeX, sizeY, sizeZ;
        _cs.GetKernelThreadGroupSizes(
            _kanel,
            out sizeX,
            out sizeY,
            out sizeZ
        );

        _cs.Dispatch(_kanel, mCubeSize / (int)sizeX, mCubeSize / (int)sizeY, 1);

        _cols = new Color[mCubeSize * mCubeSize];
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

        string output = string.Format("{0}{1}_DiffuseIrradiance_{2}.asset", path, cubeEnvironment.name, mCubeSize);

        if (System.IO.File.Exists(output) == true)
        {
            System.IO.File.Delete(output);
            AssetDatabase.Refresh();
        }

        return output;
    }
}
