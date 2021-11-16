using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public interface IGPUInstParticle
{
    bool onRender(CommandBuffer _cmd);
}
