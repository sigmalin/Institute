using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InfiniteOceanSimulator
{
	[System.Serializable]
	public class OceanPlane
	{
		public Transform TransOcean;
		public MeshFilter MeshOcean;
	}

	[SerializeField]OceanPlane mOceanPlane;

	public Mesh MeshOcean { private set; get; }

	const float OCEAN_PLANE_FOV = 60f;
	const float OCEAN_PLANE_RADIUS = 40f;
	const float OCEAN_PLANE_LOD_THRESHOLD = 20f;
	const float OCEAN_PLANE_DISTANCE_NEAR = 5f;
	const float OCEAN_PLANE_ANGLE_GAP = 1f;
	const float OCEAN_PLANE_LENGTH_GAP = 0.3f;	
	const float OCEAN_PLANE_LOD_LENGTH_GAP = 1f;	

	const float OCEAN_PLANE_HIT_LENGTH = 20f;
	const float NORMALIZE_OCEAN_PLANE_HIT_LENGTH = 1f / OCEAN_PLANE_HIT_LENGTH;
	readonly float EULER_2_THETA = Mathf.PI / 180.0f;	

	void InitOceanPlane()
	{
		ReleaseOceanPlane();

		List<Vector3> vertices;
		List<int> indices;

		InitOceanVertices(out vertices);
		InitOceanIndices(out indices);

		MeshOcean = new Mesh();
		MeshOcean.SetVertices(vertices);
		MeshOcean.SetTriangles (indices, 0);

		MeshOcean.RecalculateNormals();
		MeshOcean.RecalculateTangents();

		mOceanPlane.MeshOcean.sharedMesh = MeshOcean;
	}

	void ReleaseOceanPlane()
	{
		if(mOceanPlane != null)
		{
			if(mOceanPlane.MeshOcean != null)
				mOceanPlane.MeshOcean.sharedMesh = null;
		}

		if(MeshOcean != null) 
		{
			MeshOcean.Clear();
			MeshOcean = null;
		}
	}

	void InitOceanVertices(out List<Vector3> _vertices)
	{
		int gapCount = Mathf.FloorToInt(OCEAN_PLANE_FOV / OCEAN_PLANE_ANGLE_GAP) + 1;
		int lengthCount = Mathf.FloorToInt((OCEAN_PLANE_LOD_THRESHOLD - OCEAN_PLANE_DISTANCE_NEAR) / OCEAN_PLANE_LENGTH_GAP) + 1;
		int verticesCount = gapCount * lengthCount;

		
		lengthCount = Mathf.FloorToInt((OCEAN_PLANE_RADIUS - OCEAN_PLANE_LOD_THRESHOLD) / OCEAN_PLANE_LOD_LENGTH_GAP);
		verticesCount += gapCount * lengthCount;

		_vertices = new List<Vector3>(verticesCount);

		float length = OCEAN_PLANE_DISTANCE_NEAR;		
		while(length <= OCEAN_PLANE_LOD_THRESHOLD)
		{
			float endAngle = OCEAN_PLANE_FOV * 0.5f;
			float angle = endAngle * -1f;
			while(angle <= endAngle)
			{
				float rad = angle * EULER_2_THETA;
				Vector3 vert = new Vector3(length * Mathf.Sin(rad), 0f, length * Mathf.Cos(rad));

				_vertices.Add(vert);
				angle += OCEAN_PLANE_ANGLE_GAP;
			}	

			length += OCEAN_PLANE_LENGTH_GAP;		
		}

		// LOD
		while(length <= OCEAN_PLANE_RADIUS)
		{
			float endAngle = OCEAN_PLANE_FOV * 0.5f;
			float angle = endAngle * -1f;
			while(angle <= endAngle)
			{
				float rad = angle * EULER_2_THETA;
				Vector3 vert = new Vector3(length * Mathf.Sin(rad), 0f, length * Mathf.Cos(rad));

				_vertices.Add(vert);
				angle += OCEAN_PLANE_ANGLE_GAP;
			}	

			length += OCEAN_PLANE_LOD_LENGTH_GAP;		
		}
	}

	void InitOceanIndices(out List<int> _indices)
	{
		int gapCount = Mathf.FloorToInt(OCEAN_PLANE_FOV / OCEAN_PLANE_ANGLE_GAP);
		int lengthCount = Mathf.FloorToInt((OCEAN_PLANE_LOD_THRESHOLD - OCEAN_PLANE_DISTANCE_NEAR) / OCEAN_PLANE_LENGTH_GAP);
		int triCount = gapCount * lengthCount * 2;

		int lodLengthCount = Mathf.FloorToInt((OCEAN_PLANE_RADIUS - OCEAN_PLANE_LOD_THRESHOLD) / OCEAN_PLANE_LOD_LENGTH_GAP);
		triCount += gapCount * lodLengthCount * 2;

		lengthCount += lodLengthCount;

		_indices = new List<int>(triCount * 3);
	
		for(int IndxY = 0; IndxY < lengthCount; ++IndxY)
		{
			for(int IndxX = 0; IndxX < gapCount; ++IndxX)
			{
				int baseX = IndxX + (gapCount+1) * IndxY;
				_indices.Add(baseX);
				_indices.Add(baseX+gapCount+1);
				_indices.Add(baseX+gapCount+2);		

				_indices.Add(baseX+gapCount+2);
				_indices.Add(baseX+1);
				_indices.Add(baseX);					
			}		
		}
	}

	bool CheckHitOceanPlane(Vector3 _pt, out Vector2 _uv)
	{
		_uv = Vector2.zero;

		Vector3 diff = _pt - mCenter;

		if(Mathf.Abs(diff.x) < OCEAN_PLANE_HIT_LENGTH && Mathf.Abs(diff.z) < OCEAN_PLANE_HIT_LENGTH)
		{
			_uv.x = (diff.x * NORMALIZE_OCEAN_PLANE_HIT_LENGTH) * 0.5f + 0.5f;
			_uv.y = (diff.z * NORMALIZE_OCEAN_PLANE_HIT_LENGTH) * 0.5f + 0.5f;
			return true;
		}

		return false;
	}
}
