using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public interface IGpuDrivenTerrain
{
    bool onCulling(CommandBuffer _cmd, Camera _cam);
    bool onRender(CommandBuffer _cmd);
}
