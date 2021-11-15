using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CubeMaker
{
    struct CubeVertex
    {
        public Vector3 pos;
        public Vector3 normal;
    }

    public static bool Generate(float size, out Mesh _mesh)
    {
        const int VERTICES_COUNT = 8;

        float halfSize = size * 0.5f;

        // vertex 
        CubeVertex[] vertices = new CubeVertex[VERTICES_COUNT];
        vertices[0].pos = new Vector3(-halfSize, -halfSize, -halfSize);
        vertices[1].pos = new Vector3(-halfSize,  halfSize, -halfSize);
        vertices[2].pos = new Vector3( halfSize,  halfSize, -halfSize);
        vertices[3].pos = new Vector3( halfSize, -halfSize, -halfSize);
        vertices[4].pos = new Vector3(-halfSize, -halfSize,  halfSize);
        vertices[5].pos = new Vector3(-halfSize,  halfSize,  halfSize);
        vertices[6].pos = new Vector3( halfSize,  halfSize,  halfSize);
        vertices[7].pos = new Vector3( halfSize, -halfSize,  halfSize);

        for(int i = 0; i < VERTICES_COUNT; ++i)
        {
            vertices[i].normal = Vector3.Normalize(vertices[i].pos);
        }

        // index
        ushort[] indices = new ushort[] {
            0, 1, 2,
            2, 3, 0,
            0, 4, 5,
            5, 1, 0,
            2, 6, 7,
            7, 3, 2,
            7, 6, 5,
            5, 4, 7,
            1, 5, 6,
            6, 2, 1,
            4, 0, 3,
            3, 7, 4 
        };

        MeshUpdateFlags flag = MeshUpdateFlags.Default;

        VertexAttributeDescriptor[] layouts = new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
        };

        _mesh = new Mesh();

        // 宣告 index buffer 結構
        _mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt16);

        // 宣告 vertex buffer 結構
        _mesh.SetVertexBufferParams(VERTICES_COUNT, layouts);

        _mesh.SetVertexBufferData(vertices, 0, 0, VERTICES_COUNT, 0, flag);
        _mesh.SetIndexBufferData(indices, 0, 0, indices.Length, flag);

        // 設定 Mesh Topologiy
        SubMeshDescriptor desc = new SubMeshDescriptor(0, indices.Length, MeshTopology.Triangles);
        _mesh.SetSubMesh(0, desc, flag);

        return true;
    }
}
