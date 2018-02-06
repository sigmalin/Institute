using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour 
{
	public Texture2D FromHSV;
	public Material Target;

	Texture2D ToRGB;

	// Use this for initialization
	void Start () 
	{
		ToRGB = new Texture2D (FromHSV.width, FromHSV.height, TextureFormat.ARGB32, false);

		OnChange ();
	}

	void OnDestroy()
	{
		if (ToRGB != null) 
		{
			Destroy (ToRGB);
			ToRGB = null;
		}
	}

	float hSliderValue = 0f;
	void OnGUI()
	{
		hSliderValue = GUI.HorizontalSlider (new Rect(20,20,359,30), hSliderValue, 0f, 1f);
		if (GUI.Button (new Rect (20, 80, 60, 30), "Change"))
			OnChange();
	}

	void OnChange()
	{
		Color[] src = FromHSV.GetPixels();

		for(int Indx = 0; Indx < src.Length; ++Indx)
		{
			InvTransHSV (ref src[Indx], hSliderValue);
		}

		ToRGB.SetPixels(src);
		ToRGB.Apply();

		Target.mainTexture = ToRGB;
	}

	void InvTransHSV(ref Color _color, float _diffHue)
	{
		float h = _color.r;

		if (0f < _color.a) 
		{
			h += _diffHue;
			h -= Mathf.Floor (h);
		}

		_color = Color.HSVToRGB (h, _color.g, _color.b);
	}
}
