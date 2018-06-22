using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UiBlendSimulator : MonoBehaviour 
{
	public RenderTexture RT;

	public Material BlendDrawer;
	public Material BlendDrawerFinal;

	CommandBuffer mCmdUiBlend;
	Mesh mMesh;

	// Use this for initialization
	void Start () 
	{
		InitRT();
		InitMesh();
		InitFinalOutput();
		InitBlendCommand();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(mCmdUiBlend != null)
		{
			Graphics.ExecuteCommandBuffer(mCmdUiBlend);

			mCmdUiBlend.Clear();
			mCmdUiBlend = null;	
		}	
	}

	void InitRT()
	{
		RT = new RenderTexture(512, 512, 0, RenderTextureFormat.Default);	
	}

	void InitMesh()
	{
		List<Vector3> vertices = new List<Vector3>();
		vertices.Add(new Vector3(-1f,-1f, 0f));
		vertices.Add(new Vector3(-1f, 1f, 0f));
		vertices.Add(new Vector3( 1f,-1f, 0f));
		vertices.Add(new Vector3( 1f, 1f, 0f));

		List<Vector2> uvs = new List<Vector2>();
		uvs.Add(new Vector2( 0f, 0f));
		uvs.Add(new Vector2( 0f, 1f));
		uvs.Add(new Vector2( 1f, 0f));
		uvs.Add(new Vector2( 1f, 1f));

		List<int> indices = new List<int>();
		indices.Add(0);
		indices.Add(1);
		indices.Add(2);
		indices.Add(3);
		indices.Add(2);
		indices.Add(1);

		mMesh = new Mesh();
		mMesh.SetVertices(vertices);
		mMesh.SetUVs(0, uvs);
		mMesh.SetTriangles(indices, 0);
	}

	void InitFinalOutput()
	{
		CommandBuffer final = new CommandBuffer();
		
		final.Blit(RT, BuiltinRenderTextureType.CurrentActive, BlendDrawerFinal);

		Camera.main.AddCommandBuffer(CameraEvent.AfterSkybox, final);
	}

	void InitBlendCommand()
	{
		mCmdUiBlend = new CommandBuffer();

		mCmdUiBlend.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);

		mCmdUiBlend.SetRenderTarget(RT);
		mCmdUiBlend.ClearRenderTarget(true, true, Color.black);

		mCmdUiBlend.DrawMesh(mMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one*0.25f), BlendDrawer);
		mCmdUiBlend.DrawMesh(mMesh, Matrix4x4.TRS(Vector3.up * 0.25f, Quaternion.identity, Vector3.one*0.25f), BlendDrawer);
	}
}
