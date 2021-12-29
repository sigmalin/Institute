using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public interface IHizCullingBoxObject
{
    bool getBuffers(out ComputeBuffer _src, out ComputeBuffer _res);
    bool getBounds(out Vector3 _max, out Vector3 _min);
    bool onRender(CommandBuffer _cmd);
}
