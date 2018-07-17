using UnityEditor;
using UnityEngine;

public class DxKiraCardEXShaderGUI : ShaderGUI 
{
	MaterialProperty _MainTex = null;
	MaterialProperty _ArtMetal = null;
	MaterialProperty _ArtGloss = null;
	MaterialProperty _ArtEmission = null;
	MaterialProperty _FrameTex = null;
	MaterialProperty _FrameNormal = null;
	MaterialProperty _FrameColor = null;
	MaterialProperty _FrameMetal = null;
	MaterialProperty _FrameGloss = null;
	MaterialProperty _KiraTex = null;
	MaterialProperty _KiraColor = null;
	MaterialProperty _KiraPower = null;
	MaterialProperty _KiraTile = null;
	MaterialProperty _KiraAngle = null;
	MaterialProperty _KiraMetal = null;
	MaterialProperty _KiraGloss = null;
	MaterialProperty _SubKiraTex = null;
	MaterialProperty _SubKiraPower = null;
	MaterialProperty _SubKiraTile = null;
	MaterialProperty _SubKiraAngle = null;
	MaterialProperty _HoloTex = null;
	MaterialProperty _HoloShift = null;
	MaterialProperty _HoloBrightness = null;
	MaterialProperty _CardDistortion = null;
	MaterialProperty _NisuTex = null;
	MaterialProperty _NisuNormal = null;
	MaterialProperty _NisuNormalPower = null;
	MaterialProperty _SkyBox = null;
	MaterialProperty _SkyBoxColor = null;
	MaterialProperty _Gradation = null;
	MaterialProperty _GradColor = null;
	MaterialProperty _GradAngle = null;
	MaterialProperty _BackFace = null;
	MaterialProperty _AmbientColor = null;
	MaterialProperty _KiraParams = null;
	MaterialProperty _SubKiraParams = null;
	MaterialProperty _GradParams = null;
	MaterialProperty _GI = null;


	public void FindProperties(MaterialProperty[] props) 
	{
		_MainTex = FindProperty("_MainTex", props, false);
		_ArtMetal = FindProperty("_ArtMetal", props, false);
		_ArtGloss = FindProperty("_ArtGloss", props, false);
		_ArtEmission = FindProperty("_ArtEmission", props, false);
		_FrameTex = FindProperty("_FrameTex", props, false);
		_FrameNormal = FindProperty("_FrameNormal", props, false);
		_FrameColor = FindProperty("_FrameColor", props, false);
		_FrameMetal = FindProperty("_FrameMetal", props, false);
		_FrameGloss = FindProperty("_FrameGloss", props, false);
		_KiraTex = FindProperty("_KiraTex", props, false);
		_KiraColor = FindProperty("_KiraColor", props, false);		
		_KiraPower = FindProperty("_KiraPower", props, false);
		_KiraTile = FindProperty("_KiraTile", props, false);
		_KiraAngle = FindProperty("_KiraAngle", props, false);
		_KiraMetal = FindProperty("_KiraMetal", props, false);
		_KiraGloss = FindProperty("_KiraGloss", props, false);
		_SubKiraTex = FindProperty("_SubKiraTex", props, false);
		_SubKiraPower = FindProperty("_SubKiraPower", props, false);
		_SubKiraTile = FindProperty("_SubKiraTile", props, false);
		_SubKiraAngle = FindProperty("_SubKiraAngle", props, false);
		_HoloTex = FindProperty("_HoloTex", props, false);
		_HoloShift = FindProperty("_HoloShift", props, false);
		_HoloBrightness = FindProperty("_HoloBrightness", props, false);
		_CardDistortion = FindProperty("_CardDistortion", props, false);
		_NisuTex = FindProperty("_NisuTex", props, false);
		_NisuNormal = FindProperty("_NisuNormal", props, false);
		_NisuNormalPower = FindProperty("_NisuNormalPower", props, false);
		_SkyBox = FindProperty("_SkyBox", props, false);
		_SkyBoxColor = FindProperty("_SkyBoxColor", props, false);
		_Gradation = FindProperty("_Gradation", props, false);
		_GradColor = FindProperty("_GradColor", props, false);
		_GradAngle = FindProperty("_GradAngle", props, false);
		_BackFace = FindProperty("_BackFace", props, false);
		_AmbientColor = FindProperty("_AmbientColor", props, false);
		_KiraParams = FindProperty("_KiraParams", props, false);
		_SubKiraParams = FindProperty("_SubKiraParams", props, false);
		_GradParams = FindProperty("_GradParams", props, false);
		_GI = FindProperty("_GI", props, false);
	}

