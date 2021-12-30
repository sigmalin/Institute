using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GeometryGrass : MonoBehaviour
{
    Mesh meshGrass;
    // Start is called before the first frame update
    void Start()
    {
        InitMeshFilter();
    }

    private void OnDestroy()
    {
        ReleaseMeshFilter();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitMeshFilter()
    {
        ReleaseMeshFilter();

        int vertexCount = 100;
        int indexCount = 100;

        meshGrass = new Mesh();
        
        // 宣告 vertex buffer 結構
        VertexAttributeDescriptor[] layouts = new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3)
        };

        meshGrass.SetVertexBufferParams(vertexCount, layouts);

        // 宣告 index buffer 結構
        meshGrass.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

        MeshUpdateFlags flag = MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontRecalculateBounds;

        meshGrass.SetVertexBufferData(getRandomPosition(vertexCount, 10f), 0, 0, vertexCount, 0, flag);
        meshGrass.SetIndexBufferData(getSequenceIndices(indexCount), 0, 0, indexCount, flag);

        // 設定 Mesh Topologiy
        SubMeshDescriptor desc = new SubMeshDescriptor(0, indexCount, MeshTopology.Points);
        meshGrass.SetSubMesh(0, desc, flag);


        meshGrass.bounds = new Bounds(Vector3.zero, new Vector3(1000f,5f,1000f));

        this.GetComponent<MeshFilter>().sharedMesh = meshGrass;
    }

    void ReleaseMeshFilter()
    {
        if(meshGrass != null)
        {
            meshGrass.Clear();
            GameObject.Destroy(meshGrass);
            meshGrass = null;
        }
    }

    double RadicalInverse_VdC(uint bits)
    {
        bits = (bits << 16) | (bits >> 16);
        bits = ((bits & 0x55555555) << 1) | ((bits & 0xAAAAAAAA) >> 1);
        bits = ((bits & 0x33333333) << 2) | ((bits & 0xCCCCCCCC) >> 2);
        bits = ((bits & 0x0F0F0F0F) << 4) | ((bits & 0xF0F0F0F0) >> 4);
        bits = ((bits & 0x00FF00FF) << 8) | ((bits & 0xFF00FF00) >> 8);
        return ((double)bits) * 2.3283064365386963e-10; // / 0x100000000
    }

    Vector3[] getRandomPosition(int count, float size)
    {
        Vector3[] position = new Vector3[count];
        for (int i = 0; i < count; ++i)
        {
            float x = (float)RadicalInverse_VdC((uint)i) * size;
            float y = (float)i / count * size;

            position[i].x = x - (size * 0.5f);
            position[i].z = y - (size * 0.5f);
            position[i].y = 0f;
        }

        return position;
    }

    ushort[] getSequenceIndices(int count)
    {
        ushort[] indices = new ushort[count];
        for (int i = 0; i < count; ++i)
        {
            indices[i] = (ushort)i;
        }

        return indices;
    }
}
