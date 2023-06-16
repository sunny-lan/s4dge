#ifndef RAY_TETRAHEDRA_INTERSECTION
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
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

void _tmp_calc(float s_i, float d_i, inout float min_t, inout float max_t)
{
	if (d_i > 0) min_t = max(min_t, -s_i / d_i);
	else if (d_i < 0) max_t = min(max_t, -s_i / d_i);
	else if (s_i < 0) max_t = -1;
}

HitInfo intersection_ray_simplex(Ray r)
{
	HitInfo result = (HitInfo)0;

	float4 dir = r.dir;
	float4 st = r.origin;
	float sum_d_i = dir.x + dir.y + dir.z + dir.w;
	float sum_s_i = st.x + st.y + st.z + st.w;

	if (sum_d_i == 0)
	{
		if(sum_s_i != 1)
		{
			result.didHit = false;
			return result;
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
			return result;
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

	return result;
}

// A tetrahedron in 4D is bounded
// by 4 hyperplane inequalities for each of the 3D faces
// and 1 hyperplane equality for the volume

// The 3D equivalent is how a triangle in 3D
// is bounded by 3 lines on a plane.

#include "Matrix.hlsl"
#include "Hyperplane.hlsl"
struct Tet
{
	// Stored in 1 column per vertex
	float4x4 vertices;

	Hyperplane edges[4];
	Hyperplane volume;

	int direction[4];

	void from_points(float4x4 vertices) {
		volume.from_points(vertices);

		for (uint i = 0; i < 4; i++) {
			// We want each edge to be orthogonal to the normal
			float3x4 tmp = {
				volume.normal,
				vertices[(i+1)%4] - vertices[i],
				vertices[(i+2)%4] - vertices[i]
			};
			edges[i].normal = cross_product(tmp);
			edges[i].offset = dot(edges[i].normal, vertices[i]);

			direction[i] = dot(edges[i].normal, vertices[(i + 3) % 4]) < edges[i].offset ? -1 : 1;
		}

	}

	// Determines the range of t that a ray intersects this
	void intersection(Ray r, inout float min_t, inout float max_t)
	{
		float min_tmp, max_tmp;
		volume.intersection(r, 0, min_tmp, max_tmp);
		min_t = max(min_t, min_tmp);
		max_t = min(max_t, max_tmp);

		for (int i = 0; i < 4; i++) {
			edges[i].intersection(r, direction[i], min_tmp, max_tmp);
			min_t = max(min_t, min_tmp);
			max_t = min(max_t, max_tmp);
		}
	}
};


#endif