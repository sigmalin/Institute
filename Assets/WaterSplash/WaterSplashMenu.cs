using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSplashMenu : MonoBehaviour 
{
	public WaterSplash splash;

	private bool isStopSplash;

	// Use this for initialization
	void Start () 
	{
		StartCoroutine(TestPlayer());		
	}

	void OnEnable()
	{
		isStopSplash = false;
	}

	void OnDisable()
	{
		isStopSplash = true;
	}


	// Update is called once per frame
	void Update()
	{
		if(splash == null)
			return;

		splash.Framemove();
	}
	
	IEnumerator TestPlayer()
	{
		while(isStopSplash == false)
		{
			yield return new WaitForSeconds(1f);

			Vector3 pos = Vector3.zero;

			splash.Add(ref pos);	
		}
			
	}
}