	static GUIStyle _foldoutStyle;
    static GUIStyle foldoutStyle 
	{
        get 
		{
            if (_foldoutStyle == null) 
			{
                _foldoutStyle = new GUIStyle(EditorStyles.foldout);
                _foldoutStyle.font = EditorStyles.boldFont;
            }
            return _foldoutStyle;
        }
	}
    static GUIStyle _boxStyle;
    static GUIStyle boxStyle 
	{
        get 
		{
            if (_boxStyle == null) 
			{
                _boxStyle = new GUIStyle(EditorStyles.helpBox);
            }
            return _boxStyle;
        }
    }


	public static bool BeginFold(string foldName, bool foldState) 
	{
        EditorGUILayout.BeginVertical(boxStyle);
        GUILayout.Space(3);

        EditorGUI.indentLevel++;
        foldState = EditorGUI.Foldout(EditorGUILayout.GetControlRect(),
            foldState, " - " + foldName + " - ", true, foldoutStyle);
        EditorGUI.indentLevel--;

        if (foldState) GUILayout.Space(3);
            return foldState;
    }

    public static void EndFold() 
	{
        GUILayout.Space(3);
        EditorGUILayout.EndVertical();
        GUILayout.Space(0);
    }

	void RefreshHideProperty(Material _material)
	{
		SetKiraParmaters(_material, "Kira");
		SetKiraParmaters(_material, "SubKira");
		SetGradParmaters(_material, "Grad");

		if(_material.GetFloat("_AmbientColor") != 0)
			_material.EnableKeyword("USE_AMBIENT_COLOR");
		else
			_material.DisableKeyword("USE_AMBIENT_COLOR");

		if(_material.GetFloat("_Gradation") != 0)
			_material.EnableKeyword("USE_GRADATION");
		else
			_material.DisableKeyword("USE_GRADATION");

		if(_material.GetFloat("_GI") != 0)
			_material.EnableKeyword("USE_GI");
		else
			_material.DisableKeyword("USE_GI");
	}

	void SetKiraParmaters(Material _material, string _propertyname)
	{
		float tile = _material.GetFloat(string.Format("_{0}Tile", _propertyname));
		float invTile = tile == 0f ? 0f : 1f / tile;
		float invTileFloor = Mathf.Floor(invTile);

		float angle = _material.GetFloat(string.Format("_{0}Angle", _propertyname));
		float angle_cos = Mathf.Cos(angle * 3.141592654f);
		float angle_sin = Mathf.Sin(angle * 3.141592654f);

		_material.SetFloat(string.Format("_Inv{0}Tile", _propertyname), invTile);
		_material.SetFloat(string.Format("_Inv{0}TileFloor", _propertyname), invTileFloor);
		_material.SetFloat(string.Format("_Cos{0}Angle", _propertyname), angle_cos);
		_material.SetFloat(string.Format("_Sin{0}Angle", _propertyname), angle_sin);
	}

	void SetGradParmaters(Material _material, string _propertyname)
	{
		float angle = _material.GetFloat(string.Format("_{0}Angle", _propertyname));
		float angle_cos = Mathf.Cos(angle * 3.141592654f);
		float angle_sin = Mathf.Sin(angle * 3.141592654f);

		_material.SetFloat(string.Format("_Cos{0}Angle", _propertyname), angle_cos);
		_material.SetFloat(string.Format("_Sin{0}Angle", _propertyname), angle_sin);
	}

