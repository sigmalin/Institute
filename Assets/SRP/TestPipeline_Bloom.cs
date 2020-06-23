using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class TestPipeline
{
    Material matBloom;

    protected int[] BloomBufferIDs;

    const int MAX_BLOOM_BUFFER_SIZE = 4;

    void InitBloom()
    {
        BloomBufferIDs = new int[MAX_BLOOM_BUFFER_SIZE];
        for (int i = 0; i < MAX_BLOOM_BUFFER_SIZE; ++i)
            BloomBufferIDs[i] = Shader.PropertyToID(string.Format("_BloomBufferRT{0}", i));

        matBloom = Resources.Load<Material>("SRP_PostEffect_Bloom");

        matBloom.SetFloat("_Intensity", setting.Bloom.intensity);
        matBloom.SetVector("_Filter", setting.Bloom.GetFiliterParameters());
    }

    void ApplyBloom(CommandBuffer _cmd, RenderTargetIdentifier _src)
    {
        int width = Screen.width;
        int height = Screen.height;

        for(int i = 0; i < MAX_BLOOM_BUFFER_SIZE; ++i)
        {
            width >>= 1;
            height >>= 1;

            _cmd.GetTemporaryRT(BloomBufferIDs[i], width, height, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR);
        }

        _cmd.Blit(_src, BloomBufferIDs[0], matBloom, 0);

        for (int i = 1; i < MAX_BLOOM_BUFFER_SIZE; ++i)
        {
            CmdBuff.Blit(BloomBufferIDs[0], BloomBufferIDs[i], matBloom, 1);
        }

        for (int i = MAX_BLOOM_BUFFER_SIZE - 1; 0 < i; --i)
        {
            CmdBuff.Blit(BloomBufferIDs[i], BloomBufferIDs[i - 1], matBloom, 2);
        }

        CmdBuff.Blit(BloomBufferIDs[0], _src, matBloom, 3);

        for (int i = 0; i < MAX_BLOOM_BUFFER_SIZE; ++i)
        {
            _cmd.ReleaseTemporaryRT(BloomBufferIDs[i]);
        }
    }
}
