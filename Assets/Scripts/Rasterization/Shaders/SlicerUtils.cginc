#ifndef SLICER_H
#define SLICER_H

#include "VertexShaderUtils.cginc"

// VARIABLES

// STRUCT DEFINITIONS

struct Tet4D {
	int vertexIndices[4];
};

struct Triangle {
	int vertexIndices[3];
};

static const uint2 intersectingEdges[] = {
    // table entry 0 is empty
    uint2(0, 3), uint2(1, 3), uint2(2, 3), // entry 1
    uint2(0, 2), uint2(1, 2), uint2(3, 2), // entry 2
    uint2(0, 2), uint2(0, 3), uint2(1, 3), uint2(1, 2),  // ...
    uint2(0, 1), uint2(2, 1), uint2(3, 1),
    uint2(0, 1), uint2(0, 3), uint2(2, 3), uint2(2, 1), 
    uint2(0, 1), uint2(0, 2), uint2(2, 3), uint2(1, 3),
    uint2(0, 1), uint2(0, 2), uint2(0, 3), // last entry 8
    uint2(0, 0) // padding element at the end to avoid out of bounds access
};

// elements are (starting index, # of entries)
static const uint2 edgeTableRows[] = {
    uint2(0, 0),
    uint2(0, 3),
    uint2(3, 3),
    uint2(6, 4),
    uint2(10, 3),
    uint2(13, 4),
    uint2(17, 4),
    uint2(21, 3)
};

// FUNCTIONS

// return 0 if val <= 0, else return 1
int SignOp(float val) {
    return val > zSlice;
}

float4 LerpVByPZ(float4 v0, float4 v1, float4 p0, float4 p1) {
    if (p1.z == zSlice && p0.z == zSlice) {
        return v1;
    } else {
        return lerp(v0, v1, abs(p0.z - zSlice) / (abs(p1.z - zSlice) + abs(p0.z - zSlice)));
    }
}

#endif // SLICER_H