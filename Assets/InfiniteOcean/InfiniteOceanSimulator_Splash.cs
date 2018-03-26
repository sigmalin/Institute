using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class InfiniteOceanSimulator
{
	[SerializeField] WaterSplash mSplash;

	void InitSplash()
	{
	}

	void ReleaseSplash()
	{
	}

	void UpdateSplash()
	{
		if(mSplash == null)
			return;

		mSplash.Framemove();	
	}

	void AddSplash(Vector3 _pos)
	{
		if(mSplash == null)
			return;

		mSplash.Add(ref _pos);
	}
}
