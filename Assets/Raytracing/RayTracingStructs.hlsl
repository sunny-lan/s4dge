#ifndef _RAY_TRACING_STRUCTS
#define _RAY_TRACING_STRUCTS
#include "Transform4D.hlsl"


// --- Structures ---
struct Ray
{
	float4 origin;
	float4 dir;

	float3 origin3D()
	{
		return origin.xyz;
	}

	float3 dir3D()
	{
		return dir.xyz;
	}
};

struct RayTracingMaterial
{
	float4 colour;
	float4 emissionColour;
	float4 specularColour;
	float emissionStrength;
	float smoothness;
	float specularProbability;
	int flag;
};

struct MeshInfo
{
	uint firstTriangleIndex;
	uint numTriangles;
	RayTracingMaterial material;
	float3 boundsMin;
	float3 boundsMax;
};

struct HitInfo
{
	bool didHit;
	float dst;
	float4 hitPoint;
	float4 normal;
	float numHits;
	RayTracingMaterial material;
};


// Apply transform to ray - used for inverse transform of shapes
inline Ray TransformRay(Ray ray, Transform4D transform)
{
    Ray localRay;
    localRay.origin = transform.apply(ray.origin);
    localRay.dir = mul(transform.scaleAndRot, ray.dir); // TODO SUS
    return localRay;
}


void _compareHitInfo(inout HitInfo closestHit, in HitInfo hitInfo)
{
    if (hitInfo.didHit && abs(hitInfo.dst - closestHit.dst) > 0.01)
    {
						
        if (hitInfo.dst < closestHit.dst)
        {
            hitInfo.numHits += closestHit.numHits;
            closestHit = hitInfo;
        }
        else
        {
            closestHit.numHits += hitInfo.numHits;
        }
    }
}
#endif