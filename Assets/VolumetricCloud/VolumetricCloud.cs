using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VolumetricCloud : MonoBehaviour 
{

	Mesh mMesh;
	[SerializeField] Material mMaterial;

	const int CLOUD_MAX = 512;

	const float CLOUD_LIFE = 2f;

	const float CLOUD_INTERVAL = 2f;

	List<Vector3> mVertices;
	List<Vector2> mUVs;
	List<Vector4> mUV2s;
	List<int> mIndices;

	protected class CloudData
	{
		public Vector3 Pos { get; set; }
		public float ScaleX { get; set; }
		public float ScaleY { get; set; }
		public float Rotate { get; set; }
		public float Time { get; set; }
	}

	Queue<CloudData> mCloudDatas;
	Stack<CloudData> mFreeCloudData;

	float mCurTime;
	bool mRefresh;

	float mLastTime;

	Vector3 mCenter;

	readonly Vector2[] UV_MAPS = new Vector2[] {
		new Vector2(-1.077f, -0.5f),
		new Vector2(     0f,  1.366f),
		new Vector2( 1.077f, -0.5f),
	};

	readonly float PI_2 = Mathf.PI * 2f;
	int ShaderPropertyCurrentTime;
	int ShaderPropertyLifeTime;

	// Use this for initialization
	public void Initialize ()
	{
		mMesh = new Mesh();
		mMesh.name = "CloudMesh";
		mMesh.MarkDynamic();

		mVertices = new List<Vector3>(CLOUD_MAX*3);
		mUVs      = new List<Vector2>(CLOUD_MAX*3);
		mUV2s     = new List<Vector4>(CLOUD_MAX*3);
		mIndices  = new List<int>(CLOUD_MAX*3);

		mCloudDatas = new Queue<CloudData>(CLOUD_MAX);
		mFreeCloudData = new Stack<CloudData>(CLOUD_MAX);
		for(int Indx = 0; Indx < CLOUD_MAX; ++Indx)
		{
			mFreeCloudData.Push(new CloudData());
		}

		mRefresh = false;

		this.GetComponent<MeshFilter>().sharedMesh = mMesh;	
		this.GetComponent<MeshRenderer>().sharedMaterial = mMaterial;

		ShaderPropertyCurrentTime = Shader.PropertyToID("_CurrentTime");

		ShaderPropertyLifeTime = Shader.PropertyToID("_LifeTime");
		mMaterial.SetFloat(ShaderPropertyLifeTime, CLOUD_LIFE);
	}

	void UpdateCloudDatas()
	{
		while(mCloudDatas.Count != 0)
		{
			if(mCurTime - mCloudDatas.Peek().Time < CLOUD_LIFE)
				break;

			mFreeCloudData.Push(mCloudDatas.Dequeue());
			mRefresh = true;
		}
	}

	void CreateMesh()
	{
		if(mRefresh == false)
			return;

		ClearMeshData();

		int dataIndx = 0;

		Queue<CloudData>.Enumerator etor = mCloudDatas.GetEnumerator();		
		while(etor.MoveNext())
		{
			CloudData data = etor.Current;

			for(int Indx = 0; Indx < 3; ++Indx)
			{
				mVertices.Add(data.Pos);
				mUVs.Add(UV_MAPS[Indx]);
				mUV2s.Add(new Vector4(data.ScaleX, data.ScaleY, data.Rotate, data.Time));
			}

			mIndices.Add(dataIndx*3+0);
			mIndices.Add(dataIndx*3+1);
			mIndices.Add(dataIndx*3+2);

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

	void Add(ref Vector3 _pos)
	{
		if(mFreeCloudData.Count == 0)
			return;

		CloudData data = mFreeCloudData.Pop();
		data.Pos = _pos;
		float scale = Random.Range(4f, 8f);
		data.ScaleX = scale;
		data.ScaleY = scale;
		data.Rotate = Random.Range(0, PI_2);
		data.Time = mCurTime;

		mCloudDatas.Enqueue(data);

		mRefresh = true;
	}

	void AddCloud(ref Vector3 _pos)
	{
		int countGroup = 64;//Random.Range(16, 32);
		List<Vector3> clouds = new List<Vector3>(countGroup);

		Vector3 center = Vector3.one * 0.5f;
		Vector3 meshScale = new Vector3(10f,4f,10f);

		for(int Indx = 0; Indx < countGroup; ++Indx)
		{
			clouds.Add(_pos + Vector3.Scale (new Vector3(Random.value,Random.value,Random.value) - center, meshScale));
		}

		clouds.Sort(CompareCloud);

		for(int Indx = 0; Indx < countGroup; ++Indx)
		{
			Vector3 addr = clouds[Indx];
			Add(ref addr);
		}
	}

	int CompareCloud(Vector3 _1, Vector3 _2)
	{
		if(_2.z < _1.z)
			return -1;
		if(_1.z < _2.z)
			return 1;
		return 0;
	}

	void CreateCloud()
	{
		if(mCurTime - mLastTime < CLOUD_INTERVAL)
			return;

		mLastTime = mCurTime;

		float length = 0f;
		float angle = Random.Range(0f,2f) * Mathf.PI;

		Vector3 center = mCenter + (Vector3.right * Mathf.Cos(angle) + Vector3.forward * Mathf.Sin(angle)) * length + Vector3.up * 8f;
		
		AddCloud(ref center);
	}

	public void UpdateCenter(ref Vector3 _center)
	{
		mCenter = _center;
	}

	public void Framemove()
	{
		mCurTime = Time.realtimeSinceStartup;

		CreateCloud();

		UpdateCloudDatas();
		CreateMesh();
		SettingMaterial();
	}

	void SettingMaterial()
	{
		if(mMaterial == null)
			return;

		mMaterial.SetFloat(ShaderPropertyCurrentTime, mCurTime);
	}

#region DEMO
	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	void Start()
	{
		Initialize();
	}

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void Update()
	{
		Framemove();
	}
#endregion
}
