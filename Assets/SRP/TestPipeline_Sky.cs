using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class TestPipeline
{
    void DrawSky(ScriptableRenderContext context, Camera camera)
    {
        if (camera.clearFlags == CameraClearFlags.Skybox)
            context.DrawSkybox(camera);
    }
}
