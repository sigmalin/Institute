using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanMesh : MonoBehaviour
{
    public int GridCount = 100;
    int mGridCount = 0;
    Mesh mMesh;

    public MeshFilter MF;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int grid = Mathf.Max(2, GridCount);
        if(mGridCount != grid)
        {
            mGridCount = grid;
            BuildOceanMesh(mGridCount);
        }

        if (MF != null) MF.sharedMesh = mMesh;
    }

    void BuildOceanMesh(int _grid)
    {
        int count = _grid * _grid;

        List<Vector3> vertices = new List<Vector3>(count);
        List<Vector3> normals = new List<Vector3>(count);
        List<int> indices = new List<int>(count * 6);

        for(int z = 0; z < _grid; ++z)
        {
            for (int x = 0; x < _grid; ++x)
            {
                vertices.Add(new Vector3(x,0f,z));
                normals.Add(Vector3.up);
            }
        }

        for (int z = 0; z < _grid - 1; ++z)
        {
            for (int x = 0; x < _grid - 1; ++x)
            {
                indices.Add(z * _grid + x);
                indices.Add((z + 1) * _grid + x);
                indices.Add(z * _grid + x + 1);
                indices.Add((z + 1) * _grid + x + 1);
                indices.Add(z * _grid + x + 1);
                indices.Add((z + 1) * _grid + x);
            }
        }

        if(mMesh == null)
            mMesh = new Mesh();

        mMesh.Clear();
        mMesh.SetVertices(vertices);
        mMesh.SetNormals(normals);
        mMesh.SetTriangles(indices, 0);
        mMesh.RecalculateTangents();
        mMesh.UploadMeshData(true);
    }
}
