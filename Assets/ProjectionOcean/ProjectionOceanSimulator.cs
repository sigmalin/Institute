using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ProjectionOceanSimulator : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{		
		InitProjectionGrid();
	}

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void Update()
	{
		DrawOcean();	
	}
}
