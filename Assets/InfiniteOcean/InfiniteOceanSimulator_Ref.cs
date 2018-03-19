using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InfiniteOceanSimulator
{
	private readonly int ShaderPropertyWaveTex = Shader.PropertyToID("_WaveTex");
	private readonly int ShaderPropertyRefTex = Shader.PropertyToID("_RefTex");
	private readonly int ShaderPropertyRefVP = Shader.PropertyToID("_RefVP");
	private readonly int ShaderPropertyRefW = Shader.PropertyToID("_RefW");

	public Camera RefCam;
	RenderTexture RefRT;

	void InitRef()
	{
		CreateRefRT();
	}

	void ReleaseRef()
	{
		ReleaseRefRT();
	}

	void CreateRefRT()
	{
		ReleaseRefRT();

		RefRT = new RenderTexture (Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);

		if(RefCam != null) RefCam.targetTexture = RefRT;
	}

	void ReleaseRefRT()
	{
		ReleaseRenderTexture(ref RefRT);
	}

	private void UpdateRef()
	{
		Matrix4x4 refVMatrix = RefCam.worldToCameraMatrix;
		Matrix4x4 refPMatrix = GL.GetGPUProjectionMatrix(RefCam.projectionMatrix, false);
		Matrix4x4 refVP = refPMatrix * refVMatrix;
		Matrix4x4 refW = mWaveSetting.renderWater.localToWorldMatrix;

		OutputMat.SetMatrix(ShaderPropertyRefVP, refVP);
		OutputMat.SetMatrix(ShaderPropertyRefW, refW);
		OutputMat.SetTexture(ShaderPropertyWaveTex, RT);
		OutputMat.SetTexture(ShaderPropertyRefTex, RefRT);
	}
}
