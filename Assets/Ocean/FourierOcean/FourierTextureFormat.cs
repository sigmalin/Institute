using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourierTextureFormat
{
    static public RenderTextureFormat GetFourierRenderTextureFormat()
    {
        RenderTextureFormat fmt = RenderTextureFormat.Default;

        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat))
            fmt = RenderTextureFormat.ARGBFloat;
        else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
            fmt = RenderTextureFormat.ARGBHalf;

        Debug.Log("FourierTextureFormat = " + fmt.ToString());
        
        return fmt;
    }

    static public TextureFormat GetFourierTextureFormat()
    {
        TextureFormat fmt = TextureFormat.ARGB32;

        if (SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat))
            fmt = TextureFormat.RGBAFloat;
        else if (SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf))
            fmt = TextureFormat.RGBAHalf;

        Debug.Log("FourierTextureFormat = " + fmt.ToString());
        
        return fmt;
    }
}
