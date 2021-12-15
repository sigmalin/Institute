using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HizObject : MonoBehaviour, IHizCullingObject
{
    public Mesh mesh;
    public Material material;

    const int OBJECT_COUNT = 100;
    const int ARG_BUFFER_SIZE = 5;

    ComputeBuffer postionBuffer;
    ComputeBuffer cullingResultBuffer;
    ComputeBuffer argBuffer;

    // Start is called before the first frame update
    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        Release();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isValid() == false) return;

        CullingRenderPassFeature.Instance.Register(this);
    }

    private void Initialize()
    {
        Release();

        InitArgBuffer();

        InitPosition();
    }

    private void Release()
    {
        if (postionBuffer != null)
        {
            postionBuffer.Release();
            postionBuffer = null;
        }

        if (cullingResultBuffer != null)
        {
            cullingResultBuffer.Release();
            cullingResultBuffer = null;
        }

        if (argBuffer != null)
        {
            argBuffer.Release();
            argBuffer = null;
        }
    }

    void InitArgBuffer()
    {
        postionBuffer = new ComputeBuffer(OBJECT_COUNT, sizeof(float) * 3);

        cullingResultBuffer = new ComputeBuffer(OBJECT_COUNT, sizeof(float) * 3, ComputeBufferType.Append);

        argBuffer = new ComputeBuffer(ARG_BUFFER_SIZE, sizeof(uint), ComputeBufferType.IndirectArguments);        
    }

    void InitPosition()
    {
        const float width = 10f;
        const float height = 10f;

        Vector3[] position = new Vector3[OBJECT_COUNT];
        for(int i = 0; i < OBJECT_COUNT; ++i)
        {
            float x = (float)RadicalInverse_VdC((uint)i) * width;
            float y = (float)i / OBJECT_COUNT * height;

            position[i].x = x - (width * 0.5f);
            position[i].z = y - (height * 0.5f);
            position[i].y = 0f;
        }

        postionBuffer.SetData(position);
    }

    private bool isValid()
    {
        return CullingRenderPassFeature.Instance
            && (postionBuffer != null) && (cullingResultBuffer != null) && (argBuffer != null)
            && (material != null) && (mesh != null);
    }

    double RadicalInverse_VdC(uint bits)
    {
        bits = (bits << 16) | (bits >> 16);
        bits = ((bits & 0x55555555) << 1) | ((bits & 0xAAAAAAAA) >> 1);
        bits = ((bits & 0x33333333) << 2) | ((bits & 0xCCCCCCCC) >> 2);
        bits = ((bits & 0x0F0F0F0F) << 4) | ((bits & 0xF0F0F0F0) >> 4);
        bits = ((bits & 0x00FF00FF) << 8) | ((bits & 0xFF00FF00) >> 8);
        return ((double)bits) * 2.3283064365386963e-10; // / 0x100000000
    }

    void FillArgBuffer(int subMeshIdx)
    {
        uint[] args = new uint[ARG_BUFFER_SIZE]
        {
            0,0,0,0,0
        };

        args[0] = (uint)mesh.GetIndexCount(subMeshIdx);  // index count per instance
        args[1] = (uint)0;                               // instance count
        args[2] = (uint)mesh.GetIndexStart(subMeshIdx);  // start index location
        args[3] = (uint)mesh.GetBaseVertex(subMeshIdx);  // base vertex location
        args[4] = (uint)0;

        argBuffer.SetData(args);
        
        ComputeBuffer.CopyCount(cullingResultBuffer, argBuffer, sizeof(uint));

        argBuffer.GetData(args);
        Debug.LogFormat("count = {0}", args[1]);
    }

    public bool getBuffers(out ComputeBuffer _src, out ComputeBuffer _res)
    {
        _src = postionBuffer;
        _res = cullingResultBuffer;
        return (_src != null) && (_res != null);
    }

    public float getRadius()
    {
        return 0.05f;
    }

    public bool onRender(CommandBuffer _cmd)
    {
        material.SetBuffer(Shader.PropertyToID("positionBuffer"), cullingResultBuffer);

        for (int i = 0; i < mesh.subMeshCount; ++i)
        {
            FillArgBuffer(i);

            _cmd.DrawMeshInstancedIndirect(
                mesh,
                i,
                material,
                0,
                argBuffer
            );
        }

        return true;
    }
}
