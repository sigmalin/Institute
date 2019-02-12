using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ProjectionGrid 
{
	Mesh m_GridMesh;

	/// <summary>
    /// Creates the ocean mesh gameobject.
    /// The resolutions is how many pixels per quad in mesh.
    /// The higher the number the less verts in mesh.
    /// </summary>
    void CreateGrid(int resolution)
    {
		ReleaseGrid();

        int width = Screen.width;
        int height = Screen.height;
        int numVertsX = width / resolution;
        int numVertsY = height / resolution;

        m_GridMesh = CreateQuad(numVertsX, numVertsY);
    }

	void ReleaseGrid()
	{
		if(m_GridMesh != null)
		{
			m_GridMesh.Clear();
			m_GridMesh = null;
		}
	}

	public Mesh CreateQuad(int numVertsX, int numVertsY)
	{			
		Vector3[] vertices = new Vector3[numVertsX * numVertsY];
		Vector2[] texcoords = new Vector2[numVertsX * numVertsY];
		int[] indices = new int[numVertsX * numVertsY * 6];
			
		for (int x = 0; x < numVertsX; x++)
		{
			for (int y = 0; y < numVertsY; y++)
			{
                Vector2 uv = new Vector3(x / (numVertsX - 1.0f), y / (numVertsY - 1.0f));

                texcoords[x + y * numVertsX] = uv;
				vertices[x + y * numVertsX] = new Vector3(uv.x, uv.y, 0.0f);
			}
		}
			
		int num = 0;
		for (int x = 0; x < numVertsX - 1; x++)
		{
			for (int y = 0; y < numVertsY - 1; y++)
			{
				indices[num++] = x + y * numVertsX;
				indices[num++] = x + (y + 1) * numVertsX;
				indices[num++] = (x + 1) + y * numVertsX;
					
				indices[num++] = x + (y + 1) * numVertsX;
				indices[num++] = (x + 1) + (y + 1) * numVertsX;
				indices[num++] = (x + 1) + y * numVertsX;
			}
		}

        if (vertices.Length > 65000)
        {
            //Too many verts to make a mesh. 
            //You will need to split the mesh.
            return null;
        }
        else
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = texcoords;
            mesh.triangles = indices;

			//The position of the mesh is not known until its projected in the shader. 
            //Make the bounds large enough so the camera will draw it.
            float bigNumber = 1e6f;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(bigNumber, 20.0f, bigNumber));

            return mesh;
        }
	}
}
