Shader "Hidden/HolisticBarracuda/HandVisuallizer"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    StructuredBuffer<float4> _leftKeyPoints;
    StructuredBuffer<float4> _rightKeyPoints;
    int _isRight;
    float2 _uiScale;

    // Coloring function
    float3 DepthToColor(float z)
    {
        float3 c = lerp(1, float3(0, 0, 1), saturate(z * 2));
        return lerp(c, float3(1, 0, 0), saturate(z * -2));
    }

    void VertexKeys(uint vid : SV_VertexID,
                    uint iid : SV_InstanceID,
                    out float4 position : SV_Position,
                    out float4 color : COLOR)
    {
        float3 p = _isRight ? _rightKeyPoints[iid].xyz : _leftKeyPoints[iid].xyz;

        uint fan = vid / 3;
        uint segment = vid % 3;

        float theta = (fan + segment - 1) * UNITY_PI / 16;
        float radius = (segment > 0) * 0.08 * (max(0, -p.z) + 0.1);
        p.xy += float2(cos(theta), sin(theta)) * radius;
        p.xy = (2 * p.xy - 1) * _uiScale / _ScreenParams.xy;

        position = float4(p.xy, 0, 1);
        color = float4(DepthToColor(p.z), 0.8);
    }

    void VertexBones(uint vid : SV_VertexID,
                     uint iid : SV_InstanceID,
                     out float4 position : SV_Position,
                     out float4 color : COLOR)
    {
        uint finger = iid / 4;
        uint segment = iid % 4;

        uint i = min(4, finger) * 4 + segment + vid;
        uint root = finger > 1 && finger < 5 ? i - 3 : 0;

        i = max(segment, vid) == 0 ? root : i;

        float3 p = _isRight ? _rightKeyPoints[i].xyz : _leftKeyPoints[i].xyz;
        p.xy = (2 * p.xy - 1) * _uiScale / _ScreenParams.xy;

        position = float4(p.xy, 0, 1);
        color = float4(DepthToColor(p.z), 0.8);
    }

    float4 Fragment(float4 position : SV_Position,
                    float4 color : COLOR) : SV_Target
    {
        return color;
    }

    ENDCG

    SubShader
    {
        ZWrite Off ZTest Always Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex VertexKeys
            #pragma fragment Fragment
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex VertexBones
            #pragma fragment Fragment
            ENDCG
        }
    }
}