using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;
using MediaPipe.Holistic;

public class Visuallizer : MonoBehaviour
{
    [SerializeField] WebCamInput webCamInput;
    [SerializeField] RawImage image;
    [SerializeField] Shader _shader;
    [SerializeField] MediaPipe.FaceMesh.ResourceSet faceMeshResource;

    HolisticPipeline holisticPipeline;
    Material _material;

    void Start()
    {
        holisticPipeline = new HolisticPipeline(faceMeshResource);
        _material = new Material(_shader);
    }

    void LateUpdate()
    {
        image.texture = webCamInput.inputImageTexture;
        holisticPipeline.ProcessImage(webCamInput.inputImageTexture);
    }

    void OnRenderObject(){
        // Main view overlay
        var mv = float4x4.Translate(math.float3(-0.5f, -0.5f, 0));
        _material.SetBuffer("_Vertices", holisticPipeline.faceVertexBuffer);
        _material.SetPass(1);
        Graphics.DrawMeshNow(faceMeshResource.faceLineTemplate, mv);

        // Left eye
        var fLE = math.mul(mv, holisticPipeline.leftEyeCropMatrix);
        _material.SetMatrix("_XForm", fLE);
        _material.SetBuffer("_Vertices", holisticPipeline.leftEyeVertexBuffer);
        _material.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);

        // // Right eye
        var fRE = math.mul(mv, holisticPipeline.rightEyeCropMatrix);
        _material.SetMatrix("_XForm", fRE);
        _material.SetBuffer("_Vertices", holisticPipeline.rightEyeVertexBuffer);
        _material.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);
    }

    void OnDestroy(){
        holisticPipeline.Dispose();
    }
}
