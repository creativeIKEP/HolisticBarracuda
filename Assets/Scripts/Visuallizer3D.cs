using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MediaPipe.Holistic;

public class Visuallizer3D : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] WebCamInput webCamInput;
    [SerializeField] RawImage inputImageUI;
    [SerializeField] HolisticResource resource;
    [SerializeField] Shader poseShader;
    [SerializeField, Range(0, 1)] float humanExistThreshold = 0.5f;

    HolisticPipeline holisticPipeline;
    Material poseMaterial;

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


    void Start(){
        holisticPipeline = new HolisticPipeline(resource);
        poseMaterial = new Material(poseShader);
    }

    void Update(){
        mainCamera.transform.RotateAround(Vector3.zero, Vector3.up, 0.5f);
    }

    void LateUpdate(){
        inputImageUI.texture = webCamInput.inputImageTexture;

        // Predict pose by neural network model.
        // Switchable anytime models with 2nd argment.
        holisticPipeline.ProcessImage(webCamInput.inputImageTexture);
    } 

    void OnRenderObject(){
        PoseRender();
    }

    void PoseRender(){
        // Set predicted pose world landmark results.
        poseMaterial.SetBuffer("_worldVertices", holisticPipeline.poseLandmarkWorldBuffer);
        // Set pose landmark counts.
        poseMaterial.SetInt("_keypointCount", holisticPipeline.poseVertexCount);
        poseMaterial.SetFloat("_humanExistThreshold", humanExistThreshold);
        poseMaterial.SetVectorArray("_linePair", linePair);
        poseMaterial.SetMatrix("_invViewMatrix", mainCamera.worldToCameraMatrix.inverse);

        // Draw 35 world body topology lines.
        poseMaterial.SetPass(2);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, BODY_LINE_NUM);

        // Draw 33 world landmark points.
        poseMaterial.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, holisticPipeline.poseVertexCount);
    }

    void OnApplicationQuit(){
        // Must call Dispose method when no longer in use.
        holisticPipeline.Dispose();
    }
}
