#ifndef _OPEN_SIMPLEX_NOISE_H
#define _OPEN_SIMPLEX_NOISE_H

#define STRETCH_CONSTANT_2D -0.211324865405187    // (1/Math.sqrt(2+1)-1)/2;
#define SQUISH_CONSTANT_2D   0.366025403784439    // (Math.sqrt(2+1)-1)/2;

uniform uint PSIZE;
uniform uint PMASK;

StructuredBuffer<int> perm;
StructuredBuffer<float2> permGrad2;

float extrapolate(int2 sb, float2 d)
{
	float2 grad = permGrad2[perm[sb.x & PMASK] ^ (sb.y & PMASK)];
	return grad.x * d.x + grad.y * d.y;
}

float SimplexNoise(float2 pos)
{
	// Place input coordinates onto grid.
	float stretchOffset = (pos.x + pos.y) * STRETCH_CONSTANT_2D;
	float2 xpos = pos + float2(stretchOffset, stretchOffset);

	// Floor to get grid coordinates of rhombus (stretched square) super-cell origin.
	int2 sb = floor(xpos);

	// Compute grid coordinates relative to rhombus origin.
	float2 ins = xpos - sb;

	// Sum those together to get a value that determines which region we're in.
	float inSum = ins.x + ins.y;

	// Positions relative to origin point.
	float squishOffsetIns = inSum * SQUISH_CONSTANT_2D;
	float2 d = ins + float2(squishOffsetIns, squishOffsetIns);

	// We'll be defining these inside the next block and using them afterwards.
	float2 d_ext;
	int2 sv_ext;

	float value = 0;

	// Contribution (1,0)
	float2 d1 = d - float2(1, 0) - float2(SQUISH_CONSTANT_2D, SQUISH_CONSTANT_2D);
	float attn1 = 2 - d1.x * d1.x - d1.y * d1.y;
	if (attn1 > 0) {
		attn1 *= attn1;
		value += attn1 * attn1 * extrapolate(sb + int2(1, 0), d1);
	}

	// Contribution (0,1)
	float2 d2 = d - float2(0, 1) - float2(SQUISH_CONSTANT_2D, SQUISH_CONSTANT_2D);
	float attn2 = 2 - d2.x * d2.x - d2.y * d2.y;
	if (attn2 > 0) {
		attn2 *= attn2;
		value += attn2 * attn2 * extrapolate(sb + int2(0, 1), d2);
	}

	if (inSum <= 1) { // We're inside the triangle (2-Simplex) at (0,0)
		float zins = 1 - inSum;
		if (zins > ins.x || zins > ins.y) { // (0,0) is one of the closest two triangular vertices
			if (ins.x > ins.y) {
				sv_ext = sb + int2(1, -1);
				d_ext = d + float2(-1, 1);
			}
			else {
				sv_ext = sb + int2(-1, 1);
				d_ext = d + float2(1, -1);
			}
		}
		else { // (1,0) and (0,1) are the closest two vertices.
			sv_ext = sb + int2(1, 1);
			d_ext = d - float2(1, 1) - 2 * float2(SQUISH_CONSTANT_2D, SQUISH_CONSTANT_2D);
		}
	}
	else { // We're inside the triangle (2-Simplex) at (1,1)
		float zins = 2 - inSum;
		if (zins < ins.x || zins < ins.y) { // (0,0) is one of the closest two triangular vertices
			if (ins.x > ins.y) {
				sv_ext = sb + int2(2, 0);
				d_ext = d - float2(2, 0) - 2 * float2(SQUISH_CONSTANT_2D, SQUISH_CONSTANT_2D);
			}
			else {
				sv_ext = sb + int2(0, 2);
				d_ext = d - float2(0, 2) - 2 * float2(SQUISH_CONSTANT_2D, SQUISH_CONSTANT_2D);
			}
		}
		else { // (1,0) and (0,1) are the closest two vertices.
			d_ext = d;
			sv_ext = sb;
		}
		sb += int2(1, 1);
		d = d - float2(1, 1) - 2 * float2(SQUISH_CONSTANT_2D, SQUISH_CONSTANT_2D);
	}

	// Contribution (0,0) or (1,1)
	float attn0 = 2 - d.x * d.x - d.y * d.y;
	if (attn0 > 0) {
		attn0 *= attn0;
		value += attn0 * attn0 * extrapolate(sb, d);
	}

	// Extra Vertex
	float attn_ext = 2 - d_ext.x * d_ext.x - d_ext.y * d_ext.y;
	if (attn_ext > 0) {
		attn_ext *= attn_ext;
		value += attn_ext * attn_ext * extrapolate(sv_ext, d_ext);
	}

	return value;
}

float SimplexFbm(float2 pos)
{	
	float sum = 0;
	float freq = 0.0005, amp = 1.0;
	for (int i = 0; i < 8; i++)
	{
		float n = SimplexNoise(pos * freq + (i / 32.0));
		sum += n * amp;
		freq *= 2;
		amp *= 0.5;
	}

	return sum;
}

#endif
