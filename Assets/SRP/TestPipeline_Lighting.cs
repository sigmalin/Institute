using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class TestPipeline
{
    void SetLighting(ScriptableRenderContext context, Camera camera)
    {
        int lightIndex = -1;

        for (int i = 0; i < cullingResults.visibleLights.Length; ++i)
        {
            VisibleLight curLight = cullingResults.visibleLights[i];
            if (curLight.lightType != LightType.Directional) continue;

            lightIndex = i;
            break;
        }

        if (lightIndex < 0)
        {
            CmdBuff.DisableShaderKeyword("ENABLE_DIRECTIONAL_LIGHT");
            context.ExecuteCommandBuffer(CmdBuff);
            CmdBuff.Clear();
            return;
        }

        CmdBuff.EnableShaderKeyword("ENABLE_DIRECTIONAL_LIGHT");
        CmdBuff.SetGlobalColor(LightColorID, cullingResults.visibleLights[lightIndex].finalColor);

        Vector4 lightDirection = cullingResults.visibleLights[lightIndex].localToWorldMatrix.GetColumn(2); // cullingResults.visibleLights[lightIndex].light.transform.forward
        lightDirection.x *= -1f;
        lightDirection.y *= -1f;
        lightDirection.z *= -1f;
        CmdBuff.SetGlobalVector(LightDirectionID, lightDirection);

        context.ExecuteCommandBuffer(CmdBuff);
        CmdBuff.Clear();
    }
}
