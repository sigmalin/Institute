using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{
    Mesh mMesh;
    public Mesh Mesh { get { return mMesh; } }

    int mResolution;
    public int Resolution { set { mResolution = value; } }

    Vector3 mUp;
    public Vector3 Up { set { mUp = value; } }

    public TerrainFace()
    {
        mMesh = new Mesh();
    }

    public void Release()
    {
        if(mMesh != null)
        {
            mMesh.Clear();
            GameObject.Destroy(mMesh);
            mMesh = null;
        }
    }

    public void Construct(PlanetShapeGenerator shapeGenerator, PlanetColorGenerator colorGenerator)
    {
        List<Vector3> vertices = new List<Vector3>(mResolution * mResolution);
        List<Vector2> uvs = new List<Vector2>(mResolution * mResolution);
        List<int> indices = new List<int>(mResolution * mResolution * 6);

        Vector3 right = new Vector3(mUp.y, mUp.z, mUp.x);
        Vector3 forward = Vector3.Cross(right, mUp);
        
        for (int y = 0; y < mResolution; ++y)
        {
            for (int x = 0; x < mResolution; ++x)
            {
                Vector2 percent = new Vector2((float)x / (mResolution-1), (float)y / (mResolution - 1));
                Vector3 pointOnCube = mUp + (percent.y - 0.5f) * 2f * forward + (percent.x - 0.5f) * 2f * right;
                Vector3 pointOnCircle = pointOnCube.normalized;

                float unscaledElevation = shapeGenerator.CalculateUnscaledElevation(pointOnCircle);
                vertices.Add(pointOnCircle * shapeGenerator.GetScaledElevation(unscaledElevation));
                uvs.Add(new Vector2(colorGenerator.CalculateBiomeOnPlanet(pointOnCircle), unscaledElevation));
            }
        }

        for (int y = 0; y < mResolution - 1; ++y)
        {
            for (int x = 0; x < mResolution - 1; ++x)
            {
                int index = x + y * (mResolution);

                indices.Add(index);
                indices.Add(index + mResolution);
                indices.Add(index + 1);

                indices.Add(index + mResolution + 1);
                indices.Add(index + 1);
                indices.Add(index + mResolution);
            }
        }
        
        mMesh.Clear();
        mMesh.SetVertices(vertices);
        mMesh.SetUVs(0, uvs);
        mMesh.SetTriangles(indices, 0);
        mMesh.RecalculateNormals();
        mMesh.RecalculateTangents();
        mMesh.UploadMeshData(false);

        colorGenerator.SetMinMaxValue(shapeGenerator.VecMinMax);
    }
}
