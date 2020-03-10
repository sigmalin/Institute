using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Range(2, 256)]
    public int resoultion = 10;

    public PlanetColorSetting ColorSetting;
    public PlanetShapeSetting ShapeSetting;

    MeshFilter[] mMeshFiliters;
    TerrainFace[] mTerrainface;

    PlanetShapeGenerator mShapeGenerator;
    PlanetColorGenerator mColorGenerator;

    public void Generate()
    {
        Initialize();
        UpdateShape();
        UpdateColor();
    }

    void Initialize()
    {
        if(mMeshFiliters == null)
        {
            mMeshFiliters = new MeshFilter[6];
        }

        if (mTerrainface == null)
        {
            mTerrainface = new TerrainFace[6];
        }

        Vector3[] dirs = new Vector3[]
        {
            Vector3.up, Vector3.down, Vector3.right,
            Vector3.forward, Vector3.back, Vector3.left,
        };

        for(int i = 0; i < dirs.Length; ++i)
        {
            if (mMeshFiliters[i] == null)
            {
                GameObject go = new GameObject(dirs[i].ToString());
                go.transform.SetParent(this.transform);
                go.transform.localPosition = Vector3.zero;

                mMeshFiliters[i] = go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
            }

            if (mTerrainface[i] == null)
                mTerrainface[i] = new TerrainFace();

            mTerrainface[i].Resolution = resoultion;
            mTerrainface[i].Up = dirs[i];

            mMeshFiliters[i].sharedMesh = mTerrainface[i].Mesh;
        }

        if(mShapeGenerator == null)
        {
            mShapeGenerator = new PlanetShapeGenerator();
        }
        mShapeGenerator.Initialize(ShapeSetting);

        if(mColorGenerator == null)
        {
            mColorGenerator = new PlanetColorGenerator();
        }
        mColorGenerator.Initialize(ColorSetting);
    }

    void UpdateShape()
    {
        for (int i = 0; i < mTerrainface.Length; ++i)
        {
            mTerrainface[i].Construct(mShapeGenerator, mColorGenerator);
        }
    }

    void UpdateColor()
    {
        mColorGenerator.UpdateGradientTex();

        for (int i = 0; i < mMeshFiliters.Length; ++i)
        {
            if (ColorSetting == null) continue;

            MeshRenderer renderer = mMeshFiliters[i].GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mColorGenerator.MatPlanet;
        }
    }
}
