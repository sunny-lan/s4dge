#ifndef VARIABLE_LENGTH_BUFFER_H
#define VARIABLE_LENGTH_BUFFER_H

// VARIABLES

/*
* VL_BUFFER_COUNT must be defined by the user as a macro, this is the number of VL buffers the shader will use
*/

RWStructuredBuffer<uint> curGlobalAppendIdx;

// STRUCT DEFINITIONS

struct VLComputeBuffer {

    // MEMBERS
    uint mBufferId;
    uint mCurLocalAppendIdx;
    uint mGlobalAppendIdx;

    // FUNCTIONS

    // The first thread in a group initializes the append index
    void Init(uint3 globalId, uint3 threadId, uint bufferId) {
        [branch]
        if (globalId.x == 0) {
            curGlobalAppendIdx[bufferId] = 0;
        }

        [branch]
        if (threadId.x == 0) {
            mCurLocalAppendIdx = 0;
            mBufferId = bufferId;
            mGlobalAppendIdx = 0;
        }
    }

    // Fetches the local index for the current thread to append to and update the local index for the next requesting thread
    uint IncreaseLocalAppendIdx(uint amount) {
        uint localAppendIdx;
        InterlockedAdd(mCurLocalAppendIdx, amount, localAppendIdx);

        return localAppendIdx;
    }

    // For the first thread in the group
    // Fetches the global index for the current thread group to append to and update the index the next requesting group
    void IncreaseGlobalAppendIdx(uint3 threadId) {
        if (threadId.x == 0) {
            InterlockedAdd(curGlobalAppendIdx[mBufferId], mCurLocalAppendIdx, mGlobalAppendIdx);
        }
    }

    uint GetGroupAppendIdx() {
        return mGlobalAppendIdx;
    }
};



#endif // VARIABLE_LENGTH_BUFFER_H
