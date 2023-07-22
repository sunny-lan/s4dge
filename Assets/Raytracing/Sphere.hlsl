#ifndef _SPHERE
#define _SPHERE
#include "RayTracingStructs.hlsl"

struct Sphere
{
    Transform4D inverseTransform;
    float radius;
    RayTracingMaterial material;
};

// Calculate the intersection of a ray with a sphere
inline HitInfo RaySphere(Ray ray, Sphere sphere)
{
    Ray localRay = TransformRay(ray, sphere.inverseTransform);
    HitInfo hitInfo = (HitInfo) 0;
    float3 offsetRayOrigin = localRay.origin3D();
				// From the equation: sqrLength(rayOrigin + rayDir * dst) = radius^2
				// Solving for dst results in a quadratic equation with coefficients:
    float a = dot(localRay.dir3D(), localRay.dir3D()); // a = 1 (assuming unit vector)
    float b = 2 * dot(offsetRayOrigin, localRay.dir3D());
    float c = dot(offsetRayOrigin, offsetRayOrigin) - sphere.radius * sphere.radius;
				// Quadratic discriminant
    float discriminant = b * b - 4 * a * c;

				// No solution when d < 0 (ray misses sphere)
    if (discriminant >= 0)
    {
					// Distance to nearest intersection point (from quadratic formula)
        float dst = (-b - sqrt(discriminant)) / (2 * a);

					// Ignore intersections that occur behind the ray
        if (dst >= 0)
        {
            hitInfo.didHit = true;
            hitInfo.dst = dst;
            hitInfo.hitPoint = localRay.origin + localRay.dir * dst;
            hitInfo.numHits = discriminant > 10 ? 2 : 1;
            hitInfo.normal = normalize(hitInfo.hitPoint);
        }
    }
    return hitInfo;
}
#endif