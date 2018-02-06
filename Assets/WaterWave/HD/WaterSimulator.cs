using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WaterSimulator : MonoBehaviour 
{
	[SerializeField] Collider mWaterPlane;
	[SerializeField] Material mInputDrawer;
	[SerializeField] Material mWaveDrawer;

	Mesh mDrawMesh;

	List<Vector3> mPaintList;

	public RenderTexture InputRT;
	CommandBuffer mInputCB;


	public RenderTexture RT;
	public RenderTexture prevRT;
	public RenderTexture prevprevRT;
	CommandBuffer mWaveCB;

	Material OutputMat;

	const int PAINT_MAX = 20;
	const float center = 0f;
	const float size = 1f;

	public Transform Target;


	public Camera RefCam;
	public RenderTexture RefRT;
	public Renderer renderWater;


	private int ShaderPropertyInputTex;
	private int ShaderPropertyPrevTex;
	private int ShaderPropertyPrev2Tex;

	private int ShaderPropertyWaveTex;
	private int ShaderPropertyRefTex;
	private int ShaderPropertyRefVP;
	private int ShaderPropertyRefW;

	// Use this for initialization
	void Start () 
	{
		ShaderPropertyInputTex = Shader.PropertyToID("_InputTex");
		ShaderPropertyPrevTex = Shader.PropertyToID("_PrevTex");
		ShaderPropertyPrev2Tex = Shader.PropertyToID("_PrevPrevTex");

		ShaderPropertyWaveTex = Shader.PropertyToID("_WaveTex");
		ShaderPropertyRefTex = Shader.PropertyToID("_RefTex");
		ShaderPropertyRefVP = Shader.PropertyToID("_RefVP");
		ShaderPropertyRefW = Shader.PropertyToID("_RefW");

		OutputMat = renderWater.sharedMaterial;

		RefRT = new RenderTexture (Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
		RefCam.targetTexture = RefRT;

		mPaintList = new List<Vector3> (PAINT_MAX);

		InputRT = new RenderTexture (512,512,0,RenderTextureFormat.R8);
		RT = new RenderTexture (512,512,0,RenderTextureFormat.R8);
		prevRT = new RenderTexture (512,512,0,RenderTextureFormat.R8);
		prevprevRT = new RenderTexture (512,512,0,RenderTextureFormat.R8);

		InitRenderTexture (ref RT);
		InitRenderTexture (ref prevRT);

		mInputCB = new CommandBuffer ();
		mInputCB.name = "Input";

		mWaveCB = new CommandBuffer ();
		mWaveCB.name = "Wave";

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

	void OnDestroy()
	{
		mDrawMesh.Clear ();

		InputRT.Release ();
		RT.Release ();
		prevRT.Release ();
		prevprevRT.Release ();

		RefRT.Release ();

		mInputCB.Release ();
		mWaveCB.Release ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		//if (Input.GetMouseButton (0)) 
		{
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			RaycastHit hit;
			if (mWaterPlane.Raycast (ray, out hit, 1000f) == true) 
			{
				Target.position = hit.point;
				//Debug.Log ("Hit at " + hit.textureCoord);
				AddPaint (hit.textureCoord);
			}
		}	

		DrawPaint ();

		UpdateWave ();

		///
		//OutputMat.mainTexture = RT;
		///
		UpdateRef();
		///
	}

	void InitRenderTexture(ref RenderTexture _RT)
	{
		Graphics.SetRenderTarget (_RT);
		GL.Clear (true, true, Color.gray);
		Graphics.SetRenderTarget (null);
	}

	#region Draw Input
	void AddPaint(Vector2 _pt)
	{
		if (mPaintList.Count == PAINT_MAX)
			return;

		mPaintList.Add (new Vector3 ((_pt.x * 2f) - 1f, (_pt.y * 2f) - 1f, 0f));
	}

	void DrawPaint()
	{	
		mInputCB.SetRenderTarget (InputRT);
		mInputCB.ClearRenderTarget (true, true, Color.clear);

		mInputCB.SetViewProjectionMatrices (Matrix4x4.identity, Matrix4x4.identity);

		for(int Indx = 0; Indx < mPaintList.Count; ++Indx)
			mInputCB.DrawMesh (mDrawMesh, Matrix4x4.TRS(mPaintList[Indx], Quaternion.identity, Vector3.one * 0.1f), mInputDrawer);

		Graphics.ExecuteCommandBuffer (mInputCB);
		mInputCB.Clear ();
		//Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, mInputCB);

		mPaintList.Clear ();
	}
	#endregion

	#region Draw Wave
	void UpdateWave()
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
	#endregion

	#region reflect
	private void UpdateRef()
	{
		Matrix4x4 refVMatrix = RefCam.worldToCameraMatrix;
		Matrix4x4 refPMatrix = GL.GetGPUProjectionMatrix(RefCam.projectionMatrix, false);
		Matrix4x4 refVP = refPMatrix * refVMatrix;
		Matrix4x4 refW = renderWater.localToWorldMatrix;

		OutputMat.SetMatrix(ShaderPropertyRefVP, refVP);
		OutputMat.SetMatrix(ShaderPropertyRefW, refW);
		OutputMat.SetTexture(ShaderPropertyWaveTex, RT);
		OutputMat.SetTexture(ShaderPropertyRefTex, RefRT);
	}

	#endregion

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
