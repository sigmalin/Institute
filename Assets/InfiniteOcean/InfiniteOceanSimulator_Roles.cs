using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class InfiniteOceanSimulator
{
	public InfiniteOceanRole[] mRoleList;

	void InitRoleList()
	{
	}

	void ReleaseRoleList()
	{
	}

	void UpdateRole()
	{
		for(int roleIndx = 0; roleIndx < mRoleList.Length; ++roleIndx)
		{
			mRoleList[roleIndx].Data.Body.rotation = Quaternion.Euler(0f,mRoleList[roleIndx].Data.Angle,0f);
			Vector3 mov = mRoleList[roleIndx].Data.Body.forward * mRoleList[roleIndx].Data.Velocity * Time.deltaTime;

			mRoleList[roleIndx].Data.Body.Translate(mov.x, 0f, mov.z, Space.World);

			if(mRoleList[roleIndx].Data.IsMain)
			{
				UpdateCenter(ref mov);
			}

			for(int hitIndx = 0; hitIndx < mRoleList[roleIndx].Data.Pts.Length; ++hitIndx)
			{
				AddRaycasetList(mRoleList[roleIndx].Data.Pts[hitIndx], mRoleList[roleIndx].Data.Scale);		
				
				if(mRoleList[roleIndx].Data.Velocity != 0f)
					AddSplash(mRoleList[roleIndx].Data.Pts[hitIndx].position);		
			}
		}
	}
}
