#ifndef VERTEX_SHADER_H
#define VERTEX_SHADER_H

// UNIFORM VARIABLES

float4x4 modelWorldScaleAndRot4D;
float4 modelWorldTranslation4D;

float4x4 modelViewScaleAndRot4D;
float4x4 modelViewScaleAndRotInv4D;
float4 modelViewTranslation4D;

float4x4 modelViewProjection3D;
float zSlice; // slicing plane for 3D projected point at z = zSlice

float vanishingW; // camera clip plane - vanishing point at (0, 0, 0, vanishingW)
float nearW; // camera viewport plane at w = nearW

// STRUCT DEFINITIONS

struct VertexData {
	float4 pos: POSITION;
	float4 normal: NORMAL;
	float4 worldPos: POSITION1;
};

struct Transform4D {
	float4x4 scaleAndRot;
	float4 translation;
};

// FUNCTIONS

float4 applyScaleAndRot(float4 v, float4x4 scaleAndRot) {
	return mul(scaleAndRot, v);
}

float4 applyTranslation(float4 v, float4 translation) {
	return v + translation;
}

float4 applyTransform(float4 v, Transform4D transform) {
    return applyTranslation(applyScaleAndRot(v, transform.scaleAndRot), transform.translation);
}

float4 applyPerspectiveTransformation(float4 pos) {
	// Project 4D point to 3D
	float4 pProjectedNoPerspective = mul(modelViewProjection3D, float4(pos.xyz, 1));
	float perspectiveFactor = (vanishingW - pos.w) / (vanishingW - nearW);
	float3 pProjected3D = mul(pProjectedNoPerspective, 1.0 / pProjectedNoPerspective.w).xyz; // apply perspective division
	float3 pProjectedWithPerspective = mul(perspectiveFactor, pProjected3D.xyz);

	// Piggyback w coordinate of 4D point for depth testing
	return float4(pProjectedWithPerspective, pos.w);
}

#endif // VERTEX_SHADER_H