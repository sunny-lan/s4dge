#ifndef _TETMESH
#define _TETMESH


#include "Transform4D.hlsl"
#include "RayTracingStructs.hlsl"
struct TetMesh
{
	Transform4D inverseTransform;
	int stIdx;
	int edIdx;
	RayTracingMaterial material;
};

#endif