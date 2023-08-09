#ifndef HYPERCUBE
#define HYPERCUBE

#include "RayTracingStructs.hlsl"
#include "Hyperplane.hlsl"

struct Hypercube
{
	// two opposite points of the hypercube
	Transform4D inverseTransform;
	float4 p1;
	float4 p2;
	RayTracingMaterial material;

	// Checks the 2 faces of the hypercube on given axis
	void _check_inside(Ray r, float4 axis, inout float min_t, inout float max_t)
	{
		float min_tmp, max_tmp;
		Hyperplane p;
		p.normal = axis;

		p.offset = dot(axis, p1);
		p.intersection(r, 1, min_tmp, max_tmp);
		min_t = max(min_t, min_tmp);
		max_t = min(max_t, max_tmp);

		p.offset = dot(axis, p2);
		p.intersection(r, -1, min_tmp, max_tmp);
		min_t = max(min_t, min_tmp);
		max_t = min(max_t, max_tmp);
	}

	HitInfo intersection(Ray r)
	{

		Ray localRay = TransformRay(r, inverseTransform);

		float min_t = 0;
		float max_t = 1.#INF;

		_check_inside(localRay, float4(1, 0, 0, 0), min_t, max_t);
		_check_inside(localRay, float4(0, 1, 0, 0), min_t, max_t);
		_check_inside(localRay, float4(0, 0, 1, 0), min_t, max_t);
		_check_inside(localRay, float4(0, 0, 0, 1), min_t, max_t);

		HitInfo res;
		res.didHit = min_t <= max_t;
		res.dst = min_t;
		res.numHits = 1;
		res.material = material;
		res.hitPoint = r.origin + localRay.dir * res.dst;
		res.normal = normalize(res.hitPoint);
		return res;
	}
};

#endif