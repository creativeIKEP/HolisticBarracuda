using UnityEngine;
using Mediapipe.BlazePose;
using MediaPipe.FaceMesh;
using MediaPipe.HandLandmark;


namespace MediaPipe.Holistic {

public class HolisticPipeline : System.IDisposable
{
    public int poseVertexCount => blazePoseDetecter.vertexCount;
    public ComputeBuffer poseLandmarkBuffer => blazePoseDetecter.outputBuffer;
    public ComputeBuffer poseLandmarkWorldBuffer => blazePoseDetecter.worldLandmarkBuffer;
    public ComputeBuffer faceVertexBuffer => facePipeline.RefinedFaceVertexBuffer;
    public ComputeBuffer leftEyeVertexBuffer;
    public ComputeBuffer rightEyeVertexBuffer;
    public ComputeBuffer leftHandVertexBuffer;
    public ComputeBuffer rightHandVertexBuffer;

    const int handCropImageSize = HandLandmarkDetector.ImageSize;
    const int handVertexCount = HandLandmarkDetector.VertexCount;

    BlazePoseDetecter blazePoseDetecter;
    FacePipeline facePipeline;
    HandLandmarkDetector handLandmarkDetector;
    ComputeShader cs;
    ComputeBuffer leftHandRegion;
    RenderTexture leftHandCropTexture;
    ComputeBuffer rightHandRegion;
    RenderTexture rightHandCropTexture;


    public HolisticPipeline(HolisticResource resource, BlazePoseModel blazePoseModel = BlazePoseModel.full){
        cs = resource.cs;
        blazePoseDetecter = new BlazePoseDetecter(resource.blazePoseResource, blazePoseModel);

        facePipeline = new FacePipeline(resource.faceMeshResource);
        leftEyeVertexBuffer = new ComputeBuffer(facePipeline.RawLeftEyeVertexBuffer.count, sizeof(float) * 4);
        rightEyeVertexBuffer = new ComputeBuffer(facePipeline.RawRightEyeVertexBuffer.count, sizeof(float) * 4);

        handLandmarkDetector = new HandLandmarkDetector(resource.handResource);
        leftHandRegion = new ComputeBuffer(1, sizeof(float) * 24);
        leftHandCropTexture = new RenderTexture(handCropImageSize, handCropImageSize, 0, RenderTextureFormat.ARGB32);
        leftHandCropTexture.enableRandomWrite = true;
        leftHandCropTexture.Create();
        leftHandVertexBuffer = new ComputeBuffer(handVertexCount * 2, sizeof(float) * 4);
        rightHandRegion = new ComputeBuffer(1, sizeof(float) * 24);
        rightHandCropTexture = new RenderTexture(handCropImageSize, handCropImageSize, 0, RenderTextureFormat.ARGB32);
        rightHandCropTexture.enableRandomWrite = true;
        rightHandCropTexture.Create();
        rightHandVertexBuffer = new ComputeBuffer(handVertexCount * 2, sizeof(float) * 4);
    }

    public void ProcessImage(Texture inputTexture, BlazePoseModel blazePoseModel = BlazePoseModel.full){
        blazePoseDetecter.ProcessImage(inputTexture, blazePoseModel);
        FaceProcess(inputTexture);
        HandProcess(inputTexture, false);
        HandProcess(inputTexture, true);
    }

    void FaceProcess(Texture inputTexture){
        facePipeline.ProcessImage(inputTexture);
        
        // Reconstruct left eye rotation
        cs.SetMatrix("_irisCropMatrix", facePipeline.LeftEyeCropMatrix);
        cs.SetBuffer(0, "_irisVertices", facePipeline.RawLeftEyeVertexBuffer);
        cs.SetBuffer(0, "_irisReconVertices", leftEyeVertexBuffer);
        cs.Dispatch(0, facePipeline.RawLeftEyeVertexBuffer.count, 1, 1);

        // Reconstruct right eye rotation
        cs.SetMatrix("_irisCropMatrix", facePipeline.RightEyeCropMatrix);
        cs.SetBuffer(0, "_irisVertices", facePipeline.RawRightEyeVertexBuffer);
        cs.SetBuffer(0, "_irisReconVertices", rightEyeVertexBuffer);
        cs.Dispatch(0, facePipeline.RawRightEyeVertexBuffer.count, 1, 1);
    }

    void HandProcess(Texture inputTexture, bool isRight){
        // Letterboxing scale factor
        var scale = new Vector2
          (Mathf.Max((float)inputTexture.height / inputTexture.width, 1),
           Mathf.Max(1, (float)inputTexture.width / inputTexture.height));

        // Calculate hand region with pose landmark
        cs.SetInt("_isRight", isRight?1:0);
        cs.SetVector("_imageSize", new Vector2(inputTexture.width, inputTexture.height));
        cs.SetFloat("_bboxDt", Time.deltaTime);
        cs.SetBuffer(1, "_poseInput", blazePoseDetecter.outputBuffer);
        cs.SetBuffer(1, "_bboxRegion", isRight ? rightHandRegion : leftHandRegion);
        cs.Dispatch(1, 1, 1, 1);

        // Hand region cropping
        cs.SetVector("_spadScale", scale);
        cs.SetInt("_handCropImageSize", handCropImageSize);
        cs.SetTexture(2, "_handCropInput", inputTexture);
        cs.SetBuffer(2, "_handCropRegion", isRight ? rightHandRegion : leftHandRegion);
        cs.SetTexture(2, "_handCropOutput", isRight ? rightHandCropTexture : leftHandCropTexture);
        cs.Dispatch(2, handCropImageSize / 8, handCropImageSize / 8, 1);

        // Hand landmark detection
        handLandmarkDetector.ProcessImage(isRight ? rightHandCropTexture : leftHandCropTexture);

        // Key point postprocess
        cs.SetFloat("_handPostDt", Time.deltaTime);
        cs.SetFloat("_handPostScale", scale.y);
        cs.SetBuffer(3, "_handPostInput", handLandmarkDetector.OutputBuffer);
        cs.SetBuffer(3, "_handPostRegion", isRight ? rightHandRegion : leftHandRegion);
        cs.SetBuffer(3, "_handPostOutput", isRight ? rightHandVertexBuffer : leftHandVertexBuffer);
        cs.Dispatch(3, 1, 1, 1);
    }

    public void Dispose(){
        blazePoseDetecter.Dispose();

        facePipeline.Dispose();
        leftEyeVertexBuffer.Dispose();
        rightEyeVertexBuffer.Dispose();

        handLandmarkDetector.Dispose();
        leftHandRegion.Dispose();
        leftHandCropTexture.Release();
        leftHandVertexBuffer.Dispose();
        rightHandRegion.Dispose();
        rightHandCropTexture.Release();
        rightHandVertexBuffer.Dispose();
    }
}

}