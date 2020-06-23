using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class TestPipeline
{
    void SetShadow(ScriptableRenderContext context, Camera camera)
    {
        if (camera.cameraType != CameraType.Game)
            return;

        int lightIndex = -1;

        for (int i = 0; i < cullingResults.visibleLights.Length; ++i)
        {
            VisibleLight curLight = cullingResults.visibleLights[i];
            if (curLight.lightType != LightType.Directional) continue;

            if (curLight.light.shadows == LightShadows.None) continue;
            if (curLight.light.shadowStrength <= 0f) continue;

            Bounds bounds;
            if (cullingResults.GetShadowCasterBounds(i, out bounds) == false) continue;

            lightIndex = i;
            break;
        }

        if (lightIndex < 0)
        {
            CmdBuff.SetGlobalTexture(ShadowMapID, Texture2D.whiteTexture);
            context.ExecuteCommandBuffer(CmdBuff);
            CmdBuff.Clear();
            return;
        }

        CmdBuff.SetGlobalFloat(ShadowBiasID, cullingResults.visibleLights[lightIndex].light.shadowBias);
        CmdBuff.SetGlobalFloat(ShadowStrengthID, cullingResults.visibleLights[lightIndex].light.shadowStrength);
        CmdBuff.SetGlobalVector(ShadowMapSizeID, new Vector4(invShadowMapSize, invShadowMapSize, shadowMapSize, shadowMapSize));

        if (cullingResults.visibleLights[lightIndex].light.shadows == LightShadows.Soft)
        {
            CmdBuff.EnableShaderKeyword("_SHADOWS_SOFT");
        }
        else
        {
            CmdBuff.DisableShaderKeyword("_SHADOWS_SOFT");
        }

        Matrix4x4 viewMatrix;
        Matrix4x4 projMatrix;
        ShadowSplitData shadowSplitData;

        bool res = cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                lightIndex,                                 // int activeLightIndex,
                0,                                          // int splitIndex,
                1,                                          // int splitCount,
                new Vector3(1.0f, 0.0f, 0.0f),              // Vector3 splitRatio,
                (int)setting.shadow.atlasSize,              // int shadowResolution,
                QualitySettings.shadowNearPlaneOffset,      // float shadowNearPlaneOffset,
                out viewMatrix,                             // out Matrix4x4 viewMatrix,
                out projMatrix,                             // out Matrix4x4 projMatrix,
                out shadowSplitData							// out Experimental.Rendering.ShadowSplitData shadowSplitData
            );

        if (res == false)
        {
            CmdBuff.SetGlobalTexture(ShadowMapID, Texture2D.whiteTexture);
            context.ExecuteCommandBuffer(CmdBuff);
            CmdBuff.Clear();
            return;
        }

        CoreUtils.SetRenderTarget(CmdBuff, renderBuffers[3], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.Depth);

        CmdBuff.SetViewProjectionMatrices(viewMatrix, projMatrix);

        context.ExecuteCommandBuffer(CmdBuff);
        CmdBuff.Clear();

        ShadowDrawingSettings shadowDrawingSettings = new ShadowDrawingSettings(cullingResults, lightIndex);
        shadowDrawingSettings.splitData = shadowSplitData;
        context.DrawShadows(ref shadowDrawingSettings);


        // https://docs.unity3d.com/2019.2/Documentation/Manual/SL-PlatformDifferences.html
        if (SystemInfo.usesReversedZBuffer)
        {
            projMatrix.m20 = -projMatrix.m20;
            projMatrix.m21 = -projMatrix.m21;
            projMatrix.m22 = -projMatrix.m22;
            projMatrix.m23 = -projMatrix.m23;
        }

        // [-1,1] remap to [0, 1]
        var scaleOffset = Matrix4x4.identity;
        scaleOffset.m00 = scaleOffset.m11 = scaleOffset.m22 = 0.5f;
        scaleOffset.m03 = scaleOffset.m13 = scaleOffset.m23 = 0.5f;

        CmdBuff.SetGlobalTexture(ShadowMapID, renderBuffers[3]);
        CmdBuff.SetGlobalMatrix(ShadowMatrixID, scaleOffset * (projMatrix * viewMatrix));

        context.ExecuteCommandBuffer(CmdBuff);
        CmdBuff.Clear();
    }
}
