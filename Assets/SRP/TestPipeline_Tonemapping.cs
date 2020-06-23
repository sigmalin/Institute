using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class TestPipeline
{
    protected Material matToneMapping;

    void InitToneMapping()
    {        
        matToneMapping = Resources.Load<Material>("SRP_PostEffect_Tonemapping");

        matToneMapping.SetFloat("_Exposure", setting.Exposure);
    }

    void ApplyToneMapping(CommandBuffer _cmd, RenderTargetIdentifier _src, RenderTargetIdentifier _dest)
    {
        _cmd.Blit(_src, _dest, matToneMapping);
    }
}
