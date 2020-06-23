using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class TestPipeline
{
    void SetCamera(ScriptableRenderContext context, Camera camera)
    {
        CmdBuff.SetGlobalVector(WorldSpaceCameraPosID, camera.transform.position);        
        context.ExecuteCommandBuffer(CmdBuff);
        CmdBuff.Clear();

        context.SetupCameraProperties(camera);
    }
}
