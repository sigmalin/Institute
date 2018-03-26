using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class InfiniteOceanSimulator : MonoBehaviour 
{
	[SerializeField] Material mInputDrawer;
	[SerializeField] Material mWaveDrawer;			

	// Use this for initialization
	void Start () 
	{
		InitOceanPlane();

		InitWave();

		InitRef();

		InitOperator();

		InitRoleList();

		InitSplash();
	}

	void OnDestroy()
	{
		ReleaseOceanPlane();

		ReleaseWave();

		ReleaseRef();

		ReleaseOperator();

		ReleaseRoleList();

		ReleaseSplash();
	}

	void ReleaseCommandBuffer(ref CommandBuffer _cmdBuff)
	{
		if(_cmdBuff != null)
		{
			_cmdBuff.Release();
			_cmdBuff = null;
		}
	}

	void ClearRenderTexture(ref RenderTexture _RT)
	{
		Graphics.SetRenderTarget (_RT);
		GL.Clear (true, true, Color.gray);
		Graphics.SetRenderTarget (null);
	}

	void ReleaseRenderTexture(ref RenderTexture _RT)
	{
		if(_RT != null)
		{
			_RT.Release();
			_RT = null;
		}
	}

	void ReleaseList<T>(ref T _list) where T : IList
	{
		if(_list != null)
		{
			_list.Clear();
			_list = default(T);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateRole();

		UpdateWave();

		UpdateRef();

		UpdateOperator();

		UpdateSplash();
	}
}
