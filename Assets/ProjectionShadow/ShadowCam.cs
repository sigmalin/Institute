using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class ShadowCam : MonoBehaviour 
{
	public RenderTexture ShadowMap;

	public Material MaterialBlur;

	Camera cacheCamera;

	CommandBuffer cbBlur;

	readonly int ProjectionShadowMapID = Shader.PropertyToID("_ProjectionShadowMap");
	readonly int ProjectionViewProjID = Shader.PropertyToID("_ProjectionViewProj");

	// Use this for initialization
	void Start () 
	{
		cacheCamera = this.GetComponent<Camera>();
		cacheCamera.SetReplacementShader(Shader.Find("Shadow/ProjectionShadowMap"), "RenderType");
	}

	void OnDestroy()
	{
		ReleaseBlurCommand();
		ReleaseShadowMap();		
	}

	void CreateShadowMap()
	{
		ReleaseShadowMap();

		int sizeRT = 512;
		
		ShadowMap = RenderTexture.GetTemporary(sizeRT, sizeRT, 16, RenderTextureFormat.ARGB32);
		Shader.SetGlobalTexture (ProjectionShadowMapID, ShadowMap);
		cacheCamera.targetTexture = ShadowMap;

		CreateBlurCommand();
	}

	void ReleaseShadowMap()
	{
		if(ShadowMap != null)
		{
			Shader.SetGlobalTexture (ProjectionShadowMapID, null);
			RenderTexture.ReleaseTemporary(ShadowMap);
			ShadowMap = null;
			cacheCamera.targetTexture = null;
		}
	}
	
	// Update is called once per frame
	void OnPreRender()
	{
		if(ShadowMap == null)
			CreateShadowMap();		

		Matrix4x4 view = cacheCamera.worldToCameraMatrix;
		Matrix4x4 proj = GL.GetGPUProjectionMatrix (cacheCamera.projectionMatrix, true);
		Shader.SetGlobalMatrix(ProjectionViewProjID, proj*view);
	}

	void CreateBlurCommand()
	{
		ReleaseBlurCommand();

		int sizeRT = 256;

		cbBlur = new CommandBuffer();
		cbBlur.name = "Blur";

		int blurRT1 = Shader.PropertyToID("_blurRT1");
		int blurRT2 = Shader.PropertyToID("_blurRT2");

		cbBlur.GetTemporaryRT(blurRT1, sizeRT, sizeRT, 0, FilterMode.Bilinear);
		cbBlur.Blit(ShadowMap, blurRT1, MaterialBlur, 0);

		cbBlur.GetTemporaryRT(blurRT2, sizeRT, sizeRT, 0, FilterMode.Bilinear);
		cbBlur.Blit(blurRT1, blurRT2, MaterialBlur, 1);

		cbBlur.Blit(blurRT2, blurRT1, MaterialBlur, 2);

		cbBlur.Blit(blurRT1, ShadowMap);

		cbBlur.ReleaseTemporaryRT(blurRT1);
		cbBlur.ReleaseTemporaryRT(blurRT2);

		cacheCamera.AddCommandBuffer(CameraEvent.AfterEverything, cbBlur);
	}

	void ReleaseBlurCommand()
	{
		if(cbBlur == null)
			return;

		cacheCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, cbBlur);
		cbBlur.Release();
		cbBlur = null;
	}
}
