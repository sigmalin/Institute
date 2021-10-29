#ifndef UNIVERSAL_PHOTOSHOP_BLEND_INCLUDED
#define UNIVERSAL_PHOTOSHOP_BLEND_INCLUDED

real3 darken(real3 s, real3 d)
{
	return min(s, d);
}

real3 multiply(real3 s, real3 d)
{
	return s * d;
}

real3 colorBurn(real3 s, real3 d)
{
	return 1.0 - (1.0 - d) / s;
}

real3 linearBurn(real3 s, real3 d)
{
	return s + d - 1.0;
}

real3 darkerColor(real3 s, real3 d)
{
	return (s.x + s.y + s.z < d.x + d.y + d.z) ? s : d;
}

real3 lighten(real3 s, real3 d)
{
	return max(s, d);
}

real3 screen(real3 s, real3 d)
{
	return s + d - s * d;
}

real3 colorDodge(real3 s, real3 d)
{
	return d / (1.0 - s);
}

real3 linearDodge(real3 s, real3 d)
{
	return s + d;
}

real3 lighterColor(real3 s, real3 d)
{
	return (s.x + s.y + s.z > d.x + d.y + d.z) ? s : d;
}

real overlay(real s, real d)
{
	return (d < 0.5) ? 2.0 * s * d : 1.0 - 2.0 * (1.0 - s) * (1.0 - d);
}

real3 overlay(real3 s, real3 d)
{
	real3 c;
	c.x = overlay(s.x, d.x);
	c.y = overlay(s.y, d.y);
	c.z = overlay(s.z, d.z);
	return c;
}

real softLight(real s, real d)
{
	return (s < 0.5) ? d - (1.0 - 2.0 * s) * d * (1.0 - d)
		: (d < 0.25) ? d + (2.0 * s - 1.0) * d * ((16.0 * d - 12.0) * d + 3.0)
		: d + (2.0 * s - 1.0) * (sqrt(d) - d);
}

real3 softLight(real3 s, real3 d)
{
	real3 c;
	c.x = softLight(s.x, d.x);
	c.y = softLight(s.y, d.y);
	c.z = softLight(s.z, d.z);
	return c;
}

real hardLight(real s, real d)
{
	return (s < 0.5) ? 2.0 * s * d : 1.0 - 2.0 * (1.0 - s) * (1.0 - d);
}

real3 hardLight(real3 s, real3 d)
{
	real3 c;
	c.x = hardLight(s.x, d.x);
	c.y = hardLight(s.y, d.y);
	c.z = hardLight(s.z, d.z);
	return c;
}

real vividLight(real s, real d)
{
	return (s < 0.5) ? 1.0 - (1.0 - d) / (2.0 * s) : d / (2.0 * (1.0 - s));
}

real3 vividLight(real3 s, real3 d)
{
	real3 c;
	c.x = vividLight(s.x, d.x);
	c.y = vividLight(s.y, d.y);
	c.z = vividLight(s.z, d.z);
	return c;
}

real3 linearLight(real3 s, real3 d)
{
	return 2.0 * s + d - 1.0;
}

real pinLight(real s, real d)
{
	return (2.0 * s - 1.0 > d) ? 2.0 * s - 1.0 : (s < 0.5 * d) ? 2.0 * s : d;
}

real3 pinLight(real3 s, real3 d)
{
	real3 c;
	c.x = pinLight(s.x, d.x);
	c.y = pinLight(s.y, d.y);
	c.z = pinLight(s.z, d.z);
	return c;
}

real3 hardlerp(real3 s, real3 d)
{
	return floor(s + d);
}

real3 difference(real3 s, real3 d)
{
	return abs(d - s);
}

real3 exclusion(real3 s, real3 d)
{
	return s + d - 2.0 * s * d;
}

real3 subtract(real3 s, real3 d)
{
	return s - d;
}

real3 divide(real3 s, real3 d)
{
	return s / d;
}

real3 rgb2hsv(real3 c)
{
	real4 K = real4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	real4 p = lerp(real4(c.bg, K.wz), real4(c.gb, K.xy), step(c.b, c.g));
	real4 q = lerp(real4(p.xyw, c.r), real4(c.r, p.yzx), step(p.x, c.r));
	real d = q.x - min(q.w, q.y);
	real e = 1.0e-10;
	return real3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

real3 hsv2rgb(real3 c)
{
	real4 K = real4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	real3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

real3 hue(real3 s, real3 d)
{
	d = rgb2hsv(d);
	d.x = rgb2hsv(s).x;
	return hsv2rgb(d);
}

real3 color(real3 s, real3 d)
{
	s = rgb2hsv(s);
	s.z = rgb2hsv(d).z;
	return hsv2rgb(s);
}

real3 saturation(real3 s, real3 d)
{
	d = rgb2hsv(d);
	d.y = rgb2hsv(s).y;
	return hsv2rgb(d);
}

real3 luminosity(real3 s, real3 d)
{
	real dLum = dot(d, real3(0.3, 0.59, 0.11));
	real sLum = dot(s, real3(0.3, 0.59, 0.11));
	real lum = sLum - dLum;
	real3 c = d + lum;
	real minC = min(min(c.x, c.y), c.z);
	real maxC = max(max(c.x, c.y), c.z);
	if (minC < 0.0) return sLum + ((c - sLum) * sLum) / (sLum - minC);
	else if (maxC > 1.0) return sLum + ((c - sLum) * (1.0 - sLum)) / (maxC - sLum);
	else return c;
}

#endif
