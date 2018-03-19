using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteOceanRole : MonoBehaviour 
{
	[System.Serializable]
	public class RoleData
	{
		public Transform Body;
		public Transform[] Pts;
		public float Velocity;
		public float Angle;
		public float Scale;
		public bool IsMain;
	}

	[SerializeField] RoleData mRoleData;
	public RoleData Data { get { return mRoleData; } }
}
