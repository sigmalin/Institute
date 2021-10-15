using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class DrawCard
{
    struct QuadData
    {
        public Vector2 pt1;
        public Vector2 pt2;
        public Vector2 pt3;
        public Vector2 pt4;
        public Vector2 uv1;
        public Vector2 uv2;
        public Vector2 uv3;
        public Vector2 uv4;
    }

    struct TriangleData
    {
        public Vector2 pt1;
        public Vector2 pt2;
        public Vector2 pt3;
        public Vector2 uv1;
        public Vector2 uv2;
        public Vector2 uv3;
    }

    struct DiamondData
    {
        public Vector2 pt1;
        public Vector2 pt2;
        public Vector2 pt3;
        public Vector2 pt4;
        public Vector2 pt5;
        public Vector2 uv1;
        public Vector2 uv2;
        public Vector2 uv3;
        public Vector2 uv4;
        public Vector2 uv5;
    }

    struct FillFlipData
    {
        public Vector2 dragDir;
        public Vector2 dragPt;
        public Vector2 crossX;
        public Vector2 crossY;
    }

    Vector2 CalcSymmetry(Vector2 p, Vector2 ptOnLine, float slope)
    {
        // 求點 p 相對於線 y = kx + v 對稱點
        float v = ptOnLine.y - (slope * ptOnLine.x);
        float A = slope;
        float B = -1f;
        float C = v;

        float u = -2f * (A * p.x + B * p.y + C) / (A * A + B * B);
        return new Vector2(p.x + u * A, p.y + u * B);
    }

    float FillFlipStraight(Vector2 dragDir)
    {
        float halfDirX = dragDir.x * 0.5f;
        float halfDirY = dragDir.y * 0.5f;

        float progress = 0f;

        QuadData data;

        float minX = Mathf.Max(-halfSize.x, -halfSize.x + halfDirX);
        float maxX = Mathf.Min(halfSize.x, halfSize.x + halfDirX);
        float minY = Mathf.Max(-halfSize.y, -halfSize.y + halfDirY);
        float maxY = Mathf.Min(halfSize.y, halfSize.y + halfDirY);

        float minU = Mathf.Max(halfDirX, 0) / texSize.x;
        float maxU = Mathf.Min(texSize.x + halfDirX, texSize.x) / texSize.x;
        float minV = Mathf.Max(halfDirY, 0) / texSize.y;
        float maxV = Mathf.Min(texSize.y + halfDirY, texSize.y) / texSize.y;

        data.pt1.x = minX;
        data.pt1.y = minY;
        data.pt2.x = minX;
        data.pt2.y = maxY;
        data.pt3.x = maxX;
        data.pt3.y = maxY;
        data.pt4.x = maxX;
        data.pt4.y = minY;

        data.uv1.x = minU;
        data.uv1.y = minV;
        data.uv2.x = minU;
        data.uv2.y = maxV;
        data.uv3.x = maxU;
        data.uv3.y = maxV;
        data.uv4.x = maxU;
        data.uv4.y = minV;

        FillQuad(ref mCardBack, ref data);

        progress = 1f -Mathf.Min((maxX - minX)/ texSize.x, (maxY - minY) / texSize.y);

        if (halfDirY == 0)
        {
            if (halfDirX < 0)
            {
                minX = maxX + halfDirX;

                minU = 0;
                maxU = 0 - halfDirX / texSize.x;
            }
            else
            {
                maxX = minX + halfDirX;

                minU = 1 - halfDirX / texSize.x;
                maxU = 1;
            }
        }
        else
        {
            if (halfDirY < 0)
            {
                minY = maxY + halfDirY;

                minV = 1f;
                maxV = 1f + halfDirY / texSize.y;
            }
            else
            {
                maxY = minY + halfDirY;

                minV = 0f + halfDirY / texSize.y;
                maxV = 0;
            }

            maxU = 0f;
            minU = 1f;
        }

        data.pt1.x = minX;
        data.pt1.y = minY;
        data.pt2.x = minX;
        data.pt2.y = maxY;
        data.pt3.x = maxX;
        data.pt3.y = maxY;
        data.pt4.x = maxX;
        data.pt4.y = minY;

        data.uv1.x = minU;
        data.uv1.y = minV;
        data.uv2.x = minU;
        data.uv2.y = maxV;
        data.uv3.x = maxU;
        data.uv3.y = maxV;
        data.uv4.x = maxU;
        data.uv4.y = minV;

        FillQuad(ref mCardFront, ref data);

        return progress;
    }

    void FillQuad(ref Mesh quad, ref QuadData data)
    {
        _vertices[0].pos.x = data.pt1.x;
        _vertices[0].pos.y = data.pt1.y;
        _vertices[0].uv.x = data.uv1.x;
        _vertices[0].uv.y = data.uv1.y;

        _vertices[1].pos.x = data.pt2.x;
        _vertices[1].pos.y = data.pt2.y;
        _vertices[1].uv.x = data.uv2.x;
        _vertices[1].uv.y = data.uv2.y;

        _vertices[2].pos.x = data.pt3.x;
        _vertices[2].pos.y = data.pt3.y;
        _vertices[2].uv.x = data.uv3.x;
        _vertices[2].uv.y = data.uv3.y;

        _vertices[3].pos.x = data.pt4.x;
        _vertices[3].pos.y = data.pt4.y;
        _vertices[3].uv.x = data.uv4.x;
        _vertices[3].uv.y = data.uv4.y;

        MeshUpdateFlags flag = GetMeshUpdateFlags();

        const int indexCount = 6;
        const int vertexCount = 4;

        // 設定 Mesh Topologiy
        SubMeshDescriptor desc = new SubMeshDescriptor(0, indexCount, MeshTopology.Triangles);
        quad.SetSubMesh(0, desc, flag);

        // 宣告 index buffer 結構
        quad.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

        // 宣告 vertex buffer 結構
        quad.SetVertexBufferParams(vertexCount, _layouts);

        quad.SetVertexBufferData(_vertices, 0, 0, vertexCount, 0, flag);
        quad.SetIndexBufferData(_indices, 0, 0, indexCount, flag);
    }

    float FillFlipSlope(Vector2 dragDir)
    {
        float progress = 0f;

        // 折線中間點
        float halfX = dragDir.x * 0.5f;
        float halfY = dragDir.y * 0.5f;

        // 與 X Y 軸的交接點
        // 令與 Y 軸 交點 (0,Y),  (halfY - Y) / (halfX - 0) = slope
        float crossY = (dragDir.x * halfX) / dragDir.y + halfY;
        // 令與 X 軸 交點 (X,0),  (0 - halfY) / (X - halfX) = slope
        float crossX = (dragDir.y * halfY) / dragDir.x + halfX;

        // 根據滑動方向 計算平移量
        float displacementX = halfSize.x * (dragDir.x < 0 ? 1f : -1f);
        float displacementY = halfSize.y * (dragDir.y < 0 ? 1f : -1f);

        FillFlipData data;

        data.dragPt.x = dragDir.x + displacementX;
        data.dragPt.y = dragDir.y + displacementY;
        data.dragDir.x = dragDir.x;
        data.dragDir.y = dragDir.y;

        // 平移交接點
        data.crossX.x = crossX + displacementX;
        data.crossX.y = displacementY;
        data.crossY.x = displacementX;
        data.crossY.y = crossY + displacementY;

        // 檢查交點是否超過牌的長寬
        bool isOverBoundX = texSize.x < Mathf.Abs(crossX);
        bool isOverBoundY = texSize.y < Mathf.Abs(crossY);
        if (isOverBoundX || isOverBoundY)
        {
            // 滑動方向的法線斜率
            float slope = -dragDir.x / dragDir.y;

            float boundW = dragDir.x < 0 ? -texSize.x : texSize.x;
            float boundH = dragDir.y < 0 ? -texSize.y : texSize.y;

            Vector2 symmetryX = Vector2.zero;
            Vector2 symmetryY = Vector2.zero;

            // 交點超過牌的長寬
            if (isOverBoundX)
            {
                float crossX1 = (boundW - crossX) * slope;
                //(boundW, crossX) 
                data.crossX.x = boundW + displacementX;
                data.crossX.y = crossX1 + displacementY;

                symmetryX.x = boundW + displacementX;
                symmetryX.y = -(boundH + displacementY);

                symmetryX = CalcSymmetry(symmetryX, data.crossX, slope);

                if (!isOverBoundY)
                {
                    FillFlipQuadFrontOverX(ref data, ref symmetryX);
                    FillFlipQuadBackOverX(ref data);
                }

                progress = Mathf.Max(Mathf.Abs(crossX1) / texSize.y, progress);
            }
            if (isOverBoundY)
            {
                float crossY1 = (boundH - crossY) / slope;
                //(crossY1, boundH) 
                data.crossY.x = crossY1 + displacementX;
                data.crossY.y = boundH + displacementY;

                symmetryY.x = -(boundW + displacementX);
                symmetryY.y = boundH + displacementY;

                symmetryY = CalcSymmetry(symmetryY, data.crossY, slope);

                if (!isOverBoundX)
                {
                    FillFlipQuadFrontOverY(ref data, ref symmetryY);
                    FillFlipQuadBackOverY(ref data);
                }

                progress = Mathf.Max(Mathf.Abs(crossY1) / texSize.x, progress);
            }

            if (isOverBoundX && isOverBoundY)
            {
                FillFlipDiamondOverBound(ref data, ref symmetryX, ref symmetryY);
                FillFlipTriangleOverBound(ref data);
            }
        }
        else
        {
            FillFlipDiamond(ref data);
            FillFlipTriangle(ref data);
        }

        return progress;
    }

    void FillFlipDiamond(ref FillFlipData flip)
    {
        DiamondData data;

        if (0 < flip.dragDir.x && 0 < flip.dragDir.y)
        {
            // 0 < x  0 < y
            data.pt1.x = flip.crossY.x;
            data.pt1.y = flip.crossY.y;

            data.pt2.x = -halfSize.x;
            data.pt2.y = halfSize.y;

            data.pt3.x = halfSize.x;
            data.pt3.y = halfSize.y;

            data.pt4.x = flip.crossX.x;
            data.pt4.y = flip.crossX.y;

            data.pt5.x = halfSize.x;
            data.pt5.y = -halfSize.y;
        }
        else if (0 < flip.dragDir.x && flip.dragDir.y < 0)
        {
            // 0 < x  y < 0
            data.pt1.x = flip.crossX.x;
            data.pt1.y = flip.crossX.y;

            data.pt2.x = halfSize.x;
            data.pt2.y = halfSize.y;

            data.pt3.x = halfSize.x;
            data.pt3.y = -halfSize.y;

            data.pt4.x = flip.crossY.x;
            data.pt4.y = flip.crossY.y;

            data.pt5.x = -halfSize.x;
            data.pt5.y = -halfSize.y;
        }
        else if (flip.dragDir.x < 0 && flip.dragDir.y < 0)
        {
            // x < 0  y < 0
            data.pt1.x = flip.crossY.x;
            data.pt1.y = flip.crossY.y;

            data.pt2.x = halfSize.x;
            data.pt2.y = -halfSize.y;

            data.pt3.x = -halfSize.x;
            data.pt3.y = -halfSize.y;

            data.pt4.x = flip.crossX.x;
            data.pt4.y = flip.crossX.y;

            data.pt5.x = -halfSize.x;
            data.pt5.y = halfSize.y;
        }
        //else if(dragDir.x < 0 && 0 < dragDir.y)
        else
        {
            // x < 0  0 < y
            data.pt1.x = flip.crossX.x;
            data.pt1.y = flip.crossX.y;

            data.pt2.x = -halfSize.x;
            data.pt2.y = -halfSize.y;

            data.pt3.x = -halfSize.x;
            data.pt3.y = halfSize.y;

            data.pt4.x = flip.crossY.x;
            data.pt4.y = flip.crossY.y;

            data.pt5.x = halfSize.x;
            data.pt5.y = halfSize.y;
        }

        data.uv1.x = (data.pt1.x + halfSize.x) / texSize.x;
        data.uv1.y = (data.pt1.y + halfSize.y) / texSize.y;
        data.uv2.x = (data.pt2.x + halfSize.x) / texSize.x;
        data.uv2.y = (data.pt2.y + halfSize.y) / texSize.y;
        data.uv3.x = (data.pt3.x + halfSize.x) / texSize.x;
        data.uv3.y = (data.pt3.y + halfSize.y) / texSize.y;
        data.uv4.x = (data.pt4.x + halfSize.x) / texSize.x;
        data.uv4.y = (data.pt4.y + halfSize.y) / texSize.y;
        data.uv5.x = (data.pt5.x + halfSize.x) / texSize.x;
        data.uv5.y = (data.pt5.y + halfSize.y) / texSize.y;

        FillDiamond(ref mCardBack, ref data);
    }

    void FillDiamond(ref Mesh diamond, ref DiamondData data)
    {
        _vertices[0].pos.x = data.pt1.x;
        _vertices[0].pos.y = data.pt1.y;
        _vertices[0].uv.x = data.uv1.x;
        _vertices[0].uv.y = data.uv1.y;

        _vertices[1].pos.x = data.pt2.x;
        _vertices[1].pos.y = data.pt2.y;
        _vertices[1].uv.x = data.uv2.x;
        _vertices[1].uv.y = data.uv2.y;

        _vertices[2].pos.x = data.pt3.x;
        _vertices[2].pos.y = data.pt3.y;
        _vertices[2].uv.x = data.uv3.x;
        _vertices[2].uv.y = data.uv3.y;

        _vertices[3].pos.x = data.pt4.x;
        _vertices[3].pos.y = data.pt4.y;
        _vertices[3].uv.x = data.uv4.x;
        _vertices[3].uv.y = data.uv4.y;

        _vertices[4].pos.x = data.pt5.x;
        _vertices[4].pos.y = data.pt5.y;
        _vertices[4].uv.x = data.uv5.x;
        _vertices[4].uv.y = data.uv5.y;

        MeshUpdateFlags flag = GetMeshUpdateFlags();

        const int indexCount = 9;
        const int vertexCount = 5;

        // 設定 Mesh Topologiy
        SubMeshDescriptor desc = new SubMeshDescriptor(0, indexCount, MeshTopology.Triangles);
        diamond.SetSubMesh(0, desc, flag);

        // 宣告 index buffer 結構
        diamond.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

        // 宣告 vertex buffer 結構
        diamond.SetVertexBufferParams(vertexCount, _layouts);

        diamond.SetVertexBufferData(_vertices, 0, 0, vertexCount, 0, flag);
        diamond.SetIndexBufferData(_indices, 0, 0, indexCount, flag);
    }

    void FillFlipTriangle(ref FillFlipData flip)
    {
        TriangleData data;

        data.pt1.x = flip.dragPt.x;
        data.pt1.y = flip.dragPt.y;
        data.uv1.x = flip.dragDir.x < 0 ? 0f : 1f;
        data.uv1.y = flip.dragDir.y < 0 ? 1f : 0f;

        if (Mathf.Sign(flip.dragDir.x) == Mathf.Sign(flip.dragDir.y))
        {
            data.pt2.x = flip.crossX.x;
            data.pt2.y = flip.crossX.y;
            data.pt3.x = flip.crossY.x;
            data.pt3.y = flip.crossY.y;
        }
        else
        {
            data.pt2.x = flip.crossY.x;
            data.pt2.y = flip.crossY.y;
            data.pt3.x = flip.crossX.x;
            data.pt3.y = flip.crossX.y;
        }

        data.uv2.x = 1f - ((data.pt2.x + halfSize.x) / texSize.x);
        data.uv2.y = (data.pt2.y + halfSize.y) / texSize.y;
        data.uv3.x = 1f - ((data.pt3.x + halfSize.x) / texSize.x);
        data.uv3.y = (data.pt3.y + halfSize.y) / texSize.y;


        FillTriangle(ref mCardFront, ref data);
    }

    void FillTriangle(ref Mesh triangle, ref TriangleData data)
    {
        _vertices[0].pos.x = data.pt1.x;
        _vertices[0].pos.y = data.pt1.y;
        _vertices[0].uv.x = data.uv1.x;
        _vertices[0].uv.y = data.uv1.y;

        _vertices[1].pos.x = data.pt2.x;
        _vertices[1].pos.y = data.pt2.y;
        _vertices[1].uv.x = data.uv2.x;
        _vertices[1].uv.y = data.uv2.y;

        _vertices[2].pos.x = data.pt3.x;
        _vertices[2].pos.y = data.pt3.y;
        _vertices[2].uv.x = data.uv3.x;
        _vertices[2].uv.y = data.uv3.y;

        MeshUpdateFlags flag = GetMeshUpdateFlags();

        const int indexCount = 3;
        const int vertexCount = 3;

        // 設定 Mesh Topologiy
        SubMeshDescriptor desc = new SubMeshDescriptor(0, indexCount, MeshTopology.Triangles);
        triangle.SetSubMesh(0, desc, flag);

        // 宣告 index buffer 結構
        triangle.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

        // 宣告 vertex buffer 結構
        triangle.SetVertexBufferParams(vertexCount, _layouts);

        triangle.SetVertexBufferData(_vertices, 0, 0, vertexCount, 0, flag);
        triangle.SetIndexBufferData(_indices, 0, 0, indexCount, flag);
    }

    void FillFlipQuadFrontOverX(ref FillFlipData flip, ref Vector2 symmerty)
    {
        QuadData data;

        if (Mathf.Sign(flip.dragDir.x) == Mathf.Sign(flip.dragDir.y))
        {
            data.pt1.x = flip.dragPt.x;
            data.pt1.y = flip.dragPt.y;

            data.pt2.x = symmerty.x;
            data.pt2.y = symmerty.y;

            data.pt3.x = flip.crossX.x;
            data.pt3.y = flip.crossX.y;

            data.pt4.x = flip.crossY.x;
            data.pt4.y = flip.crossY.y;

            data.uv1.x = (0 < flip.dragDir.x ? 1f : 0f);
            data.uv2.x = (0 < flip.dragDir.x ? 0f : 1f);
        }
        else
        {
            data.pt1.x = symmerty.x;
            data.pt1.y = symmerty.y;

            data.pt2.x = flip.dragPt.x;
            data.pt2.y = flip.dragPt.y;

            data.pt3.x = flip.crossY.x;
            data.pt3.y = flip.crossY.y;

            data.pt4.x = flip.crossX.x;
            data.pt4.y = flip.crossX.y;

            data.uv1.x = (0 < flip.dragDir.x ? 0f : 1f);
            data.uv2.x = (0 < flip.dragDir.x ? 1f : 0f);
        }

        data.uv1.y = (0 < flip.dragDir.y ? 0f : 1f);
        data.uv2.y = (0 < flip.dragDir.y ? 0f : 1f);

        data.uv3.x = 1f - (data.pt3.x + halfSize.x) / texSize.x;
        data.uv3.y = (data.pt3.y + halfSize.y) / texSize.y;

        data.uv4.x = 1f - (data.pt4.x + halfSize.x) / texSize.x;
        data.uv4.y = (data.pt4.y + halfSize.y) / texSize.y;

        FillQuad(ref mCardFront, ref data);
    }

    void FillFlipQuadFrontOverY(ref FillFlipData flip, ref Vector2 symmerty)
    {
        QuadData data;

        if (Mathf.Sign(flip.dragDir.x) == Mathf.Sign(flip.dragDir.y))
        {
            data.pt1.x = symmerty.x;
            data.pt1.y = symmerty.y;

            data.pt2.x = flip.dragPt.x;
            data.pt2.y = flip.dragPt.y;

            data.pt3.x = flip.crossX.x;
            data.pt3.y = flip.crossX.y;

            data.pt4.x = flip.crossY.x;
            data.pt4.y = flip.crossY.y;

            data.uv1.x = (0 < flip.dragDir.x ? 1f : 0f);
            data.uv1.y = (0 < flip.dragDir.y ? 1f : 0f);
            data.uv2.x = (0 < flip.dragDir.x ? 1f : 0f);
            data.uv2.y = (0 < flip.dragDir.y ? 0f : 1f);
        }
        else
        {
            data.pt1.x = flip.dragPt.x;
            data.pt1.y = flip.dragPt.y;

            data.pt2.x = symmerty.x;
            data.pt2.y = symmerty.y;

            data.pt3.x = flip.crossY.x;
            data.pt3.y = flip.crossY.y;

            data.pt4.x = flip.crossX.x;
            data.pt4.y = flip.crossX.y;

            data.uv1.x = (0 < flip.dragDir.x ? 1f : 0f);
            data.uv1.y = (0 < flip.dragDir.y ? 0f : 1f);
            data.uv2.x = (0 < flip.dragDir.x ? 1f : 0f);
            data.uv2.y = (0 < flip.dragDir.y ? 1f : 0f);
        }

        data.uv3.x = 1f - (data.pt3.x + halfSize.x) / texSize.x;
        data.uv3.y = (data.pt3.y + halfSize.y) / texSize.y;

        data.uv4.x = 1f - (data.pt4.x + halfSize.x) / texSize.x;
        data.uv4.y = (data.pt4.y + halfSize.y) / texSize.y;

        FillQuad(ref mCardFront, ref data);
    }

    void FillFlipQuadBackOverX(ref FillFlipData flip)
    {
        QuadData data;

        if (Mathf.Sign(flip.dragDir.x) == Mathf.Sign(flip.dragDir.y))
        {
            data.pt1.x = (0 < flip.dragDir.x ? -halfSize.x : halfSize.x);
            data.pt1.y = (0 < flip.dragDir.y ? halfSize.y : -halfSize.y);

            data.pt2.x = (0 < flip.dragDir.x ? halfSize.x : -halfSize.x);
            data.pt2.y = (0 < flip.dragDir.y ? halfSize.y : -halfSize.y);

            data.pt3.x = flip.crossX.x;
            data.pt3.y = flip.crossX.y;

            data.pt4.x = flip.crossY.x;
            data.pt4.y = flip.crossY.y;
        }
        else
        {
            data.pt1.x = (0 < flip.dragDir.x ? halfSize.x : -halfSize.x);
            data.pt1.y = (0 < flip.dragDir.y ? halfSize.y : -halfSize.y);

            data.pt2.x = (0 < flip.dragDir.x ? -halfSize.x : halfSize.x);
            data.pt2.y = (0 < flip.dragDir.y ? halfSize.y : -halfSize.y);

            data.pt3.x = flip.crossY.x;
            data.pt3.y = flip.crossY.y;

            data.pt4.x = flip.crossX.x;
            data.pt4.y = flip.crossX.y;
        }

        data.uv1.x = (data.pt1.x + halfSize.x) / texSize.x;
        data.uv1.y = (data.pt1.y + halfSize.y) / texSize.y;

        data.uv2.x = (data.pt2.x + halfSize.x) / texSize.x;
        data.uv2.y = (data.pt2.y + halfSize.y) / texSize.y;

        data.uv3.x = (data.pt3.x + halfSize.x) / texSize.x;
        data.uv3.y = (data.pt3.y + halfSize.y) / texSize.y;

        data.uv4.x = (data.pt4.x + halfSize.x) / texSize.x;
        data.uv4.y = (data.pt4.y + halfSize.y) / texSize.y;

        //Debug.LogFormat("pt1 = {0}, pt2 = {1}, pt3 = {2}, pt4 = {3}", data.pt1, data.pt2, data.pt3, data.pt4);
        //Debug.LogFormat("uv1 = {0}, uv2 = {1}, uv3 = {2}, uv4 = {3}", data.uv1, data.uv2, data.uv3, data.uv4);

        FillQuad(ref mCardBack, ref data);
    }

    void FillFlipQuadBackOverY(ref FillFlipData flip)
    {
        QuadData data;

        if (Mathf.Sign(flip.dragDir.x) == Mathf.Sign(flip.dragDir.y))
        {
            data.pt1.x = (0 < flip.dragDir.x ? halfSize.x : -halfSize.x);
            data.pt1.y = (0 < flip.dragDir.y ? halfSize.y : -halfSize.y);

            data.pt2.x = (0 < flip.dragDir.x ? halfSize.x : -halfSize.x);
            data.pt2.y = (0 < flip.dragDir.y ? -halfSize.y : halfSize.y);

            data.pt3.x = flip.crossX.x;
            data.pt3.y = flip.crossX.y;

            data.pt4.x = flip.crossY.x;
            data.pt4.y = flip.crossY.y;
        }
        else
        {
            data.pt1.x = (0 < flip.dragDir.x ? halfSize.x : -halfSize.x);
            data.pt1.y = (0 < flip.dragDir.y ? -halfSize.y : halfSize.y);

            data.pt2.x = (0 < flip.dragDir.x ? halfSize.x : -halfSize.x);
            data.pt2.y = (0 < flip.dragDir.y ? halfSize.y : -halfSize.y);

            data.pt3.x = flip.crossY.x;
            data.pt3.y = flip.crossY.y;

            data.pt4.x = flip.crossX.x;
            data.pt4.y = flip.crossX.y;
        }

        data.uv1.x = (data.pt1.x + halfSize.x) / texSize.x;
        data.uv1.y = (data.pt1.y + halfSize.y) / texSize.y;

        data.uv2.x = (data.pt2.x + halfSize.x) / texSize.x;
        data.uv2.y = (data.pt2.y + halfSize.y) / texSize.y;

        data.uv3.x = (data.pt3.x + halfSize.x) / texSize.x;
        data.uv3.y = (data.pt3.y + halfSize.y) / texSize.y;

        data.uv4.x = (data.pt4.x + halfSize.x) / texSize.x;
        data.uv4.y = (data.pt4.y + halfSize.y) / texSize.y;

        FillQuad(ref mCardBack, ref data);
    }

    void FillFlipDiamondOverBound(ref FillFlipData flip, ref Vector2 symmertyX, ref Vector2 symmertyY)
    {
        DiamondData data;

        if (Mathf.Sign(flip.dragDir.x) == Mathf.Sign(flip.dragDir.y))
        {
            data.pt1.x = flip.crossY.x;
            data.pt1.y = flip.crossY.y;

            data.pt2.x = symmertyY.x;
            data.pt2.y = symmertyY.y;

            data.pt3.x = flip.dragPt.x;
            data.pt3.y = flip.dragPt.y;

            data.pt4.x = flip.crossX.x;
            data.pt4.y = flip.crossX.y;

            data.pt5.x = symmertyX.x;
            data.pt5.y = symmertyX.y;

            data.uv2.x = (0 < flip.dragDir.x ? 1f : 0f);
            data.uv2.y = (0 < flip.dragDir.y ? 1f : 0f);
            data.uv5.x = (0 < flip.dragDir.x ? 0f : 1f);
            data.uv5.y = (0 < flip.dragDir.y ? 0f : 1f);
        }
        else
        {
            data.pt1.x = flip.crossX.x;
            data.pt1.y = flip.crossX.y;

            data.pt2.x = symmertyX.x;
            data.pt2.y = symmertyX.y;

            data.pt3.x = flip.dragPt.x;
            data.pt3.y = flip.dragPt.y;

            data.pt4.x = flip.crossY.x;
            data.pt4.y = flip.crossY.y;

            data.pt5.x = symmertyY.x;
            data.pt5.y = symmertyY.y;

            data.uv2.x = (0 < flip.dragDir.x ? 0f : 1f);
            data.uv2.y = (0 < flip.dragDir.y ? 0f : 1f);
            data.uv5.x = (0 < flip.dragDir.x ? 1f : 0f);
            data.uv5.y = (0 < flip.dragDir.y ? 1f : 0f);
        }

        data.uv1.x = 1f - (data.pt1.x + halfSize.x) / texSize.x;
        data.uv1.y = (data.pt1.y + halfSize.y) / texSize.y;

        data.uv3.x = (0 < flip.dragDir.x ? 1f : 0f);
        data.uv3.y = (0 < flip.dragDir.y ? 0f : 1f);

        data.uv4.x = 1f - (data.pt4.x + halfSize.x) / texSize.x;
        data.uv4.y = (data.pt4.y + halfSize.y) / texSize.y;

        FillDiamond(ref mCardFront, ref data);
    }

    void FillFlipTriangleOverBound(ref FillFlipData flip)
    {
        TriangleData data;

        data.pt1.x = (0 < flip.dragDir.x ? halfSize.x : -halfSize.x);
        data.pt1.y = (0 < flip.dragDir.y ? halfSize.y : -halfSize.y);

        if (Mathf.Sign(flip.dragDir.x) == Mathf.Sign(flip.dragDir.y))
        {
            data.pt2.x = flip.crossX.x;
            data.pt2.y = flip.crossX.y;
            data.pt3.x = flip.crossY.x;
            data.pt3.y = flip.crossY.y;
        }
        else
        {
            data.pt2.x = flip.crossY.x;
            data.pt2.y = flip.crossY.y;
            data.pt3.x = flip.crossX.x;
            data.pt3.y = flip.crossX.y;
        }

        data.uv1.x = ((data.pt1.x + halfSize.x) / texSize.x);
        data.uv1.y = (data.pt1.y + halfSize.y) / texSize.y;
        data.uv2.x = ((data.pt2.x + halfSize.x) / texSize.x);
        data.uv2.y = (data.pt2.y + halfSize.y) / texSize.y;
        data.uv3.x = ((data.pt3.x + halfSize.x) / texSize.x);
        data.uv3.y = (data.pt3.y + halfSize.y) / texSize.y;


        FillTriangle(ref mCardBack, ref data);
    }

    void FillFrontOnly()
    {
        QuadData data;

        data.pt1.x = -halfSize.x;
        data.pt1.y = -halfSize.y;
        data.pt2.x = -halfSize.x;
        data.pt2.y = halfSize.y;
        data.pt3.x = halfSize.x;
        data.pt3.y = halfSize.y;
        data.pt4.x = halfSize.x;
        data.pt4.y = -halfSize.y;

        data.uv1.x = 0f;
        data.uv1.y = 0f;
        data.uv2.x = 0f;
        data.uv2.y = 1f;
        data.uv3.x = 1f;
        data.uv3.y = 1f;
        data.uv4.x = 1f;
        data.uv4.y = 0f;

        FillQuad(ref mCardFront, ref data);


        data.pt1.x = 0f;
        data.pt1.y = 0f;
        data.pt2.x = 0f;
        data.pt2.y = 0f;
        data.pt3.x = 0f;
        data.pt3.y = 0f;
        data.pt4.x = 0f;
        data.pt4.y = 0f;

        data.uv1.x = 0f;
        data.uv1.y = 0f;
        data.uv2.x = 0f;
        data.uv2.y = 1f;
        data.uv3.x = 1f;
        data.uv3.y = 1f;
        data.uv4.x = 1f;
        data.uv4.y = 0f;

        FillQuad(ref mCardBack, ref data);
    }

    void FlopBefore(Vector2 worldSize, Vector2 dragDir)
    {
        float progress = 0f;

        if (dragDir.x == 0 || dragDir.y == 0)
        {
            progress = FillFlipStraight(dragDir);
        }
        else
        {
            progress = FillFlipSlope(dragDir);
        }

        if (0.75f < progress)
        {
            isFlop = true;
        }

        Debug.LogFormat("progress = {0}", progress);
    }

    void FlopAfter()
    {
        FillFrontOnly();
    }
}
