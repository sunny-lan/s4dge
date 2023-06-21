#ifndef VARIABLE_LENGTH_BUFFER_H
#define VARIABLE_LENGTH_BUFFER_H

// VARIABLES

groupshared uint curLocalAppendIdx[VL_BUFFER_COUNT];
groupshared uint globalAppendIdx[VL_BUFFER_COUNT];
RWStructuredBuffer<uint> curGlobalAppendIdx;

// STRUCT DEFINITIONS

// FUNCTIONS

/*
* The following functions help deal with dynamically-sized output
*/

// The first thread in a group initializes the append index
void InitLocalAppendIdx(uint3 threadId, uint bufferId) {
    if (threadId.x == 0) {
        curLocalAppendIdx[bufferId] = 0;
    }
}

// Fetches the local index for the current thread to append to and update the local index for the next requesting thread
uint IncreaseLocalAppendIdx(uint amount, uint bufferId) {
    uint localAppendIdx;
    InterlockedAdd(curLocalAppendIdx[bufferId], amount, localAppendIdx);

    return localAppendIdx;
}

// For the first thread in the group
// Fetches the global index for the current thread group to append to and update the index the next requesting group
uint IncreaseGlobalAppendIdx(uint3 threadId, uint bufferId) {
    if (threadId.x == 0) {
        InterlockedAdd(curGlobalAppendIdx[bufferId], curLocalAppendIdx[bufferId], globalAppendIdx[bufferId]);
    }

    return globalAppendIdx[bufferId];
}

#endif // VARIABLE_LENGTH_BUFFER_H