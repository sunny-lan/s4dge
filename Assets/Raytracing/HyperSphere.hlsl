#ifndef _HYPERSPHERE
#define _HYPERSPHERE
#include "RayTracingStructs.hlsl"

struct HyperSphere
{
    Transform4D inverseTransform;
    float radius;
    RayTracingMaterial material;
};



// Calculate intersection of a ray with a hypersphere
// Math from: http://reprints.gravitywaves.com/People/Hollasch/Four-Space%20Visualization%20of%204D%20Objects%20-%20Chapter%205.htm 
HitInfo RayHyperSphere(Ray ray, HyperSphere hyperSphere)
{
    Ray localRay = TransformRay(ray, hyperSphere.inverseTransform);
    HitInfo hitInfo = (HitInfo)0;

    float4 V = localRay.origin * -1;
    float bb = dot(V, localRay.dir);

    float rad = (bb*bb) - dot(V, V) + hyperSphere.radius * hyperSphere.radius;

    if (rad < 0) { // If rad negative then no intersection
        return hitInfo;				
    } 

    rad = sqrt(rad);

    float t2 = bb - rad;
    float t1 = bb + rad;

    // Get smaller of t1 and t2
    if (t1 < 0 || (t2 > 0 && t2 < t1)) {
        t1 = t2;
    }

    // If behind sphere return false
    if (t1 < 0) {
        return hitInfo;
    }


    float4 intersection = localRay.origin + (t1 * localRay.dir);
    float4 normal = intersection / hyperSphere.radius;

    hitInfo.didHit = true;
    hitInfo.dst = t1;
    hitInfo.hitPoint = ray.origin + (t1 * localRay.dir); //! Very important
    hitInfo.numHits = t2 > 0 ? 2 : 0; // I think this works if I understand the math correctly
    hitInfo.normal = normal;
    hitInfo.material = hyperSphere.material;

    return hitInfo;
}
#endif