	bool init = false;
    public override void OnMaterialPreviewGUI(MaterialEditor materialEditor, Rect r, GUIStyle background) 
	{
        base.OnMaterialPreviewGUI(materialEditor, r, background);
        if (init) return;
        RefreshHideProperty((Material)materialEditor.target);
        init = true;
    }

	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props) 
	{
        FindProperties(props);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(-7);
        EditorGUILayout.BeginVertical();
        EditorGUI.BeginChangeCheck();
        DrawGUI(materialEditor);
        if(EditorGUI.EndChangeCheck())
		{
            var material = (Material)materialEditor.target;
            EditorUtility.SetDirty(material);
            RefreshHideProperty(material);
        }
        EditorGUILayout.EndVertical();
        GUILayout.Space(1);
        EditorGUILayout.EndHorizontal();
        //base.OnGUI(materialEditor, props);
    }

	bool mArtFold = true;
	bool mFrameFold = true;
	bool mKiraFold = true;
	bool mMainKiraFold = true;
	bool mSubKiraFold = true;
	bool mHoloFold = true;
	bool mNisuFold = true;
	bool mSkyBoxFold = true;
	bool mGradFold = true;
	bool mBackFold = true;
	bool mAmbientFold = true;

	static readonly GUIContent artDLbl = new GUIContent("Art Main Texture ");
	static readonly GUIContent artELbl = new GUIContent("Art Emission Texture ");

	static readonly GUIContent frameDLbl = new GUIContent("Frame Main Texture ");
	static readonly GUIContent frameNLbl = new GUIContent("Frame Bump Texture ");

	static readonly GUIContent kiraBLbl = new GUIContent("Kira Bump Texture ");
	static readonly GUIContent subkiraBLbl = new GUIContent("SubKira Bump Texture ");

	static readonly GUIContent holoDLbl = new GUIContent("Holo Texture ");

	static readonly GUIContent nisuDLbl = new GUIContent("Nisu Main Texture ");
	static readonly GUIContent nisuBLbl = new GUIContent("Nisu Bump Texture ");

	static readonly GUIContent skyLbl = new GUIContent("Skybox (CUBE) ");

	static readonly GUIContent backDLbl = new GUIContent("Back Face Texture ");

	public void DrawGUI(MaterialEditor materialEditor) 
	{
		mArtFold = BeginFold("Art", mArtFold);
		if(mArtFold)
		{
			materialEditor.TexturePropertySingleLine(artDLbl, _MainTex);
			materialEditor.TextureScaleOffsetProperty(_MainTex);

			GUILayout.Space(10);
			
			materialEditor.TexturePropertySingleLine(artELbl, _ArtEmission);
			materialEditor.TextureScaleOffsetProperty(_ArtEmission);

			GUILayout.Space(10);

			materialEditor.ShaderProperty(_ArtMetal, _ArtMetal.displayName);
			materialEditor.ShaderProperty(_ArtGloss, _ArtGloss.displayName);
		}
		EndFold();

		mFrameFold = BeginFold("Frame", mFrameFold);
		if(mFrameFold)
		{
			materialEditor.TexturePropertySingleLine(frameDLbl, _FrameTex);
			materialEditor.TextureScaleOffsetProperty(_FrameTex);

			GUILayout.Space(10);
			
			materialEditor.TexturePropertySingleLine(frameNLbl, _FrameNormal);
			materialEditor.TextureScaleOffsetProperty(_FrameNormal);

			GUILayout.Space(10);

			materialEditor.ShaderProperty(_FrameColor, _FrameColor.displayName);
			materialEditor.ShaderProperty(_FrameMetal, _FrameMetal.displayName);
			materialEditor.ShaderProperty(_FrameGloss, _FrameGloss.displayName);
		}
		EndFold();

		mKiraFold = BeginFold("Kira", mKiraFold);
		if(mKiraFold)
		{

			mMainKiraFold = BeginFold("Main Kira", mMainKiraFold);
			if(mMainKiraFold)
			{
				materialEditor.TexturePropertySingleLine(kiraBLbl, _KiraTex);
				materialEditor.TextureScaleOffsetProperty(_KiraTex);

				GUILayout.Space(10);

				materialEditor.ShaderProperty(_KiraPower, _KiraPower.displayName);
				materialEditor.ShaderProperty(_KiraTile, _KiraTile.displayName);
				materialEditor.ShaderProperty(_KiraAngle, _KiraAngle.displayName);

			}
			EndFold();

			mSubKiraFold = BeginFold("Sub Kira", mSubKiraFold);
			if(mSubKiraFold)
			{
				materialEditor.TexturePropertySingleLine(subkiraBLbl, _SubKiraTex);
				materialEditor.TextureScaleOffsetProperty(_SubKiraTex);

				GUILayout.Space(10);

				materialEditor.ShaderProperty(_SubKiraPower, _SubKiraPower.displayName);
				materialEditor.ShaderProperty(_SubKiraTile, _SubKiraTile.displayName);
				materialEditor.ShaderProperty(_SubKiraAngle, _SubKiraAngle.displayName);
			}
			EndFold();

			GUILayout.Space(10);

			materialEditor.ShaderProperty(_KiraColor, _KiraColor.displayName);
			materialEditor.ShaderProperty(_KiraMetal, _KiraMetal.displayName);
			materialEditor.ShaderProperty(_KiraGloss, _KiraGloss.displayName);			
		}
		EndFold();

		mHoloFold = BeginFold("Holo", mHoloFold);
		if(mHoloFold)
		{
			materialEditor.TexturePropertySingleLine(holoDLbl, _HoloTex);
			materialEditor.TextureScaleOffsetProperty(_HoloTex);

			GUILayout.Space(10);

			materialEditor.ShaderProperty(_HoloShift, _HoloShift.displayName);
			materialEditor.ShaderProperty(_HoloBrightness, _HoloBrightness.displayName);
			materialEditor.ShaderProperty(_CardDistortion, _CardDistortion.displayName);
		}
		EndFold();		

		mNisuFold = BeginFold("Nisu", mNisuFold);
		if(mNisuFold)
		{
			materialEditor.TexturePropertySingleLine(nisuDLbl, _NisuTex);
			materialEditor.TextureScaleOffsetProperty(_NisuTex);

			GUILayout.Space(10);

			materialEditor.TexturePropertySingleLine(nisuBLbl, _NisuNormal);
			materialEditor.TextureScaleOffsetProperty(_NisuNormal);

			GUILayout.Space(10);

			materialEditor.ShaderProperty(_NisuNormalPower, _NisuNormalPower.displayName);			
		}
		EndFold();			

		mSkyBoxFold = BeginFold("SkyBox", mSkyBoxFold);
		if(mSkyBoxFold)
		{
			materialEditor.TexturePropertySingleLine(skyLbl, _SkyBox);
			materialEditor.ShaderProperty(_SkyBoxColor, _SkyBoxColor.displayName);						
		}
		EndFold();		

		mGradFold = BeginFold("Gradation", mGradFold);
		if(mGradFold)
		{
			materialEditor.ShaderProperty(_Gradation, _Gradation.displayName);			
			if(_Gradation.floatValue != 0f)
			{
				materialEditor.ShaderProperty(_GradColor, _GradColor.displayName);
				materialEditor.ShaderProperty(_GradAngle, _GradAngle.displayName);
			}							
		}
		EndFold();	

		mBackFold = BeginFold("BackFace", mBackFold);
		if(mBackFold)
		{
			materialEditor.TexturePropertySingleLine(backDLbl, _BackFace);
			materialEditor.TextureScaleOffsetProperty(_BackFace);						
		}
		EndFold();

		mAmbientFold = BeginFold("Other", mAmbientFold);
		if(mAmbientFold)
		{
			materialEditor.ShaderProperty(_AmbientColor, _AmbientColor.displayName);					
			materialEditor.ShaderProperty(_GI, _GI.displayName);
		}
		EndFold();
	}
}
