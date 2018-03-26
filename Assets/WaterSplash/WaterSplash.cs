using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WaterSplash : MonoBehaviour 
{
	Mesh mMesh;
	[SerializeField] Material mMaterial;

	const int WATERSPLASH_MAX = 512;
	const float f3 = 0.292893218813452f;
	const float f7 = 0.707106781186548f;

	List<Vector3> mVertices;
	List<Vector2> mUVs;
	List<Vector2> mUV2s;
	List<int> mIndices;

	protected class SplashData
	{
		public Vector3 Pos { get; set; }
		public float Time { get; set; }
		public float Rotate { get; set; }
	}

	Queue<SplashData> mSplashData;
	Stack<SplashData> mFreeSplashData;

	float mCurTime;
	bool mRefresh;

	readonly Vector2[] UV_MAPS = new Vector2[] {
		new Vector2(f3, 0f),
		new Vector2(f7, 0f),
		new Vector2(0f, f3),
		new Vector2(1f, f3),
		new Vector2(0f, f7),
		new Vector2(1f, f7),
		new Vector2(f3, 1f),
		new Vector2(f7, 1f),
	};

	readonly float PI_2 = Mathf.PI * 2f;
	readonly int ShaderPropertyCurrentTime = Shader.PropertyToID("_CurrentTime");

	// Use this for initialization
	void Start () 
	{
		Initialize();
	}

	void Initialize ()
	{
		mMesh = new Mesh();
		mMesh.name = "SplashMesh";
		mMesh.MarkDynamic();

		mVertices = new List<Vector3>(WATERSPLASH_MAX*8);
		mUVs      = new List<Vector2>(WATERSPLASH_MAX*8);
		mUV2s     = new List<Vector2>(WATERSPLASH_MAX*8);
		mIndices  = new List<int>(WATERSPLASH_MAX*18);

		mSplashData = new Queue<SplashData>(WATERSPLASH_MAX);
		mFreeSplashData = new Stack<SplashData>(WATERSPLASH_MAX);
		for(int Indx = 0; Indx < WATERSPLASH_MAX; ++Indx)
		{
			mFreeSplashData.Push(new SplashData());
		}

		mRefresh = false;

		this.GetComponent<MeshFilter>().sharedMesh = mMesh;	
		this.GetComponent<MeshRenderer>().sharedMaterial = mMaterial;
	}

	void CreateMesh()
	{
		if(mRefresh == false)
			return;

		ClearMeshData();

		int dataIndx = 0;

		Queue<SplashData>.Enumerator etor = mSplashData.GetEnumerator();		
		while(etor.MoveNext())
		{
			for(int Indx = 0; Indx < 8; ++Indx)
			{
				mVertices.Add(etor.Current.Pos);
				mUVs.Add(UV_MAPS[Indx]);
				mUV2s.Add(new Vector2(etor.Current.Time, etor.Current.Rotate));
			}

			mIndices.Add(dataIndx*8+0);
			mIndices.Add(dataIndx*8+1);
			mIndices.Add(dataIndx*8+3);
			mIndices.Add(dataIndx*8+2);
			mIndices.Add(dataIndx*8+0);
			mIndices.Add(dataIndx*8+3);
			mIndices.Add(dataIndx*8+2);
			mIndices.Add(dataIndx*8+3);
			mIndices.Add(dataIndx*8+5);
			mIndices.Add(dataIndx*8+2);
			mIndices.Add(dataIndx*8+5);
			mIndices.Add(dataIndx*8+4);
			mIndices.Add(dataIndx*8+4);
			mIndices.Add(dataIndx*8+5);
			mIndices.Add(dataIndx*8+7);
			mIndices.Add(dataIndx*8+4);
			mIndices.Add(dataIndx*8+7);
			mIndices.Add(dataIndx*8+6);

			++dataIndx;
		}

		mMesh.Clear();

		mMesh.SetVertices(mVertices);
		mMesh.SetUVs(0, mUVs);
		mMesh.SetUVs(1, mUV2s);
		mMesh.SetTriangles(mIndices, 0, false);

		mMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 99999999f);

		mRefresh = false;
	}

	void ClearMeshData()
	{
		mVertices.Clear();
		mUVs.Clear();
		mUV2s.Clear();
		mIndices.Clear();
	}

	public void Add(ref Vector3 _pos)
	{
		if(mFreeSplashData.Count == 0)
			return;

		SplashData data = mFreeSplashData.Pop();
		data.Pos = _pos;
		data.Time = mCurTime;
		data.Rotate = Random.Range(0, PI_2);

		mSplashData.Enqueue(data);

		mRefresh = true;
	}

	public void Framemove()
	{
		mCurTime = Time.realtimeSinceStartup;

		while(mSplashData.Count != 0)
		{
			SplashData data = mSplashData.Peek();
			if(1f <= mCurTime - data.Time)
			{
				mFreeSplashData.Push(mSplashData.Dequeue());
				mRefresh = true;
			}
			else
				break;
		}

		CreateMesh();
		SettingMaterial();
	}

	void SettingMaterial()
	{
		if(mMaterial == null)
			return;

		mMaterial.SetFloat(ShaderPropertyCurrentTime, mCurTime);
	}
}
