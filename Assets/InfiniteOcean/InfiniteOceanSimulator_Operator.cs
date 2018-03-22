using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class InfiniteOceanSimulator
{
	float mPitch = 5f;
	float mYaw = 0f;
	float mDist = 10f;
	float mHeight = 3f;

	Vector3 mCenter;	

	private readonly int ShaderPropertyCenter = Shader.PropertyToID("_Center");

	// Use this for initialization
	void InitOperator()
	{
		mCenter = Vector3.zero;
	}

	void ReleaseOperator()
	{

	}

	void UpdateOperator()
	{
		OperCamera();

		SettingCamera();
		SettingWaterPlane();
	}

	void OperCamera()
	{
		if(Input.GetKey(KeyCode.A))
		{
			mYaw += 1f;
		}

		if(Input.GetKey(KeyCode.D))
		{
			mYaw -= 1f;
		}
	}

	void UpdateCenter(ref Vector3 _mov)
	{
		mCenter += _mov;
		OutputMat.SetVector(ShaderPropertyCenter, new Vector4(mCenter.x,mCenter.y,mCenter.z,NORMALIZE_OCEAN_PLANE_HIT_LENGTH));

		UpdatePrevPrev2Diff(_mov.x, _mov.z);
	}

	void SettingCamera()
	{
		Camera.main.transform.rotation = Quaternion.Euler(mPitch, mYaw, 0f);

		Vector3 dir = -Camera.main.transform.forward;
		Vector3 pos = mCenter + new Vector3(dir.x * mDist, mHeight, dir.z * mDist);
		Camera.main.transform.position = pos;

		RefCam.transform.rotation = Quaternion.Euler(-mPitch, mYaw, 0f);
		RefCam.transform.position = pos - Vector3.up * mHeight * 2f;
	}

	void SettingWaterPlane()
	{
		mWaveSetting.TargetPlane.position = new Vector3(Camera.main.transform.position.x,0f,Camera.main.transform.position.z);
		mWaveSetting.TargetPlane.rotation = Quaternion.Euler(0f, mYaw, 0f);
	}
}
