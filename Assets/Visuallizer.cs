using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using MediaPipe.Holistic;

public class Visuallizer : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] WebCamInput webCamInput;
    [SerializeField] RawImage image;
    [SerializeField] Shader poseShader;
    [SerializeField, Range(0, 1)] float humanExistThreshold = 0.5f;
    [SerializeField] Shader faceMeshShader;
    [SerializeField] Shader handShader;
    [SerializeField] HolisticResource resource;

    HolisticPipeline holisticPipeline;
    Material poseMaterial;
    Material faceMeshMaterial;
    Material handMaterial;

    // Lines count of body's topology.
    const int BODY_LINE_NUM = 35;
    // Pairs of vertex indices of the lines that make up body's topology.
    // Defined by the figure in https://google.github.io/mediapipe/solutions/pose.
    readonly List<Vector4> linePair = new List<Vector4>{
        new Vector4(0, 1), new Vector4(1, 2), new Vector4(2, 3), new Vector4(3, 7), new Vector4(0, 4), 
        new Vector4(4, 5), new Vector4(5, 6), new Vector4(6, 8), new Vector4(9, 10), new Vector4(11, 12), 
        new Vector4(11, 13), new Vector4(13, 15), new Vector4(15, 17), new Vector4(17, 19), new Vector4(19, 15), 
        new Vector4(15, 21), new Vector4(12, 14), new Vector4(14, 16), new Vector4(16, 18), new Vector4(18, 20), 
        new Vector4(20, 16), new Vector4(16, 22), new Vector4(11, 23), new Vector4(12, 24), new Vector4(23, 24), 
        new Vector4(23, 25), new Vector4(25, 27), new Vector4(27, 29), new Vector4(29, 31), new Vector4(31, 27), 
        new Vector4(24, 26), new Vector4(26, 28), new Vector4(28, 30), new Vector4(30, 32), new Vector4(32, 28)
    };

    void Start()
    {
        holisticPipeline = new HolisticPipeline(resource);
        poseMaterial = new Material(poseShader);
        faceMeshMaterial = new Material(faceMeshShader);
        handMaterial = new Material(handShader);
        handMaterial.SetBuffer("_leftKeyPoints",holisticPipeline.leftHandVertexBuffer);
        handMaterial.SetBuffer("_rightKeyPoints", holisticPipeline.rightHandVertexBuffer);
    }

    void LateUpdate()
    {
        image.texture = webCamInput.inputImageTexture;
        holisticPipeline.ProcessImage(webCamInput.inputImageTexture);
    }

    void OnRenderObject(){
        PoseRender();
        FaceRender();
        HandRender(false);
        HandRender(true);
    }

    void PoseRender(){
        var w = image.rectTransform.rect.width;
        var h = image.rectTransform.rect.height;

        // Set predicted pose landmark results.
        poseMaterial.SetBuffer("_vertices", holisticPipeline.poseLandmarkBuffer);
        // Set pose landmark counts.
        poseMaterial.SetInt("_keypointCount", holisticPipeline.poseVertexCount);
        poseMaterial.SetFloat("_humanExistThreshold", humanExistThreshold);
        poseMaterial.SetVector("_uiScale", new Vector2(w, h));
        poseMaterial.SetVectorArray("_linePair", linePair);

        // Draw 35 body topology lines.
        poseMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, BODY_LINE_NUM);

        // Draw 33 landmark points.
        poseMaterial.SetPass(1);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, holisticPipeline.poseVertexCount);
    }

    void FaceRender(){
        var w = image.rectTransform.rect.width;
        var h = image.rectTransform.rect.height;
        faceMeshMaterial.SetVector("_uiScale", new Vector2(w, h));

        // FaceMesh
        faceMeshMaterial.SetBuffer("_Vertices", holisticPipeline.faceVertexBuffer);
        faceMeshMaterial.SetPass(0);
        Graphics.DrawMeshNow(resource.faceMeshResource.faceLineTemplate, Vector3.zero, Quaternion.identity);

        // Left eye
        faceMeshMaterial.SetBuffer("_Vertices", holisticPipeline.leftEyeVertexBuffer);
        faceMeshMaterial.SetPass(1);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);

        // Right eye
        faceMeshMaterial.SetBuffer("_Vertices", holisticPipeline.rightEyeVertexBuffer);
        faceMeshMaterial.SetPass(1);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);
    }

    void HandRender(bool isRight){
        var w = image.rectTransform.rect.width;
        var h = image.rectTransform.rect.height;
        handMaterial.SetInt("_isRight", isRight?1:0);
        handMaterial.SetVector("_uiScale", new Vector2(w, h));

        // Key point circles
        handMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 96, 21);

        // Skeleton lines
        handMaterial.SetPass(1);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 2, 4 * 5 + 1);
    }

    void OnDestroy(){
        holisticPipeline.Dispose();
        Destroy(poseMaterial);
        Destroy(faceMeshMaterial);
        Destroy(handMaterial);
    }
}
