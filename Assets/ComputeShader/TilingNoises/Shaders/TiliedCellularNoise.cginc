#ifndef __TiliedCellularNoise_
#define __TiliedCellularNoise_

float TiliedCellularNoise2D(float2 st, float period)
{
	st *= period;

	// Tile the space
	float2 i_st = floor(st);
	float2 f_st = frac(st);

	float dist = 1.0;  // minimun distance

	for (int y = -1; y <= 1; y++)
	{
		for (int x = -1; x <= 1; x++)
		{
			// Neighbor place in the grid
			float2 neighbor = float2(x, y);

			float2 tiledCell = i_st + neighbor;
			tiledCell.x = modulo(tiledCell.x, period);
			tiledCell.y = modulo(tiledCell.y, period);

			// Random position from current + neighbor place in the grid
			float2 pt = random2(tiledCell);

			// Animate the point
			pt = sin(pt * 6.2831 + 0);
			pt = 0.5 + 0.5 * pt;

			// Vector between the pixel and the point
			float2 diff = neighbor + pt - f_st;//i_st + neighbor + point - st;

			// Distance to the point
			float d = length(diff);

			// Keep the closer distance
			dist = min(d, dist);
		}
	}

	return dist;
}

float TiliedCellularNoise3D(float3 st, float period)
{
	st *= period;

	// Tile the space
	float3 i_st = floor(st);
	float3 f_st = frac(st);

	float dist = 1.0;

	for (int k = -1; k <= 1; ++k)
	{
		for (int j = -1; j <= 1; ++j)
		{
			for (int i = -1; i <= 1; ++i)
			{
				float3 neighbor = float3(i, j, k);

				float3 tiledCell = i_st + neighbor;
				tiledCell.x = modulo(tiledCell.x, period);
				tiledCell.y = modulo(tiledCell.y, period);
				tiledCell.z = modulo(tiledCell.z, period);

				// Random position from current + neighbor place in the grid
				float3 pt = random3(tiledCell);

				// Animate the point
				pt = sin(pt * 6.2831 + 0);
				pt = 0.5 + 0.5 * pt;

				// Vector between the pixel and the point
				float3 diff = neighbor + pt - f_st;//i_st + neighbor + point - st;

				// Distance to the point
				float d = length(diff);

				// Keep the closer distance
				dist = min(d, dist);
			}
		}
	}

	return dist;
}

#endif // __TiliedCellularNoise_
