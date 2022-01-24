using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class QuadTreeCulling
{
    int kernelCullingPatches;
        
    int MatrixVPID;
    int MeshRadiusID;
    int IsOpenGLID;

    int PatchesBufferID;
    int CullingResultID;

    ComputeBuffer CullingPatchesBuffer;

    GraphicsBuffer argBuffer;

    QuadTreeSetting Setting;

    public QuadTreeCulling(QuadTreeSetting setting)
    {
        Setting = setting;

        if (Setting != null && Setting.CullingPatchesCS != null)
        {
            kernelCullingPatches = Setting.CullingPatchesCS.FindKernel("CSCullingPatches");

            MatrixVPID = Shader.PropertyToID("matrixVP");
            MeshRadiusID = Shader.PropertyToID("meshRadius");
            IsOpenGLID = Shader.PropertyToID("isOpenGL");

            PatchesBufferID = Shader.PropertyToID("patchesBuffer");
            CullingResultID = Shader.PropertyToID("cullingResult");
        }
    }

    public void Initialize()
    {
        if (Setting != null)
        {
            InitGraphicsBuffer();
        }
    }

    public void Release()
    {
        ReleaseGraphicsBuffer();
    }

    bool isValid()
    {
        return Setting != null && Setting.CullingPatchesCS != null &&
                CullingPatchesBuffer != null && argBuffer != null;
    }

    void InitGraphicsBuffer()
    {
        ReleaseGraphicsBuffer();

        CreateCullingBuffer();

        CreateArgumentBuffer();
    }

    void ReleaseGraphicsBuffer()
    {
        if (CullingPatchesBuffer != null)
        {
            CullingPatchesBuffer.Release();
            CullingPatchesBuffer.Dispose();
            CullingPatchesBuffer = null;
        }

        if(argBuffer != null)
        {
            argBuffer.Release();
            argBuffer.Dispose();
            argBuffer = null;
        }
    }

    void CreateCullingBuffer()
    {
        int maxPatchCount = Setting.MaxPatchCount;

        CullingPatchesBuffer = new ComputeBuffer(maxPatchCount, sizeof(float) * 2 + sizeof(uint) + sizeof(uint) + sizeof(uint), ComputeBufferType.Append);
    }

    void CreateArgumentBuffer()
    {
        uint[] args = new uint[]
        {
            1u,    // number of work groups in X dimension
            1u,    // number of work groups in Y dimension
            1u,    // number of work groups in Z dimension
        };

        argBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, args.Length, sizeof(uint));
        argBuffer.SetData(args);
    }

    public bool Culling(CommandBuffer cmd, Camera cam, ComputeBuffer patches, out ComputeBuffer result)
    {
        result = null;

        if (isValid() == false) return false;

        cmd.SetComputeBufferCounterValue(CullingPatchesBuffer, 0);

        cmd.CopyCounterValue(patches, argBuffer, 0);

        Matrix4x4 proj = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);

        cmd.SetComputeIntParam(Setting.CullingPatchesCS, IsOpenGLID, cam.projectionMatrix.Equals(proj) ? 1 : 0);

        var viewProj = proj * cam.worldToCameraMatrix;
        cmd.SetComputeMatrixParam(Setting.CullingPatchesCS, MatrixVPID, viewProj);

        cmd.SetComputeIntParam(Setting.CullingPatchesCS, MeshRadiusID, Setting.LodMeshRadius);

        cmd.SetComputeBufferParam(Setting.CullingPatchesCS, kernelCullingPatches, PatchesBufferID, patches);
        cmd.SetComputeBufferParam(Setting.CullingPatchesCS, kernelCullingPatches, CullingResultID, CullingPatchesBuffer);

        cmd.DispatchCompute(Setting.CullingPatchesCS, kernelCullingPatches, argBuffer, 0);

        result = CullingPatchesBuffer;

        return true;
    }
}
