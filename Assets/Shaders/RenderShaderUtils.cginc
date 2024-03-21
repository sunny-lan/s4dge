#ifndef RENDER_SHADER_UTILS_H
#define RENDER_SHADER_UTILS_H

float4 applyClipSpaceTransform(float4 v) {
    return float4(v.x / 1.7, -v.y, -v.w / 100 + 0.5, 1.0);
}

#endif // RENDER_SHADER_UTILS_H