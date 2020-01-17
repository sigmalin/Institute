using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public interface IFourierOceanCore
{
    void Init(int _fourierSize);
    void Perform(Texture2D _Spectrum0, Texture2D _Omega, out RenderTexture _normal, out RenderTexture _displacement);
}
