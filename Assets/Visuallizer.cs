using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using MediaPipe.Holistic;

public class Visuallizer : MonoBehaviour
{
    [SerializeField] WebCamInput webCamInput;
    [SerializeField] RawImage image;
    [SerializeField] Shader faceMeshShader;
    [SerializeField] HolisticResource resource;

    HolisticPipeline holisticPipeline;
    Material faceMeshMaterial;

    void Start()
    {
        holisticPipeline = new HolisticPipeline(resource);
        faceMeshMaterial = new Material(faceMeshShader);
    }

    void LateUpdate()
    {
        image.texture = webCamInput.inputImageTexture;
        holisticPipeline.ProcessImage(webCamInput.inputImageTexture);
    }

    void OnRenderObject(){
        // FaceMesh
        var mv = float4x4.Translate(math.float3(-0.5f, -0.5f, 0));
        faceMeshMaterial.SetBuffer("_Vertices", holisticPipeline.faceVertexBuffer);
        faceMeshMaterial.SetPass(1);
        Graphics.DrawMeshNow(resource.faceMeshResource.faceLineTemplate, mv);

        // Left eye
        faceMeshMaterial.SetMatrix("_XForm", mv);
        faceMeshMaterial.SetBuffer("_Vertices", holisticPipeline.leftEyeVertexBuffer);
        faceMeshMaterial.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);

        // Right eye
        faceMeshMaterial.SetMatrix("_XForm", mv);
        faceMeshMaterial.SetBuffer("_Vertices", holisticPipeline.rightEyeVertexBuffer);
        faceMeshMaterial.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);
    }

    void OnDestroy(){
        holisticPipeline.Dispose();
    }
}
