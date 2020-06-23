using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class TestPipeline
{
    const string COMMAND_BUFFER_NAME = "Render Camera";

    protected CommandBuffer CmdBuff;

    protected ShaderTagId[] shaderTagIds;

    protected int RenderBuffer1ID;

    protected int RenderBuffer2ID;

    protected int DepthBufferID;

    protected int ShadowBufferID;    

    protected int LightColorID;

    protected int LightDirectionID;

    protected int ShadowMapID;

    protected int ShadowMatrixID;

    protected int ShadowBiasID;

    protected int ShadowStrengthID;

    protected int ShadowMapSizeID;

    protected int WorldSpaceCameraPosID;

    RenderTargetIdentifier[] renderBuffers;

    CullingResults cullingResults;

    void Initialize()
    {
        CmdBuff = new CommandBuffer()
        {
            name = COMMAND_BUFFER_NAME,
        };

        shaderTagIds = new ShaderTagId[]
        {
            new ShaderTagId("DepthOnly"),
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("ForwardBase"),
        };

        RenderBuffer1ID = Shader.PropertyToID("_RenderBuffer1RT");

        RenderBuffer2ID = Shader.PropertyToID("_RenderBuffer2RT");

        DepthBufferID = Shader.PropertyToID("_DepthBufferRT");

        ShadowBufferID = Shader.PropertyToID("_ShadowBufferRT");
                
        LightColorID = Shader.PropertyToID("_LightColor");

        LightDirectionID = Shader.PropertyToID("_LightDirection");

        ShadowMapID = Shader.PropertyToID("_ShadowMap");

        ShadowMatrixID = Shader.PropertyToID("_ShadowMatrixVP");

        ShadowBiasID = Shader.PropertyToID("_ShadowBias");

        ShadowStrengthID = Shader.PropertyToID("_ShadowStrength");

        ShadowMapSizeID = Shader.PropertyToID("_ShadowMapSize");

        WorldSpaceCameraPosID = Shader.PropertyToID("_WorldSpaceCameraPos");

        InitToneMapping();

        InitFXAA();

        InitBloom();
    }
}
