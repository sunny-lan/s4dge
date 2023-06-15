#ifndef RAY_TETRAHEDRA_INTERSECTION
#define RAY_TETRAHEDRA_INTERSECTION

#include "RayTracingStructs.cginc"

// A simplex is defined as x,y,z,w>=0 and x+y+z+w=1
// This is the most basic form of a tetrahedron in 4D

// For a ray, p_i(t) = s_i + d_i*t
// so, p_i(t) >= 0 for all i
// Which means
//	d_i*t >= -s_i
//  t >= -s_i/d_i       (if d_i>0)
//  t <= -s_i/d_i       (if d_i<0)
//  t can be any number (if d_i=0 and s_i >= 0)
//  no intersection     (if d_i=0 and s_i < 0)

// Also, sum p_i for i in xyzw = 1, which means
//	sum (s_i + d_i*t) = 1
//  sum s_i + t * sum d_i = 1
//  t * sum d_i = 1 - sum s_i
// 
// The solutions are
//  t = (1 - sum s_i)/(sum d_i) (if sum d_i != 0)
//  no intersection				(if sum d_i = 0 and sum s_i != 1)
//  any number                  (if sum d_i = 0 and sum s_i = 1)

// The intersection is the set of all t that satisfy the above.
// The solution is never 'any number' unless the ray has no direction.

void _tmp_calc(float s_i, float d_i, out float min_t, out float max_t)
{
	if (d_i > 0) min_t = max(min_t, -s_i / d_i);
	else if (d_i < 0) max_t = min(max_t, -s_i / d_i);
	else if (s_i < 0) max_t = -1;
}

void intersection_ray_simplex(Ray r, inout HitInfo result)
{
	float4 dir = r.dir;
	float4 st = r.origin;
	float sum_d_i = dir.x + dir.y + dir.z + dir.w;
	float sum_s_i = st.x + st.y + st.z + st.w;

	if (sum_d_i == 0)
	{
		if(sum_s_i != 1)
		{
			result.didHit = false;
			return;
		}

		float min_t = 0;
		float max_t = 1.#INF;
		_tmp_calc(st.x, dir.x, min_t, max_t);
		_tmp_calc(st.y, dir.y, min_t, max_t);
		_tmp_calc(st.z, dir.z, min_t, max_t);
		_tmp_calc(st.w, dir.w, min_t, max_t);

		if (min_t > max_t)
		{
			result.didHit = false;
			return;
		}

		result.didHit = true;
		result.dst = min_t;
	}
	else 
	{
		float t = (1 - sum_s_i) / (sum_d_i);
		result.didHit = true;
		result.didHit = result.didHit && (dir.x * t + st.x >=0);
		result.didHit = result.didHit && (dir.y * t + st.y >=0);
		result.didHit = result.didHit && (dir.z * t + st.z >=0);
		result.didHit = result.didHit && (dir.w * t + st.w >=0);
		result.dst = t;
	}
}

#endif