// https://toropippi.livedoor.blog/archives/54817221.html
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BitonicSort
{
    readonly float MIN_FLOAT = float.MinValue;
    public struct BitonicSortData
    {
        public float key;
        public uint index;
    }

    public enum SortMethod
    {
        fastest,
        NoUsedSharedMemory,
        normal,
    }

    ComputeShader cs;

    int kernelParallelBitonic_B16;
    int kernelParallelBitonic_B8;
    int kernelParallelBitonic_B4;
    int kernelParallelBitonic_B2;
    int kernelParallelBitonic_C4;
    int kernelParallelBitonic_C2;

    public BitonicSort()
    {
        Init();
    }

    void Init()
    {
        cs = Resources.Load<ComputeShader>("BitonicSort");
        if(cs)
        {
            kernelParallelBitonic_B16 = cs.FindKernel("ParallelBitonic_B16");
            kernelParallelBitonic_B8 = cs.FindKernel("ParallelBitonic_B8");
            kernelParallelBitonic_B4 = cs.FindKernel("ParallelBitonic_B4");
            kernelParallelBitonic_B2 = cs.FindKernel("ParallelBitonic_B2");
            kernelParallelBitonic_C4 = cs.FindKernel("ParallelBitonic_C4");
            kernelParallelBitonic_C2 = cs.FindKernel("ParallelBitonic_C2");
        }
    }

    int CreateBuffer(float[] _datas, out GraphicsBuffer _buffer)
    {
        int len = Mathf.NextPowerOfTwo(_datas.Length);

        BitonicSortData[] stream = new BitonicSortData[len];
        for(int i = 0; i < _datas.Length; ++i)
        {
            stream[i].index = (uint)i;
            stream[i].key = _datas[i];
        }

        for (int i = _datas.Length; i < len; ++i)
        {
            stream[i].index = (uint)i;
            stream[i].key = MIN_FLOAT;
        }

        _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, len, System.Runtime.InteropServices.Marshal.SizeOf(stream[0]));
        _buffer.SetData(stream);
        return len;
    }

    public bool Sort(float[] _datas, SortMethod _method)
    {
        if (_datas.Length < 2) return false;
        if (!cs) return false;

        GraphicsBuffer buffer;
        int count = CreateBuffer(_datas, out buffer);

        System.Action<GraphicsBuffer, int> process;
        switch(_method)
        {
            case SortMethod.fastest:
                process = Bitonic_fastest;
                break;
            case SortMethod.NoUsedSharedMemory:
                process = Bitonic_NoUseSharedMemort;
                break;
            default:
                process = Bitonic_fastest;
                break;
        }

        process(buffer, count);
        BitonicSortData[] stream = new BitonicSortData[count];

        buffer.GetData(stream);
        buffer.Release();

        for(int i = 0; i < _datas.Length; ++i)
        {
            _datas[i] = stream[i].key;
        }

        return true;
    }

    void Bitonic_fastest(GraphicsBuffer _buffer, int _count)
    {
        cs.SetBuffer(kernelParallelBitonic_B16, "data", _buffer);
        cs.SetBuffer(kernelParallelBitonic_B8, "data", _buffer);
        cs.SetBuffer(kernelParallelBitonic_B4, "data", _buffer);
        cs.SetBuffer(kernelParallelBitonic_B2, "data", _buffer);
        cs.SetBuffer(kernelParallelBitonic_C4, "data", _buffer);
        cs.SetBuffer(kernelParallelBitonic_C2, "data", _buffer);

        int nlog = (int)(Mathf.Log(_count, 2));
        int B_indx, inc;
        int kernel_id;

        for (int i = 0; i < nlog; i++)
        {
            inc = 1 << i;
            for (int j = 0; j < i + 1; j++)
            {
                if (inc <= 128) break;

                if (inc >= 2048)
                {
                    B_indx = 16;
                    kernel_id = kernelParallelBitonic_B16;
                }
                else if (inc >= 1024)
                {
                    B_indx = 8;
                    kernel_id = kernelParallelBitonic_B8;
                }
                else if (inc >= 512)
                {
                    B_indx = 4;
                    kernel_id = kernelParallelBitonic_B4;
                }
                else
                {
                    B_indx = 2;
                    kernel_id = kernelParallelBitonic_B2;
                }

                uint sizeX;
                cs.GetKernelThreadGroupSizes(
                    kernel_id,
                    out sizeX,
                    out _,
                    out _
                );

                cs.SetInt("inc", inc * 2 / B_indx);
                cs.SetInt("dir", 2 << i);
                cs.Dispatch(kernel_id, _count / B_indx / (int)sizeX, 1, 1);
                inc /= B_indx;
            }

            cs.SetInt("inc0", inc);
            cs.SetInt("dir", 2 << i);
            if ((inc == 8) | (inc == 32) | (inc == 128))
            {
                cs.Dispatch(kernelParallelBitonic_C4, _count / 4 / 64, 1, 1);
            }
            else
            {
                cs.Dispatch(kernelParallelBitonic_C2, _count / 2 / 128, 1, 1);
            }
        }
    }

    void Bitonic_NoUseSharedMemort(GraphicsBuffer _buffer, int _count)
    {
        cs.SetBuffer(kernelParallelBitonic_B16, "data", _buffer);
        cs.SetBuffer(kernelParallelBitonic_B8, "data", _buffer);
        cs.SetBuffer(kernelParallelBitonic_B4, "data", _buffer);
        cs.SetBuffer(kernelParallelBitonic_B2, "data", _buffer);

        int nlog = (int)(Mathf.Log(_count, 2));
        int B_indx, inc;
        int kernel_id;

        for (int i = 0; i < nlog; i++)
        {
            inc = 1 << i;
            for (int j = 0; j < i + 1; j++)
            {
                if (inc == 0) break;

                if ((inc >= 8) & (nlog >= 10))
                {
                    B_indx = 16;
                    kernel_id = kernelParallelBitonic_B16;
                }
                else if ((inc >= 4) & (nlog >= 9))
                {
                    B_indx = 8;
                    kernel_id = kernelParallelBitonic_B8;
                }
                else if ((inc >= 2) & (nlog >= 8))
                {
                    B_indx = 4;
                    kernel_id = kernelParallelBitonic_B4;
                }
                else
                {
                    B_indx = 2;
                    kernel_id = kernelParallelBitonic_B2;
                }

                uint sizeX;
                cs.GetKernelThreadGroupSizes(
                    kernel_id,
                    out sizeX,
                    out _,
                    out _
                );

                cs.SetInt("inc", inc * 2 / B_indx);
                cs.SetInt("dir", 2 << i);
                cs.Dispatch(kernel_id, _count / B_indx / (int)sizeX, 1, 1);
                inc /= B_indx;
            }
        }
    }

    void Bitonic_normal(GraphicsBuffer _buffer, int _count)
    {
        cs.SetBuffer(kernelParallelBitonic_B2, "data", _buffer);

        int nlog = (int)(Mathf.Log(_count, 2));
        int B_indx, inc;

        uint sizeX;
        cs.GetKernelThreadGroupSizes(
            kernelParallelBitonic_B2,
            out sizeX,
            out _,
            out _
        );

        for (int i = 0; i < nlog; i++)
        {
            inc = 1 << i;
            for (int j = 0; j < i + 1; j++)
            {
                B_indx = 2;
                cs.SetInt("inc", inc * 2 / B_indx);
                cs.SetInt("dir", 2 << i);
                cs.Dispatch(kernelParallelBitonic_B2, _count / B_indx / (int)sizeX, 1, 1);
                inc /= B_indx;
            }
        }
    }
}
