using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class IntegrateBRDFMaker : IBLMaker
{
    static IntegrateBRDFMaker window;

    int mTexSize = 1024;

    public enum TexFormat
    {
        RGB24 = 0,
        R16G16 = 1,
    }

    TexFormat mFmt;

    [MenuItem("sigmalin/Institute/IBL/IntegrateBRDFMaker")]
    static void OpenIntegrateBRDFMaker()
    {
        window = (IntegrateBRDFMaker)EditorWindow.GetWindow(typeof(IntegrateBRDFMaker));
    }

    void OnGUI()
    {
        if (window == null)
        {
            GUILayout.Label("Missing window reference!");
            return;
        }
        
        IntPow2Field("IntegrateBRDF Texture Size", ref mTexSize);

        TextureReadableField();

        mFmt = (TexFormat)EditorGUILayout.EnumPopup("Texture Format", mFmt);

        if (GUILayout.Button("Generate"))
        {
            ComputeIntegrateBRDF();
        }
    }

    void ComputeIntegrateBRDF()
    {
        ComputeShader cs = Resources.Load<ComputeShader>("IntegrateBRDF");
        if (cs == null) return ;

        int kanel = cs.FindKernel("CS_IntegrateBRDF");        

        var buffer = new ComputeBuffer(mTexSize * mTexSize, sizeof(float) * 4);
        cs.SetBuffer(kanel, "Result", buffer);

        cs.SetInt("texSize", mTexSize);

        uint sizeX, sizeY, sizeZ;
        cs.GetKernelThreadGroupSizes(
            kanel,
            out sizeX,
            out sizeY,
            out sizeZ
        );

        cs.Dispatch(kanel, mTexSize / (int)sizeX, mTexSize / (int)sizeY, 1);

        Color[] cols = new Color[mTexSize * mTexSize];
        buffer.GetData(cols);
        buffer.Release();

        TextureFormat texFmt = TextureFormat.RGBA32;

        switch (mFmt)
        {
            case TexFormat.R16G16:
                texFmt = TextureFormat.RGHalf;
                break;

            case TexFormat.RGB24:
                texFmt = TextureFormat.RGB24;
                break;
        }

        Texture2D clone = new Texture2D(mTexSize, mTexSize, texFmt, false, true);
        clone.SetPixels(cols);
        clone.Apply(false, !mOutputTextureReadable);

        SaveTexture<Texture2D>(clone, GetPath());
    }

    string GetPath()
    {
        string path = "Assets/sigmalin/Institute/";

        if (System.IO.Directory.Exists(path) != true)
        {
            System.IO.Directory.CreateDirectory(path);
        }

        string output = string.Format("{0}IntegrateBRDF_{1}_{2}.asset", path, mFmt, mTexSize);

        if (System.IO.File.Exists(output) == true)
        {
            System.IO.File.Delete(output);
            AssetDatabase.Refresh();
        }

        return output;
    }
}
