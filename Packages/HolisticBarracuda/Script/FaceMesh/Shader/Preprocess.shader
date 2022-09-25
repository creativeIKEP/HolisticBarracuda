Shader "Hidden/MediaPipe/FaceMesh/Preprocess"
{
    Properties
    {
        _MainTex("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4x4 _Xform;

    float4 Fragment(float4 vertex : SV_Position,
                    float2 uv : TEXCOORD0) : SV_Target
    {
        uv = mul(_Xform, float4(uv, 0, 1)).xy;
        float4 color = tex2D(_MainTex, uv);

        // The Shader will display the appropriate colors even in the liner color space, 
        // so the color representation will be wrong, but we will convert it for better estimation accuracy.
        #if !UNITY_COLORSPACE_GAMMA
        color.rgb = LinearToGammaSpace(color.rgb);
        #endif
        
        return color;
    }

    ENDCG

    SubShader
    {
        Cull Off ZTest Always ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Fragment
            ENDCG
        }
    }
}
