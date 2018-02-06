using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RainMesh : MonoBehaviour 
{
	private const int TRIANGLE_COUNT = 1024;
	private const float TRIANGLE_SCALE = 0.002f;

	// Use this for initialization
	void Start () 
	{
		Initialize ();
	}
	
	void Initialize()
	{
		List<Vector3> vertices = new List<Vector3> (TRIANGLE_COUNT*3);
		List<int> indices = new List<int> (TRIANGLE_COUNT*3);

		int pos = 0;
		Vector3 center = Vector3.one * 0.5f;
		Vector3 meshScale = new Vector3(20f,20f,20f);
		for (int Indx = 0; Indx < TRIANGLE_COUNT; ++Indx) 
		{
			Vector3 v1 = Vector3.Scale (new Vector3(Random.value,Random.value,Random.value) - center, meshScale);
			Vector3 v2 = v1 + new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f) * TRIANGLE_SCALE;
			Vector3 v3 = v1 + new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f) * TRIANGLE_SCALE;

			vertices.Add (v1);
			vertices.Add (v2);
			vertices.Add (v3);

			indices.Add (pos + 0);
			indices.Add (pos + 1);
			indices.Add (pos + 2);

			pos += 3;
		}

		Mesh mesh = new Mesh ();
		mesh.SetVertices (vertices);
		mesh.SetIndices (indices.ToArray(), MeshTopology.Triangles, 0);
		mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 99999999);

		this.GetComponent<MeshFilter> ().sharedMesh = mesh;
	}
}
