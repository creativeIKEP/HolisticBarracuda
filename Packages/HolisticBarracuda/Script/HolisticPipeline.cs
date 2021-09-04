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
    public ComputeBuffer faceVertexBuffer;
    public ComputeBuffer leftEyeVertexBuffer;
    public ComputeBuffer rightEyeVertexBuffer;
    public ComputeBuffer leftHandVertexBuffer;
    public ComputeBuffer rightHandVertexBuffer;

    const int handCropImageSize = HandLandmarkDetector.ImageSize;
    const int handVertexCount = HandLandmarkDetector.VertexCount;
    const int letterboxWidth = 128;

    BlazePoseDetecter blazePoseDetecter;
    FacePipeline facePipeline;
    HandLandmarkDetector handLandmarkDetector;
    ComputeShader cs;
    RenderTexture letterBoxTexture;
    ComputeBuffer leftHandRegion;
    RenderTexture leftHandCropTexture;
    ComputeBuffer rightHandRegion;
    RenderTexture rightHandCropTexture;


    public HolisticPipeline(HolisticResource resource, BlazePoseModel blazePoseModel = BlazePoseModel.full){
        cs = resource.cs;
        blazePoseDetecter = new BlazePoseDetecter(resource.blazePoseResource, blazePoseModel);

        facePipeline = new FacePipeline(resource.faceMeshResource);
        faceVertexBuffer = new ComputeBuffer(facePipeline.RefinedFaceVertexBuffer.count, sizeof(float) * 4);
        leftEyeVertexBuffer = new ComputeBuffer(facePipeline.RawLeftEyeVertexBuffer.count, sizeof(float) * 4);
        rightEyeVertexBuffer = new ComputeBuffer(facePipeline.RawRightEyeVertexBuffer.count, sizeof(float) * 4);

        letterBoxTexture = new RenderTexture(letterboxWidth, letterboxWidth, 0, RenderTextureFormat.ARGB32);
        letterBoxTexture.enableRandomWrite = true;
        letterBoxTexture.Create();

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
        // Letterboxing scale factor
        var scale = new Vector2(
            Mathf.Max((float)inputTexture.height / inputTexture.width, 1),
            Mathf.Max(1, (float)inputTexture.width / inputTexture.height)
        );
        
        // Image scaling and padding
        // Output image is letter-box image.
        // For example, top and bottom pixels of `letterboxTexture` are black if `inputTexture` size is 1920(width)*1080(height)
        cs.SetInt("_letterboxWidth", letterboxWidth);
        cs.SetVector("_spadScale", scale);
        cs.SetTexture(0, "_letterboxInput", inputTexture);
        cs.SetTexture(0, "_letterboxTexture", letterBoxTexture);
        cs.Dispatch(0, letterboxWidth / 8, letterboxWidth / 8, 1);

        facePipeline.ProcessImage(letterBoxTexture);

        cs.SetBuffer(1, "_faceVertices", facePipeline.RefinedFaceVertexBuffer);
        cs.SetBuffer(1, "_faceReconVertices", faceVertexBuffer);
        cs.Dispatch(1, facePipeline.RefinedFaceVertexBuffer.count, 1, 1);
        
        // Reconstruct left eye rotation
        cs.SetMatrix("_irisCropMatrix", facePipeline.LeftEyeCropMatrix);
        cs.SetBuffer(2, "_irisVertices", facePipeline.RawLeftEyeVertexBuffer);
        cs.SetBuffer(2, "_irisReconVertices", leftEyeVertexBuffer);
        cs.Dispatch(2, facePipeline.RawLeftEyeVertexBuffer.count, 1, 1);

        // Reconstruct right eye rotation
        cs.SetMatrix("_irisCropMatrix", facePipeline.RightEyeCropMatrix);
        cs.SetBuffer(2, "_irisVertices", facePipeline.RawRightEyeVertexBuffer);
        cs.SetBuffer(2, "_irisReconVertices", rightEyeVertexBuffer);
        cs.Dispatch(2, facePipeline.RawRightEyeVertexBuffer.count, 1, 1);
    }

    void HandProcess(Texture inputTexture, bool isRight){
        // Calculate hand region with pose landmark
        cs.SetInt("_isRight", isRight?1:0);
        cs.SetFloat("_bboxDt", Time.deltaTime);
        cs.SetBuffer(3, "_poseInput", blazePoseDetecter.outputBuffer);
        cs.SetBuffer(3, "_bboxRegion", isRight ? rightHandRegion : leftHandRegion);
        cs.Dispatch(3, 1, 1, 1);

        // Hand region cropping
        cs.SetInt("_handCropImageSize", handCropImageSize);
        cs.SetTexture(4, "_handCropInput", inputTexture);
        cs.SetBuffer(4, "_handCropRegion", isRight ? rightHandRegion : leftHandRegion);
        cs.SetTexture(4, "_handCropOutput", isRight ? rightHandCropTexture : leftHandCropTexture);
        cs.Dispatch(4, handCropImageSize / 8, handCropImageSize / 8, 1);

        // Hand landmark detection
        handLandmarkDetector.ProcessImage(isRight ? rightHandCropTexture : leftHandCropTexture);

        // Key point postprocess
        cs.SetFloat("_handPostDt", Time.deltaTime);
        cs.SetBuffer(5, "_handPostInput", handLandmarkDetector.OutputBuffer);
        cs.SetBuffer(5, "_handPostRegion", isRight ? rightHandRegion : leftHandRegion);
        cs.SetBuffer(5, "_handPostOutput", isRight ? rightHandVertexBuffer : leftHandVertexBuffer);
        cs.Dispatch(5, 1, 1, 1);
    }

    public void Dispose(){
        blazePoseDetecter.Dispose();

        facePipeline.Dispose();
        faceVertexBuffer.Dispose();
        leftEyeVertexBuffer.Dispose();
        rightEyeVertexBuffer.Dispose();
        letterBoxTexture.Release();

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