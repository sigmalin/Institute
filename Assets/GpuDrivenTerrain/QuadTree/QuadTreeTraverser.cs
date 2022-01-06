using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class QuadTreeTraverser
{
    struct QuadTreeNode
    {
        public uint x, y;
    }

    int kernelTraverseQuadTree;

    int CameraPosShaderID;
    int ConsumeNodeListShaderID;
    int AppendNodeListShaderID;
    int AppendFinalNodeListShaderID;
    int LengthOfLod0ShaderID;
    int MaxLODShaderID;
    int ConsumeNodeCountShaderID;
    int PassLODShaderID;

    int NodeSizeAtMaxLodID;
    int NodeDescriptorsShaderID;

    GraphicsBuffer ConsumeNodeList;
    GraphicsBuffer AppendNodeList;
    GraphicsBuffer AppendFinalNodeList;
    GraphicsBuffer CounterBuffer;
    GraphicsBuffer NodeDescriptorBuffer;

    QuadTreeSetting Setting;

    public QuadTreeTraverser(QuadTreeSetting setting)
    {
        Setting = setting;

        if (Setting != null && Setting.TraverserCS != null)
        {
            kernelTraverseQuadTree = Setting.TraverserCS.FindKernel("TraverseQuadTree");

            CameraPosShaderID = Shader.PropertyToID("CameraPos");
            ConsumeNodeListShaderID = Shader.PropertyToID("ConsumeNodeList");
            AppendNodeListShaderID = Shader.PropertyToID("AppendNodeList");
            AppendFinalNodeListShaderID = Shader.PropertyToID("AppendFinalNodeList");
            LengthOfLod0ShaderID = Shader.PropertyToID("LengthOfLod0");
            MaxLODShaderID = Shader.PropertyToID("MaxLOD");
            ConsumeNodeCountShaderID = Shader.PropertyToID("ConsumeNodeCount");
            PassLODShaderID = Shader.PropertyToID("PassLOD");

            NodeSizeAtMaxLodID = Shader.PropertyToID("NodeSizeAtMaxLOD");
            NodeDescriptorsShaderID = Shader.PropertyToID("NodeDescriptors");
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
        return Setting != null && Setting.TraverserCS != null &&
                ConsumeNodeList != null &&
                AppendNodeList != null &&
                AppendFinalNodeList != null &&
                CounterBuffer != null &&
                NodeDescriptorBuffer != null;
    }

    void InitGraphicsBuffer()
    {
        ReleaseGraphicsBuffer();

        int maxCount = Setting.MaxNodeCount;
        int maxDescCount = Setting.GetDescriptorCount();

        ConsumeNodeList = new GraphicsBuffer(GraphicsBuffer.Target.Append, maxCount, sizeof(uint) * 2);
        AppendNodeList = new GraphicsBuffer(GraphicsBuffer.Target.Append, maxCount, sizeof(uint) * 2);
        AppendFinalNodeList = new GraphicsBuffer(GraphicsBuffer.Target.Append, maxCount, sizeof(uint) * 3);
        CounterBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, sizeof(int));
        NodeDescriptorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxDescCount, sizeof(uint));
    }

    void ReleaseGraphicsBuffer()
    {
        if (ConsumeNodeList != null)
        {
            ConsumeNodeList.Release();
            ConsumeNodeList.Dispose();
            ConsumeNodeList = null;
        }

        if (AppendNodeList != null)
        {
            AppendNodeList.Release();
            AppendNodeList.Dispose();
            AppendNodeList = null;
        }

        if (AppendFinalNodeList != null)
        {
            AppendFinalNodeList.Release();
            AppendFinalNodeList.Dispose();
            AppendFinalNodeList = null;
        }

        if (CounterBuffer != null)
        {
            CounterBuffer.Release();
            CounterBuffer.Dispose();
            CounterBuffer = null;
        }

        if (NodeDescriptorBuffer != null)
        {
            NodeDescriptorBuffer.Release();
            NodeDescriptorBuffer.Dispose();
            NodeDescriptorBuffer = null;
        }
    }

    void ResetGraphicsBuffer(int nodeSize)
    {
        ConsumeNodeList.SetCounterValue(0);
        AppendNodeList.SetCounterValue(0);
        AppendFinalNodeList.SetCounterValue(0);

        QuadTreeNode[] nodes = new QuadTreeNode[nodeSize * nodeSize];
        int index = 0;
        for (int i = 0; i < nodeSize; ++i)
        {
            for (int j = 0; j < nodeSize; ++j)
            {
                nodes[index].x = (uint)j;
                nodes[index].y = (uint)i;
                ++index;
            }
        }

        ConsumeNodeList.SetData(nodes);
        ConsumeNodeList.SetCounterValue((uint)nodes.Length);
    }

    void SwapGraphicsBuffer()
    {
        GraphicsBuffer swap = ConsumeNodeList;
        ConsumeNodeList = AppendNodeList;
        AppendNodeList = swap;
    }

    int GetBufferCount(GraphicsBuffer buffer)
    {
        if (buffer == null || buffer == CounterBuffer) return 0;

        GraphicsBuffer.CopyCount(buffer, CounterBuffer, 0);

        int[] counter = new int[1];
        CounterBuffer.GetData(counter);

        return counter[0];
    }

    void TraverseQuadTree(int ConsumeNodeCount, int PassLOD)
    {
        Setting.TraverserCS.SetVector(CameraPosShaderID, Camera.main.transform.position);

        Setting.TraverserCS.SetBuffer(kernelTraverseQuadTree, ConsumeNodeListShaderID, ConsumeNodeList);
        Setting.TraverserCS.SetBuffer(kernelTraverseQuadTree, AppendNodeListShaderID, AppendNodeList);
        Setting.TraverserCS.SetBuffer(kernelTraverseQuadTree, AppendFinalNodeListShaderID, AppendFinalNodeList);
        Setting.TraverserCS.SetBuffer(kernelTraverseQuadTree, NodeDescriptorsShaderID, NodeDescriptorBuffer);

        Setting.TraverserCS.SetInt(LengthOfLod0ShaderID, Setting.LengthOfLod0);
        Setting.TraverserCS.SetInt(MaxLODShaderID, Setting.MaxLOD);

        Setting.TraverserCS.SetInt(ConsumeNodeCountShaderID, ConsumeNodeCount);
        Setting.TraverserCS.SetInt(PassLODShaderID, PassLOD);

        Setting.TraverserCS.SetInt(NodeSizeAtMaxLodID, Setting.NodeSizeAtMaxLOD);

        uint sizeX;
        Setting.TraverserCS.GetKernelThreadGroupSizes(
            kernelTraverseQuadTree,
            out sizeX,
            out _,
            out _
        );

        Setting.TraverserCS.Dispatch(kernelTraverseQuadTree, Mathf.CeilToInt((ConsumeNodeCount + sizeX - 1) / sizeX), 1, 1);
    }

    public int Traverse(out GraphicsBuffer buffer)
    {
        buffer = null;

        if (isValid() == false) return 0;

        ResetGraphicsBuffer(Setting.NodeSizeAtMaxLOD);

        int passLOD = Setting.MaxLOD;
        int count = GetBufferCount(ConsumeNodeList);
        while (0 < count)
        {
            TraverseQuadTree(count, passLOD);

            SwapGraphicsBuffer();

            count = GetBufferCount(ConsumeNodeList);
            passLOD -= 1;
        }

        buffer = AppendFinalNodeList;

        return GetBufferCount(buffer);
    }
}
