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
	void _check_inside(Ray r, float4 axis, inout float min_t, inout float max_t, inout float4 normal)
	{
		float min_tmp, max_tmp;

		float old_min = min_t;
		float old_max = max_t;

		Hyperplane p;
		p.normal = axis;

		p.offset = dot(axis, p1);
		p.intersection(r, 1, min_tmp, max_tmp);
		min_t = max(min_t, min_tmp);
		max_t = min(max_t, max_tmp);

		if(min_t != old_min && min_t <= max_t){ // New closest point
			normal = p.normal;
			if(dot(p.normal, r.dir) > 0) {
				normal = normal * -1;
			}
			// normal = p.normal;
			old_min = min_t;
			old_max = max_t;
		}

		p.offset = dot(axis, p2);
		p.intersection(r, -1, min_tmp, max_tmp);
		min_t = max(min_t, min_tmp);
		max_t = min(max_t, max_tmp);

		if(min_t != old_min &&  min_t <= max_t){
			normal = p.normal;
			if(dot(p.normal, r.dir) > 0) {
				normal = normal * -1;
			}
		}
	}

	HitInfo intersection(Ray r)
	{

		Ray localRay = TransformRay(r, inverseTransform);
		float4 normal;

		float min_t = 0;
		float max_t = 1.#INF;

		_check_inside(localRay, float4(1, 0, 0, 0), min_t, max_t, normal);
		_check_inside(localRay, float4(0, 1, 0, 0), min_t, max_t, normal);
		_check_inside(localRay, float4(0, 0, 1, 0), min_t, max_t, normal);
		_check_inside(localRay, float4(0, 0, 0, 1), min_t, max_t, normal);

		HitInfo res;
		res.didHit = min_t <= max_t;
		res.dst = min_t;
		res.numHits = 1;
		res.material = material;
		res.hitPoint = r.origin + localRay.dir * res.dst;

		// res.hitPoint.x = 10;
		// res.hitPoint.y = 12;
		// res.hitPoint.z = 4;
		// res.hitPoint.w = 0;

		res.normal = normal;

		// res.normal = normalize(res.hitPoint);
		// res.normal.x = 1;
		// res.normal.y = 1;
		// res.normal.z = 1;
		return res;
	}
};

#endif