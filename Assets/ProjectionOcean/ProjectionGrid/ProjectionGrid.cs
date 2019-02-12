using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ProjectionGrid
{
	Material m_Drawer;

	int ShaderID_Interpolation;

	public ProjectionGrid()
	{
		InitShaderID();
	}

	// Use this for initialization
	public void Initial (Material _drawer) 
	{
		m_Drawer = _drawer;

		CreateGrid(8);

		InitProjection();		
	}

	public void Release()
	{
		m_Drawer = null;

		ReleaseGrid();
	}

	void InitShaderID()
	{
		ShaderID_Interpolation = Shader.PropertyToID("_Interpolation");
	}

	public void Draw()
	{
		Camera cam = Camera.main;
        if (cam == null || m_GridMesh == null || m_Drawer == null) return;

        UpdateProjection(cam);

        m_Drawer.SetMatrix(ShaderID_Interpolation, Interpolation);

        //Once the camera goes below the projection plane (the ocean level) the projected
        //grid will flip the triangle winding order. 
        //Need to flip culling so the top remains the top.
        //bool isFlipped = IsFlipped;
        //m_Drawer.SetInt("_CullFace", (isFlipped) ? (int)CullMode.Front : (int)CullMode.Back);

		Graphics.DrawMesh(m_GridMesh, Matrix4x4.identity, m_Drawer, 0, cam, 0, null, false, false);				
	}
}
