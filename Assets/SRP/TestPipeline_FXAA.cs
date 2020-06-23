using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class TestPipeline
{
    Material matFXAA;

    void InitFXAA()
    {
        matFXAA = Resources.Load<Material>("SRP_PostEffect_FXAA");

        matFXAA.SetFloat("_ContrastThreshold", setting.Fxaa.GetContrastThreshold());
        matFXAA.SetFloat("_RelativeThreshold", setting.Fxaa.GetRelativeThreshold());
        matFXAA.SetFloat("_SubpixelBlending" , setting.Fxaa.SubPixelBlending);
    }

    void ApplyFXAA(CommandBuffer _cmd, RenderTargetIdentifier _src, RenderTargetIdentifier _dest)
    {
        _cmd.Blit(_src, _dest, matFXAA);
    }
}
