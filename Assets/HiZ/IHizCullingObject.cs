using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public interface IHizCullingObject
{
    bool getBuffers(out ComputeBuffer _src, out ComputeBuffer _res);
    float getRadius();
    bool onRender(CommandBuffer _cmd);
}
