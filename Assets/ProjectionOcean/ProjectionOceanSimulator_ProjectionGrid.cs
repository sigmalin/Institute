using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ProjectionOceanSimulator
{
	public Material OceanDrawer;

	ProjectionGrid mProjectionGrid;	

	void InitProjectionGrid()
	{
		ReleaseProjectionGrid();

		mProjectionGrid = new ProjectionGrid();
		mProjectionGrid.Initial(OceanDrawer);
	}

	void ReleaseProjectionGrid()
	{
		if(mProjectionGrid != null)
		{
			mProjectionGrid.Release();
			mProjectionGrid = null;
		}
	}

	void DrawOcean()
	{
		if(mProjectionGrid == null) return;

		mProjectionGrid.Draw();
	}
}
