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

        Texture2D clone = new Texture2D(mTexSize, mTexSize, TextureFormat.RGBA32, false, true);
        clone.SetPixels(cols);
        clone.Apply();

        SaveTexture<Texture2D>(clone, GetPath());
    }

    string GetPath()
    {
        string path = "Assets/sigmalin/Institute/";

        if (System.IO.Directory.Exists(path) != true)
        {
            System.IO.Directory.CreateDirectory(path);
        }

        string output = string.Format("{0}IntegrateBRDF_{1}.asset", path, mTexSize);

        if (System.IO.File.Exists(output) == true)
        {
            System.IO.File.Delete(output);
            AssetDatabase.Refresh();
        }

        return output;
    }
}
