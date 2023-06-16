#ifndef HYPERPLANE
#define HYPERPLANE

// A hyperplane is the 4D extension of a 3D plane
// Like a 3D plane, it also can be defined using a normal and a point along that normal
// 
// In 4D the volume of a hyperplane is R^3
// A hyperplane divides the 4D space into two halves, just like a plane does in 3D
//
// This is very useful because we can define most 4D shapes with flat faces
// as the set of points that is bounded by some hyperplanes,
// just like how in 3D, a cube is bounded by a number of planes

struct Hyperplane
{
	float4 normal;
	float offset;

	bool _tmp_cmp(float a, float b, int sign)
	{
		if (sign == 0)return a == b;
		return a * sign > b * sign;
	}

	// Determines the part of a ray that is within a given side of a hyperplane
	// Depending on sign, this determines which side we are testing

	// If sign < 0, finds t that normal . x <= offset
	// If sign = 0, finds t that normal . x = offset
	// If sign > 0, finds t that normal . x >= offset

	// normal . (s + dt) <= offset
	// t * normal . d <= offset - normal . s
	// t <= (offset - normal . s) / (normal . d)	(flip sign if normal . d < 0)

	void intersection(Ray r, int sign, out float min_t, out float max_t)
	{
		float o_n_s = offset - dot(normal, r.origin);
		float n_d = dot(normal, r.dir);


		if (n_d == 0)
		{
			// if n_d == 0, LHS = 0 <= o_n_s
			if (_tmp_cmp(0, o_n_s, sign)) {
				// All t valid
				min_t = 0;
				max_t = 1.#INF;
			}
			else
			{
				// No solution
				min_t = 1;
				max_t = 0;
			}
		}
		else
		{
			float boundary = o_n_s / n_d;
			if (n_d < 0) sign = -sign;
			min_t = 0;
			max_t = 1.#INF;
			if (sign <= 0) max_t = boundary;
			if (sign >= 0) min_t = boundary;
		}
	}
};
#endif