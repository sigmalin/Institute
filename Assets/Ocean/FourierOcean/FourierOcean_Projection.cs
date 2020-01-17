using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourierOcean_Projection : FourierOcean
{
    ProjectionGrid mProjectionGrid;

    protected override void Draw(Vector3 _lookAt, Material _drawer)
    {
        if(mProjectionGrid == null)
        {
            mProjectionGrid = new ProjectionGrid();
            mProjectionGrid.Initial(_drawer);
        }

        mProjectionGrid.Draw();
    }
}
