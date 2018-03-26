using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

///
// Alpha for Color Change Mask
///
public class RGB2HSV : MonoBehaviour 
{
	public Texture2D SrcRGB;

	// Use this for initialization
	void Start () 
	{
		if (SrcRGB == null)
			return;

		int width = SrcRGB.width;
		int height = SrcRGB.height;

		Texture2D resTexture = new Texture2D (width, height, TextureFormat.ARGB32, false);

		Color[] cols = SrcRGB.GetPixels();

		for(int Indx = 0; Indx < cols.Length; ++Indx)
			_RGB2HSV(ref cols[Indx]);

		resTexture.SetPixels(cols);
		resTexture.Apply();

		//save to file
		//System.IO.File.WriteAllBytes ("xxx.png", resTexture.EncodeToPNG());
	}

	void _RGB2HSV(ref Color _col)
	{
		float h = 0f;
		float s = 0f;
		float v = 0f;

		Color.RGBToHSV (_col, out h, out s, out v);

		_col.r = h;
		_col.g = s;
		_col.b = v;
	}
}
