using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourierOcean_Matrices : FourierOcean
{
    const int OCEAN_GRID = 10;
    const int OCEAN_GRID_PLUS_ONE = OCEAN_GRID + 1;
    const int HALF_OCEAN_GRID = OCEAN_GRID >> 1;
    const float OCEAN_GRID_RADIUS = OCEAN_GRID * 0.707f;  // 2^0.5 / 2

    const int OCEAN_VIEW_COUNT = 32;
    const int HALF_OCEAN_VIEW_COUNT = OCEAN_VIEW_COUNT >> 1;

    Mesh mOceanPlane;

    Matrix4x4[] mMatricesTRS;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        InitOceanPlane();

        mMatricesTRS = new Matrix4x4[OCEAN_VIEW_COUNT * OCEAN_VIEW_COUNT];
    }

    protected override void Draw(Vector3 _lookAt, Material _drawer)
    {
        if (Camera.main == null) return;

        Vector4[] planes;
        CalcFrustumPlanes(Camera.main, out planes);
        
        int viewCount = 0;

        float startX = HALF_OCEAN_GRID + ((HALF_OCEAN_VIEW_COUNT-1) * OCEAN_GRID);
        float startZ = HALF_OCEAN_GRID + ((HALF_OCEAN_VIEW_COUNT - 1) * OCEAN_GRID);

        for (int z = 0; z < OCEAN_VIEW_COUNT; ++z)
        {
            float posZ = (z * OCEAN_GRID) - startZ;

            for (int x = 0; x < OCEAN_VIEW_COUNT; ++x)
            {
                float posX = (x * OCEAN_GRID) - startX;

                Vector3 pos = new Vector3(_lookAt.x + posX, 0f, _lookAt.z + posZ);
                if(IsInSide(pos, OCEAN_GRID_RADIUS, planes) == true)
                {
                    mMatricesTRS[viewCount++] = Matrix4x4.Translate(pos);
                }
            }
        }

        if(viewCount != 0)
        {
            Graphics.DrawMeshInstanced(mOceanPlane, 0, mMatFourierOcean, mMatricesTRS, viewCount);
        }
    }
    
    void InitOceanPlane()
    {
        
        List<Vector3> vertices = new List<Vector3>(OCEAN_GRID_PLUS_ONE * OCEAN_GRID_PLUS_ONE);
        List<Vector3> normals = new List<Vector3>(OCEAN_GRID_PLUS_ONE * OCEAN_GRID_PLUS_ONE);
        List<Vector2> uvs = new List<Vector2>(OCEAN_GRID_PLUS_ONE * OCEAN_GRID_PLUS_ONE);
        List<int> indices = new List<int>(OCEAN_GRID_PLUS_ONE * OCEAN_GRID_PLUS_ONE * 6);

        for (int y = 0; y < OCEAN_GRID_PLUS_ONE; ++y)
        {
            for (int x = 0; x < OCEAN_GRID_PLUS_ONE; ++x)
            {
                vertices.Add(new Vector3(x - HALF_OCEAN_GRID, 0f, y - HALF_OCEAN_GRID));
                normals.Add(Vector3.up);
            }
        }

        for (int y = 0; y < OCEAN_GRID; ++y)
        {
            for (int x = 0; x < OCEAN_GRID; ++x)
            {
                indices.Add(x + y * OCEAN_GRID_PLUS_ONE);
                indices.Add(x + (y + 1) * OCEAN_GRID_PLUS_ONE);
                indices.Add(x + 1 + y * OCEAN_GRID_PLUS_ONE);
                indices.Add(x + 1 + (y + 1) * OCEAN_GRID_PLUS_ONE);
                indices.Add(x + 1 + y * OCEAN_GRID_PLUS_ONE);
                indices.Add(x + (y + 1) * OCEAN_GRID_PLUS_ONE);
            }
        }

        mOceanPlane = new Mesh();

        mOceanPlane.SetVertices(vertices);
        mOceanPlane.SetNormals(normals);
        mOceanPlane.SetUVs(0, uvs);
        mOceanPlane.SetTriangles(indices, 0);
        mOceanPlane.RecalculateTangents();
        mOceanPlane.UploadMeshData(true);
    }
    
    void CalcFrustumPlanes(Camera _cam, out Vector4[] _planes)
    {
        _planes = new Vector4[6];

        Plane[] sourcePlanes = GeometryUtility.CalculateFrustumPlanes(_cam);

        _planes[0] = new Vector4(sourcePlanes[0].normal.x, sourcePlanes[0].normal.y, sourcePlanes[0].normal.z, sourcePlanes[0].distance);
        _planes[1] = new Vector4(sourcePlanes[1].normal.x, sourcePlanes[1].normal.y, sourcePlanes[1].normal.z, sourcePlanes[1].distance);
        _planes[2] = new Vector4(sourcePlanes[2].normal.x, sourcePlanes[2].normal.y, sourcePlanes[2].normal.z, sourcePlanes[2].distance);
        _planes[3] = new Vector4(sourcePlanes[3].normal.x, sourcePlanes[3].normal.y, sourcePlanes[3].normal.z, sourcePlanes[3].distance);
        _planes[4] = new Vector4(sourcePlanes[4].normal.x, sourcePlanes[4].normal.y, sourcePlanes[4].normal.z, sourcePlanes[4].distance);
        _planes[5] = new Vector4(sourcePlanes[5].normal.x, sourcePlanes[5].normal.y, sourcePlanes[5].normal.z, sourcePlanes[5].distance);
    }

    bool IsInSide(Vector3 _pos, float _radius, Vector4[] _planes)
    {
        if (_planes == null || _planes.Length != 6) return false;

        Vector4 center = new Vector4(_pos.x, _pos.y, _pos.z, 1.0f);

        var leftDistance = Vector4.Dot(_planes[0], center);
        var rightDistance = Vector4.Dot(_planes[1], center);
        var downDistance = Vector4.Dot(_planes[2], center);
        var upDistance = Vector4.Dot(_planes[3], center);
        var nearDistance = Vector4.Dot(_planes[4], center);
        var farDistance = Vector4.Dot(_planes[5], center);

        var leftOut = leftDistance < -_radius;
        var rightOut = rightDistance < -_radius;
        var downOut = downDistance < -_radius;
        var upOut = upDistance < -_radius;
        var nearOut = nearDistance < -_radius;
        var farOut = farDistance < -_radius;
        var anyOut = leftOut || rightOut || downOut || upOut || nearOut || farOut;

        if (anyOut) return false;
        /*
                var leftIn = leftDistance > _radius;
                var rightIn = rightDistance > _radius;
                var downIn = downDistance > _radius;
                var upIn = upDistance > _radius;
                var nearIn = nearDistance > _radius;
                var farIn = farDistance > _radius;
                var allIn = leftIn && rightIn && downIn && upIn && nearIn && farIn;
        */
        return true;
    }
}
