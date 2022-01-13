using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LodMeshCreator
{
    struct LodVertex
    {
        public Vector3 pos;
        public Color32 colour;
        public uint uv;

        public LodVertex(Vector3 _pos, Color32 _colour, uint _uv)
        {
            pos = _pos;
            colour = _colour;
            uv = _uv;
        }
    }

    static public bool Generate(ushort step, float radius, out Mesh mesh)
    {
        LodVertex[] vertices;
        ushort[] indices;        

        if (getMeshData(step, radius, out vertices, out indices) == false)
        {
            mesh = null;
            return false;
        }

        if (createMesh(ref vertices, ref indices, out mesh) == false)
        {
            return false;
        }

        return true;
    }

    static public bool Generate(ushort step, float radius, out Mesh mesh, out GraphicsBuffer arg)
    {
        if (Generate(step, radius, out mesh) == false)
        {
            arg = null;
            return false;
        }

        return getArgBuffer(mesh, out arg);
    }

    static private bool getArgBuffer(Mesh mesh, out GraphicsBuffer argBuffer)
    {
        argBuffer = null;

        if (mesh == null) return false;

        // Indirect args
        uint[] args = new uint[]
        {
            (uint)mesh.GetIndexCount(0),    // index count per instance
            (uint)1,                        // instance count
            (uint)mesh.GetIndexStart(0),    // start index location
            (uint)mesh.GetBaseVertex(0),    // base vertex location
            (uint)0                         // start instance location
        };

        argBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, args.Length, sizeof(uint));
        argBuffer.SetData(args);

        return true;
    }

    static private bool getMeshData(ushort step, float radius, out LodVertex[] vertices, out ushort[] indices)
    {
        vertices = null;
        indices = null;

        if (step <= 0 || radius <= 0f)
        {
            return false;
        }

        float gap = radius / step;

        vertices = new LodVertex[(step + 1) * (step + 1)];

        int index = 0;

        for (uint z = 0; z <= step; ++z)
        {
            float zPos = gap * z;
            for (uint x = 0; x <= step; ++x)
            {
                vertices[index].pos.x = gap * x;
                vertices[index].pos.y = 0f;
                vertices[index].pos.z = zPos;

                vertices[index].colour.r = 0;
                vertices[index].colour.g = 0;
                vertices[index].colour.b = 0;
                vertices[index].colour.a = 0;

                vertices[index].uv = ((z & 0xffff) << 16) | (x & 0xffff);

                ++index;
            }
        }

        // rgba => news
        index = step * (step + 1);
        for (ushort i = 0; i < step; ++i)
        {
            vertices[index + i].colour.r = 255;
        }

        index = step;
        for (ushort i = 0; i < step; ++i)
        {
            vertices[index + i * (step + 1)].colour.g = 255;
        }

        index = 0;
        for (ushort i = 0; i < step; ++i)
        {
            vertices[index + i * (step + 1)].colour.b = 255;
        }

        index = 0;
        for (ushort i = 0; i < step; ++i)
        {
            vertices[index + i].colour.a = 255;
        }

        ///

        indices = new ushort[step * step * 6];

        index = 0;

        for (ushort z = 0; z < step; ++z)
        {
            bool dir = ((z & 0x01) == 0);

            for (ushort x = 0; x < step; ++x)
            {
                indices[index++] = (ushort)((z * (step + 1)) + x);

                if (dir)
                {
                    indices[index++] = (ushort)(((z + 1) * (step + 1)) + x);
                    indices[index++] = (ushort)(((z + 1) * (step + 1)) + x + 1);
                    indices[index++] = (ushort)(((z + 1) * (step + 1)) + x + 1);
                    indices[index++] = (ushort)((z * (step + 1)) + x + 1);
                    indices[index++] = (ushort)((z * (step + 1)) + x);
                }
                else
                {
                    indices[index++] = (ushort)(((z + 1) * (step + 1)) + x);
                    indices[index++] = (ushort)((z * (step + 1)) + x + 1);
                    indices[index++] = (ushort)((z * (step + 1)) + x + 1);
                    indices[index++] = (ushort)(((z + 1) * (step + 1)) + x);
                    indices[index++] = (ushort)(((z + 1) * (step + 1)) + x + 1);
                }

                dir = !dir;
            }
        }

        return true;
    }

    static private bool createMesh(ref LodVertex[] vertices, ref ushort[] indices, out Mesh mesh)
    {
        mesh = null;

        if(vertices.Length == 0 || indices.Length == 0)
        {
            return false;
        }

        mesh = new Mesh();

        // 宣告 vertex buffer 結構
        VertexAttributeDescriptor[] layouts = new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.UInt32, 1)
        };

        mesh.SetVertexBufferParams(vertices.Length, layouts);

        // 宣告 index buffer 結構
        mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt16);

        MeshUpdateFlags flag = MeshUpdateFlags.Default;

        mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length, 0, flag);
        mesh.SetIndexBufferData(indices, 0, 0, indices.Length, flag);

        // 設定 Mesh Topologiy
        SubMeshDescriptor desc = new SubMeshDescriptor(0, indices.Length, MeshTopology.Triangles);
        mesh.SetSubMesh(0, desc, flag);

        return true;
    }
}
