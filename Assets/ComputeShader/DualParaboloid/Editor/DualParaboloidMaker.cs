using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class DualParaboloidMaker : IBLMaker
{
    static DualParaboloidMaker window;

    public Cubemap cubeEnvironment;

    int mTexSize = 512;

    bool mUseComputerShader;

    [MenuItem("sigmalin/Institute/DualParaboloid/DualParaboloidMaker")]
    static void OpenDualParaboloidMaker()
    {
        window = (DualParaboloidMaker)EditorWindow.GetWindow(typeof(DualParaboloidMaker));
    }

    void OnGUI()
    {
        if (window == null)
        {
            GUILayout.Label("Missing window reference!");
            return;
        }

        ObjectField<Cubemap>("Trans Environment Cube (HDR)", ref cubeEnvironment, false);
        if (cubeEnvironment == null) return;

        IntPow2Field("Cube Size", ref mTexSize);

        mUseComputerShader = GUILayout.Toggle(mUseComputerShader, "Use ComputerShader");

        if (GUILayout.Button("Generate"))
        {
            if(mUseComputerShader)
                ComputeDualParaboloid();
            else
                RenderDualParaboloid();
        }
    }

    void ComputeDualParaboloid()
    {
        ComputeShader cs = Resources.Load<ComputeShader>("Cube2DualParaboloid");
        if (cs == null) return;

        int kanel = cs.FindKernel("CS_Cube2DualParaboloid");

        var buffer = new ComputeBuffer(mTexSize * mTexSize * 2, sizeof(float) * 4);

        cs.SetTexture(kanel, "cubemap", cubeEnvironment);
        cs.SetBuffer(kanel, "Result", buffer);

        cs.SetInt("texSize", mTexSize);

        uint sizeX, sizeY, sizeZ;
        cs.GetKernelThreadGroupSizes(
            kanel,
            out sizeX,
            out sizeY,
            out sizeZ
        );

        cs.Dispatch(kanel, (mTexSize * 2) / (int)sizeX, mTexSize / (int)sizeY, 1);

        Color[] cols = new Color[mTexSize * mTexSize * 2];
        buffer.GetData(cols);
        buffer.Release();

        Texture2D clone = new Texture2D(mTexSize * 2, mTexSize, TextureFormat.RGBA32, false, true);
        clone.SetPixels(cols);
        clone.Apply();

        SaveTexture<Texture2D>(clone, GetPath("Combine"));
    }

    void RenderDualParaboloid()
    {
        Shader shader = Shader.Find("DualParaboloid/DrawDualParaboloidCube");
        if (shader == null) return;

        Material mat = new Material(shader);
        mat.SetTexture("_Cubemap", cubeEnvironment);

        Mesh cube = null;
        CreateCubeMesh(out cube);

        RenderFrontParaboloid(mat, cube);

        RenderRearParaboloid(mat, cube);
    }

    void RenderFrontParaboloid(Material _mat, Mesh _mesh)
    {
        Matrix4x4 view = Matrix4x4.Inverse(Matrix4x4.TRS(
            Vector3.zero,
            Quaternion.LookRotation(Vector3.forward, Vector3.up),
            new Vector3(1, 1, -1)
        ));

        RenderTexture rt = new RenderTexture(mTexSize, mTexSize, 16);

        UnityEngine.Rendering.CommandBuffer cmd = new UnityEngine.Rendering.CommandBuffer();
        cmd.name = "RenderFrontParaboloid";

        cmd.SetViewProjectionMatrices(view, Matrix4x4.identity);
        cmd.SetRenderTarget(rt);
        cmd.ClearRenderTarget(true, true, Color.clear);
        cmd.DrawMesh(_mesh, Matrix4x4.identity, _mat);

        Graphics.ExecuteCommandBuffer(cmd);

        Texture2D tex = RenderTexture2Texture2D(rt);

        SaveTexture<Texture2D>(tex, GetPath("Front"));
    }

    void RenderRearParaboloid(Material _mat, Mesh _mesh)
    {
        Matrix4x4 view = Matrix4x4.Inverse(Matrix4x4.TRS(
            Vector3.zero,
            Quaternion.LookRotation(Vector3.back, Vector3.up),
            new Vector3(1, 1, -1)
        ));

        RenderTexture rt = new RenderTexture(mTexSize, mTexSize, 16);

        UnityEngine.Rendering.CommandBuffer cmd = new UnityEngine.Rendering.CommandBuffer();
        cmd.name = "RenderFrontParaboloid";

        cmd.SetViewProjectionMatrices(view, Matrix4x4.identity);
        cmd.SetRenderTarget(rt);
        cmd.ClearRenderTarget(true, true, Color.clear);
        cmd.DrawMesh(_mesh, Matrix4x4.identity, _mat);

        Graphics.ExecuteCommandBuffer(cmd);

        Texture2D tex = RenderTexture2Texture2D(rt);

        SaveTexture<Texture2D>(tex, GetPath("Rear"));
    }

    Texture2D RenderTexture2Texture2D(RenderTexture _rt)
    {
        RenderTexture.active = _rt;

        Texture2D texture = new Texture2D(_rt.width, _rt.height, TextureFormat.RGB24, false, true);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.ReadPixels(new Rect(0, 0, _rt.width, _rt.height), 0, 0);
        texture.Apply(false, false);

        RenderTexture.active = null;

        return texture;
    }

    void CreateCubeMesh(out Mesh _mesh)
    {
        const int interval = 20;
        const float step = Mathf.PI / interval;

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        for (int phi = 0; phi <= (interval * 2); ++phi)
        {
            float sinPhi = Mathf.Sin(step * phi);
            float cosPhi = Mathf.Cos(step * phi);

            for (int theta = 0; theta <= interval; ++theta)
            {
                float sinTheta = Mathf.Sin(step * theta);
                float cosTheta = Mathf.Cos(step * theta);

                vertices.Add(new Vector3(sinTheta * cosPhi, sinTheta * sinPhi, cosTheta));
            }
        }

        for (int phi = 0; phi < (interval * 2); ++phi)
        {
            int IndxTheta = phi * (interval + 1);

            for (int theta = 0; theta < interval; ++theta)
            {
                indices.Add(IndxTheta);
                indices.Add(IndxTheta + interval + 1);
                indices.Add(IndxTheta + 1);
                indices.Add(IndxTheta + interval + 2);
                indices.Add(IndxTheta + 1);
                indices.Add(IndxTheta + interval + 1);

                ++IndxTheta;
            }
        }

        _mesh = new Mesh();
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(indices, 0);

        _mesh.UploadMeshData(true);
    }

    string GetPath(string _mark)
    {
        string path = "Assets/sigmalin/Institute/";

        if (System.IO.Directory.Exists(path) != true)
        {
            System.IO.Directory.CreateDirectory(path);
        }

        string output = string.Format("{0}{1}_DualParaboloid_{2}_{3}.asset", path, cubeEnvironment.name, _mark, mTexSize);

        if (System.IO.File.Exists(output) == true)
        {
            System.IO.File.Delete(output);
            AssetDatabase.Refresh();
        }

        return output;
    }
}
