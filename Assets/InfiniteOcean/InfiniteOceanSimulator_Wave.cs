using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class InfiniteOceanSimulator
{
	private readonly int ShaderPropertyInputTex = Shader.PropertyToID("_InputTex");
	private readonly int ShaderPropertyPrevTex = Shader.PropertyToID("_PrevTex");
	private readonly int ShaderPropertyPrev2Tex = Shader.PropertyToID("_PrevPrevTex");
	private readonly int ShaderPropertyPrevPrev2Vec = Shader.PropertyToID("_PrevPrev2Diff");

	const int PAINT_MAX = 20;	

	const int WAVE_TEX_SIZE = 1024;

	Mesh mDrawMesh;


	protected struct PaintData
	{
		public Vector3 Position;
		public float Scale;

		public PaintData(Vector3 _pos, float _scale)
		{
			Position = _pos;
			Scale = _scale;
		}
	}

	List<PaintData> mPaintList;
	List<PaintData> mRaycastList;
	
	CommandBuffer mInputCB;
	CommandBuffer mWaveCB;

	RenderTexture InputRT;
	RenderTexture RT;
	RenderTexture prevRT;
	RenderTexture prevprevRT;

	Material OutputMat;

	[System.Serializable]
	public class WaveSetting
	{
		public Transform TargetPlane;	
		public Renderer renderWater;
	}

	[SerializeField] WaveSetting mWaveSetting;	

	public Vector2 mPrevDiff = Vector2.zero;
	public Vector2 mPrev2Diff = Vector2.zero;


	void InitWave()
	{
		CreateWaveRT();

		CreateWaveDrawer();

		CreateDrawMesh();

		InitWaveList();

		InitWaterRenderer();
	}

	void ReleaseWave()
	{
		ReleaseWaveRT();

		ReleaseWaveDrawer();

		ReleaseDrawMesh();

		ReleaseWaveList();
	}

	void CreateWaveRT()
	{
		ReleaseWaveRT();

		InputRT = new RenderTexture (WAVE_TEX_SIZE,WAVE_TEX_SIZE,0,RenderTextureFormat.R8);
		RT = new RenderTexture (WAVE_TEX_SIZE,WAVE_TEX_SIZE,0,RenderTextureFormat.R8);
		RT.wrapMode = TextureWrapMode.Repeat;
		prevRT = new RenderTexture (WAVE_TEX_SIZE,WAVE_TEX_SIZE,0,RenderTextureFormat.R8);
		prevRT.wrapMode = TextureWrapMode.Repeat;
		prevprevRT = new RenderTexture (WAVE_TEX_SIZE,WAVE_TEX_SIZE,0,RenderTextureFormat.R8);
		prevprevRT.wrapMode = TextureWrapMode.Repeat;

		ClearRenderTexture (ref RT);
		ClearRenderTexture (ref prevRT);
	}

	void ReleaseWaveRT()
	{
		ReleaseRenderTexture(ref InputRT);
		ReleaseRenderTexture(ref RT);
		ReleaseRenderTexture(ref prevRT);
		ReleaseRenderTexture(ref prevprevRT);
	}

	void CreateWaveDrawer()
	{
		ReleaseWaveDrawer();

		mInputCB = new CommandBuffer ();
		mInputCB.name = "Input";

		mWaveCB = new CommandBuffer ();
		mWaveCB.name = "Wave";
	}

	void ReleaseWaveDrawer()
	{
		ReleaseCommandBuffer(ref mInputCB);

		ReleaseCommandBuffer(ref mWaveCB);
	}

	void CreateDrawMesh()
	{
		ReleaseDrawMesh();

		const float center = 0f;
		const float size = 1f;

		List<Vector3> vertices = new List<Vector3> (4);
		vertices.Add (new Vector3 (center-size, center-size, 1f));
		vertices.Add (new Vector3 (center-size, center+size, 1f));
		vertices.Add (new Vector3 (center+size, center-size, 1f));
		vertices.Add (new Vector3 (center+size, center+size, 1f));

		List<Vector2> uvs = new List<Vector2> (4);
		uvs.Add (new Vector2 (0f, 0f));
		uvs.Add (new Vector2 (0f, 1f));
		uvs.Add (new Vector2 (1f, 0f));
		uvs.Add (new Vector2 (1f, 1f));

		List<int> indices = new List<int> (6);
		indices.Add (0);
		indices.Add (1);
		indices.Add (2);
		indices.Add (3);
		indices.Add (2);
		indices.Add (1);

		mDrawMesh = new Mesh ();
		mDrawMesh.MarkDynamic ();
		mDrawMesh.SetVertices (vertices);
		mDrawMesh.SetUVs (0, uvs);
		mDrawMesh.SetTriangles (indices, 0);
	}

	void ReleaseDrawMesh()
	{
		if(mDrawMesh != null)
		{
			mDrawMesh.Clear();
			mDrawMesh = null;
		}
	}

	void InitWaveList()
	{
		ReleaseWaveList();

		mPaintList = new List<PaintData> (PAINT_MAX);

		mRaycastList = new List<PaintData> (PAINT_MAX*2);
	}

	void ReleaseWaveList()
	{
		ReleaseList<List<PaintData>>(ref mPaintList);
		ReleaseList<List<PaintData>>(ref mRaycastList);
	}

	void InitWaterRenderer()
	{
		OutputMat = mWaveSetting.renderWater.sharedMaterial;
	}

	void UpdateWave()
	{
		CheckHitWater();

		DrawPaint ();

		RunWave ();
	}

	void UpdatePrevPrev2Diff(float _diffX, float _diffZ)
	{
		Vector2 diff = new Vector2(_diffX * NORMALIZE_OCEAN_PLANE_HIT_LENGTH * 0.5f, _diffZ * NORMALIZE_OCEAN_PLANE_HIT_LENGTH * 0.5f);
		mPrev2Diff = mPrevDiff + diff;
		mPrevDiff = diff;

		mWaveDrawer.SetVector (
			ShaderPropertyPrevPrev2Vec, 
			new Vector4(mPrevDiff.x, mPrevDiff.y, mPrev2Diff.x, mPrev2Diff.y)
		);
	}

	void AddRaycasetList(Transform _pt, float _scale)
	{
		if (mRaycastList.Count == PAINT_MAX)
			return;

		mRaycastList.Add(new PaintData(_pt.position, _scale));
	}	

	void CheckHitWater()
	{
		for(int Indx = 0; Indx < mRaycastList.Count; ++Indx)
		{
			Vector2 textureCoord;
			if (CheckHitOceanPlane (mRaycastList[Indx].Position, out textureCoord) == true) 
			{
				AddPaint (textureCoord, mRaycastList[Indx].Scale);
			}
		}	

		mRaycastList.Clear();
	}

	void AddPaint(Vector2 _pt, float _scale)
	{
		if (mPaintList.Count == PAINT_MAX)
			return;

		mPaintList.Add (new PaintData(new Vector3 ((_pt.x * 2f) - 1f, (_pt.y * 2f) - 1f, 0f), _scale));
	}

	void RunWave()
	{
		RenderTexture tmp = prevprevRT;
		prevprevRT = prevRT;
		prevRT = RT;
		RT = tmp;

		mWaveDrawer.SetTexture (ShaderPropertyInputTex,    InputRT);
		mWaveDrawer.SetTexture (ShaderPropertyPrevTex ,     prevRT);
		mWaveDrawer.SetTexture (ShaderPropertyPrev2Tex, prevprevRT);

		mWaveCB.SetRenderTarget (RT);
		mWaveCB.ClearRenderTarget (true, true, Color.gray);

		mWaveCB.SetViewProjectionMatrices (Matrix4x4.identity, Matrix4x4.identity);
		mWaveCB.DrawMesh (mDrawMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one), mWaveDrawer);

		Graphics.ExecuteCommandBuffer (mWaveCB);
		mWaveCB.Clear ();
	}

	void DrawPaint()
	{	
		mInputCB.SetRenderTarget (InputRT);
		mInputCB.ClearRenderTarget (true, true, Color.clear);

		mInputCB.SetViewProjectionMatrices (Matrix4x4.identity, Matrix4x4.identity);

		for(int Indx = 0; Indx < mPaintList.Count; ++Indx)
			mInputCB.DrawMesh (mDrawMesh, Matrix4x4.TRS(mPaintList[Indx].Position, Quaternion.identity, Vector3.one * mPaintList[Indx].Scale), mInputDrawer);

		Graphics.ExecuteCommandBuffer (mInputCB);
		mInputCB.Clear ();

		mPaintList.Clear ();
	}

	#region debug
	public bool ShowDebug = true;

	private void OnGUI()
	{
		if(ShowDebug)
		{
			var h = Screen.height / 3;
			const int StrWidth = 20;
			GUI.Box(new Rect(0, 0, h, h * 3), "");
			GUI.DrawTexture(new Rect(0, 0 * h, h, h), InputRT);
			GUI.DrawTexture(new Rect(0, 1 * h, h, h), prevRT);
			GUI.DrawTexture(new Rect(0, 2 * h, h, h), prevprevRT);
			GUI.Box(new Rect(0, 1 * h - StrWidth, h, StrWidth), "INPUT");
			GUI.Box(new Rect(0, 2 * h - StrWidth, h, StrWidth), "PREV");
			GUI.Box(new Rect(0, 3 * h - StrWidth, h, StrWidth), "PREV2");
		}
	}
	#endregion
}
