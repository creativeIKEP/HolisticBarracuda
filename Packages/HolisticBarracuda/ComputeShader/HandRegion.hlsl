// Same with https://github.com/keijiro/HandPoseBarracuda/blob/main/Assets/HandPose/Shader/HandRegion.hlsl

#ifndef _HOLISTICBARRACUDA_HANDREGION_HLSL_
#define _HOLISTICBARRACUDA_HANDREGION_HLSL_

struct HandRegion
{
    float4 box; // center_x, center_y, size, angle
    float4 dBox;
    float4x4 cropMatrix;
};

#endif
