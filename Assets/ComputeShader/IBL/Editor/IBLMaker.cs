using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class IBLMaker : EditorWindow
{
    protected enum HDRDecoder
    {
        dLDR,
        BC6H_UFloat,
        Other,
    }

    protected Vector4 mDecodeInstructions;

    protected HDRDecoder mHDRDecoder;

    void OnInspectorUpdate()
    {
        Repaint();
    }

    protected bool HDRDecoderField()
    {
        mHDRDecoder = (HDRDecoder)EditorGUILayout.EnumPopup("HDR Decoder", mHDRDecoder);
        switch (mHDRDecoder)
        {
            case HDRDecoder.dLDR:
                mDecodeInstructions = new Vector4(2, 1, 0, 0);
                break;
            case HDRDecoder.BC6H_UFloat:
                mDecodeInstructions = new Vector4(1, 1, 0, 0);
                break;
            default:
                mDecodeInstructions = EditorGUILayout.Vector4Field("Decode Instructions", mDecodeInstructions);
                break;
        }

        return true;
    }

    protected bool IntPow2Field(string _label, ref int data, params GUILayoutOption[] par)
    {
        int bak = data;
        data = EditorGUILayout.IntField(_label, bak);

        data = Mathf.Max(32, data);

        if (Mathf.IsPowerOfTwo(data) == false)
            data = Mathf.ClosestPowerOfTwo(data);

        return bak != data;
    }

    protected void SetColorSpace(ComputeShader _cs)
    {
        _cs.SetFloat("colorSpace", PlayerSettings.colorSpace == ColorSpace.Linear ? 1f : 2.2f);
    }

    protected void SetHDRDecode(ComputeShader _cs)
    {
        _cs.SetVector("decodeInstructions", mDecodeInstructions);
    }

    protected void SaveTexture<T>(T _Tex, string _Path) where T : Texture
    {
        if (_Tex == null) return;

        T clone = GameObject.Instantiate<T>(_Tex);

        UnityEditor.AssetDatabase.CreateAsset(clone, _Path);

        AssetDatabase.SaveAssets();

        UnityEditor.AssetDatabase.ImportAsset(_Path);

        AssetDatabase.Refresh();

    }

    protected bool ObjectField<T>(string name, ref T data, bool allowSceneObjects, params GUILayoutOption[] par) where T : UnityEngine.Object
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
