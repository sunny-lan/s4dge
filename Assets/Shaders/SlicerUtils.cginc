#ifndef SLICER_H
#define SLICER_H

// VARIABLES

groupshared uint curLocalAppendIdx;
groupshared uint globalAppendIdx;
RWStructuredBuffer<uint> curGlobalAppendIdx;

// STRUCT DEFINITIONS

struct Tet4D {
	int vertexIndices[4];
};

struct Triangle {
	float4 vertexIndices[3];
};

// FUNCTIONS

/*
* The following functions help deal with dynamically-sized output
*/

// The first thread in a group initializes the append index
void InitLocalAppendIdx(uint3 threadId) {
    if (threadId.x == 0) {
        curLocalAppendIdx = 0;
    }
    GroupMemoryBarrierWithGroupSync(); // wait for all threads to reach this stage
}

// Fetches the local index for the current thread to append to and update the local index for the next requesting thread
uint IncreaseLocalAppendIdx(uint amount) {
    uint localAppendIdx;
    InterlockedAdd(curLocalAppendIdx, amount, localAppendIdx);
    GroupMemoryBarrierWithGroupSync();

    return localAppendIdx;
}

// For the first thread in the group
// Fetches the global index for the current thread group to append to and update the index the next requesting group
uint IncreaseGlobalAppendIdx(uint3 threadId) {
    if (threadId.x == 0) {
        InterlockedAdd(curGlobalAppendIdx[0], curLocalAppendIdx, globalAppendIdx);
    }
    GroupMemoryBarrierWithGroupSync();

    return globalAppendIdx;
}

#endif // SLICER_H