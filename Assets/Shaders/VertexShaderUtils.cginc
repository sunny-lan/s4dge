#ifndef VERTEX_SHADER_H
#define VERTEX_SHADER_H

// UNIFORM VARIABLES

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
};

// FUNCTIONS

VertexData applyScaleAndRot(VertexData v, float4x4 scaleAndRot, float4x4 scaleAndRotInv) {
	VertexData transformed;
	transformed.pos = mul(scaleAndRot, v.pos);
	transformed.normal = mul(scaleAndRotInv, v.normal); // transpose of transform matrix is equal to the inverse
	return transformed;
}

VertexData applyTranslation(VertexData v, float4 translation) {
	VertexData transformed;
	transformed.pos = v.pos + translation;
	transformed.normal = v.normal;
	return transformed;
}

float4 applyPerspectiveTransformation(float4 pos) {
	// Apply 4D perspective transformation
	float4 pTransformed4D = mul((vanishingW - nearW) / (vanishingW - pos.w), pos);

	// Project 4D point to 3D
	float4 pProjectedNoPerspective = mul(modelViewProjection3D, float4(pos.xyz, 1));
	float3 pProjected3D = mul(pProjectedNoPerspective, 1.0 / pProjectedNoPerspective.w).xyz; // apply perspective division

	// Piggyback w coordinate of 4D point for depth testing
	return float4(pProjected3D, pTransformed4D.w);
}

#endif // VERTEX_SHADER_H