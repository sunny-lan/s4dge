#ifndef TRANSFORM4D
#define TRANSFORM4D

struct Transform4D {
	float4x4 scaleAndRot;
	float4 position;

	float4 apply(float4 v) {
		return mul(scaleAndRot, v) + position;
	}
};

#endif