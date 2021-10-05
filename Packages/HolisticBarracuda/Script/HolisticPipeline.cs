using UnityEngine;
using Mediapipe.BlazePose;
using MediaPipe.FaceMesh;
using MediaPipe.FaceLandmark;
using MediaPipe.Iris;
using MediaPipe.BlazePalm;
using MediaPipe.HandLandmark;


namespace MediaPipe.Holistic {

public class HolisticPipeline : System.IDisposable
{
    public int poseVertexCount => blazePoseDetecter.vertexCount;
    public ComputeBuffer poseLandmarkBuffer => blazePoseDetecter.outputBuffer;
    public ComputeBuffer poseLandmarkWorldBuffer => blazePoseDetecter.worldLandmarkBuffer;
    
    public int faceVertexCount => FaceLandmarkDetector.VertexCount;
    public ComputeBuffer faceVertexBuffer;

    public int eyeVertexCount => EyeLandmarkDetector.VertexCount;
    public ComputeBuffer leftEyeVertexBuffer;
    public ComputeBuffer rightEyeVertexBuffer;

    public int handVertexCount => HandLandmarkDetector.VertexCount;
    public ComputeBuffer leftHandVertexBuffer;
    public ComputeBuffer rightHandVertexBuffer;


    const int letterboxWidth = 128;
    const int handCropImageSize = HandLandmarkDetector.ImageSize;

    ComputeShader commonCs;
    ComputeShader faceCs;
    ComputeShader handCs;
    BlazePoseDetecter blazePoseDetecter;
    FacePipeline facePipeline;
    PalmDetector palmDetector;
    HandLandmarkDetector handLandmarkDetector;
    RenderTexture letterBoxTexture;
    ComputeBuffer handsRegionFromPalm;
    ComputeBuffer leftHandRegionFromPose;
    ComputeBuffer rightHandRegionFromPose;
    ComputeBuffer handCropBuffer;
    ComputeBuffer deltaLeftHandVertexBuffer;
    ComputeBuffer deltaRightHandVertexBuffer;


    public HolisticPipeline(HolisticResource resource, BlazePoseModel blazePoseModel = BlazePoseModel.full){
        commonCs = resource.commonCs;
        faceCs = resource.faceCs;
        handCs = resource.handCs;

        blazePoseDetecter = new BlazePoseDetecter(resource.blazePoseResource, blazePoseModel);
        facePipeline = new FacePipeline(resource.faceMeshResource);
        palmDetector = new PalmDetector(resource.blazePalmResource);
        handLandmarkDetector = new HandLandmarkDetector(resource.handLandmarkResource);

        faceVertexBuffer = new ComputeBuffer(faceVertexCount, sizeof(float) * 4);
        leftEyeVertexBuffer = new ComputeBuffer(eyeVertexCount, sizeof(float) * 4);
        rightEyeVertexBuffer = new ComputeBuffer(eyeVertexCount, sizeof(float) * 4);

        leftHandVertexBuffer = new ComputeBuffer(handVertexCount + 1, sizeof(float) * 4);
        rightHandVertexBuffer = new ComputeBuffer(handVertexCount + 1, sizeof(float) * 4);

        letterBoxTexture = new RenderTexture(letterboxWidth, letterboxWidth, 0, RenderTextureFormat.ARGB32);
        letterBoxTexture.enableRandomWrite = true;
        letterBoxTexture.Create();

        handsRegionFromPalm = new ComputeBuffer(2, sizeof(float) * 24);
        leftHandRegionFromPose = new ComputeBuffer(1, sizeof(float) * 24);
        rightHandRegionFromPose = new ComputeBuffer(1, sizeof(float) * 24);
        handCropBuffer = new ComputeBuffer(handCropImageSize * handCropImageSize * 3, sizeof(float));
        deltaLeftHandVertexBuffer = new ComputeBuffer(handVertexCount, sizeof(float) * 4);
        deltaRightHandVertexBuffer = new ComputeBuffer(handVertexCount, sizeof(float) * 4);
    }

    public void Dispose(){
        blazePoseDetecter.Dispose();
        facePipeline.Dispose();
        palmDetector.Dispose();
        handLandmarkDetector.Dispose();

        faceVertexBuffer.Dispose();
        leftEyeVertexBuffer.Dispose();
        rightEyeVertexBuffer.Dispose();

        leftHandVertexBuffer.Dispose();
        rightHandVertexBuffer.Dispose();

        letterBoxTexture.Release();

        handsRegionFromPalm.Dispose();
        
        leftHandRegionFromPose.Dispose();
        rightHandRegionFromPose.Dispose();
        handCropBuffer.Dispose();
        deltaLeftHandVertexBuffer.Dispose();
        deltaRightHandVertexBuffer.Dispose();
    }

    public void ProcessImage(
            Texture inputTexture, 
            BlazePoseModel blazePoseModel = BlazePoseModel.full,
            float poseDetectionThreshold = 0.75f,
            float poseDetectionIouThreshold = 0.3f)
    {
        blazePoseDetecter.ProcessImage(inputTexture, blazePoseModel, poseDetectionThreshold, poseDetectionIouThreshold);

        // Letterboxing scale factor
        var scale = new Vector2(
            Mathf.Max((float)inputTexture.height / inputTexture.width, 1),
            Mathf.Max(1, (float)inputTexture.width / inputTexture.height)
        );
        
        // Image scaling and padding
        // Output image is letter-box image.
        // For example, top and bottom pixels of `letterboxTexture` are black if `inputTexture` size is 1920(width)*1080(height)
        commonCs.SetVector("_spadScale", scale);
        commonCs.SetInt("_letterboxWidth", letterboxWidth);
        commonCs.SetTexture(0, "_letterboxInput", inputTexture);
        commonCs.SetTexture(0, "_letterboxTexture", letterBoxTexture);
        commonCs.Dispatch(0, letterboxWidth / 8, letterboxWidth / 8, 1);

        FaceProcess(inputTexture, letterBoxTexture, scale);
        HandProcess(inputTexture, letterBoxTexture, scale);
    }

    void FaceProcess(Texture inputTexture, Texture letterBoxTexture, Vector2 spadScale){
        facePipeline.ProcessImage(letterBoxTexture);

        faceCs.SetVector("_spadScale", spadScale);
        faceCs.SetBuffer(0, "_faceVertices", facePipeline.RefinedFaceVertexBuffer);
        faceCs.SetBuffer(0, "_faceReconVertices", faceVertexBuffer);
        faceCs.Dispatch(0, faceVertexCount, 1, 1);
        
        // Reconstruct left eye rotation
        faceCs.SetMatrix("_irisCropMatrix", facePipeline.LeftEyeCropMatrix);
        faceCs.SetBuffer(1, "_irisVertices", facePipeline.RawLeftEyeVertexBuffer);
        // facePipeline output is fliped.
        faceCs.SetBuffer(1, "_irisReconVertices", rightEyeVertexBuffer);
        faceCs.Dispatch(1, eyeVertexCount, 1, 1);

        // Reconstruct right eye rotation
        faceCs.SetMatrix("_irisCropMatrix", facePipeline.RightEyeCropMatrix);
        faceCs.SetBuffer(1, "_irisVertices", facePipeline.RawRightEyeVertexBuffer);
        // facePipeline output is fliped.
        faceCs.SetBuffer(1, "_irisReconVertices", leftEyeVertexBuffer);
        faceCs.Dispatch(1, eyeVertexCount, 1, 1);
    }

    void HandProcess(Texture inputTexture, Texture letterBoxTexture, Vector2 spadScale){
        palmDetector.ProcessImage(letterBoxTexture);

        int[] countReadCache = new int[1];
        palmDetector.CountBuffer.GetData(countReadCache, 0, 0, 1);
        var handDetectionCount = countReadCache[0];
        handDetectionCount = (int)Mathf.Min(handDetectionCount, 2);

        bool isNeedLeftFallback = (handDetectionCount == 0);
        bool isNeedRightFallback = (handDetectionCount == 0);
        var scoreCache = new Vector4[1];

        if(handDetectionCount > 0){
            // Hand region bounding box update
            handCs.SetInt("_detectionCount", handDetectionCount);
            handCs.SetFloat("_regionDetectDt", Time.deltaTime);
            handCs.SetBuffer(0, "_palmDetections", palmDetector.DetectionBuffer);
            handCs.SetBuffer(0, "_handsRegionFromPalm", handsRegionFromPalm);
            handCs.Dispatch(0, 1, 1, 1);
        }

        handCs.SetVector("_spadScale", spadScale);
        handCs.SetInt("_isVerticalFlip", 1);
        for(int i=0; i<handDetectionCount; i++){
            handCs.SetInt("_handRegionIndex", i);

            // Hand region cropping
            handCs.SetInt("_handCropImageSize", handCropImageSize);
            handCs.SetTexture(2, "_handCropInput", inputTexture);
            handCs.SetBuffer(2, "_handCropRegion", handsRegionFromPalm);
            handCs.SetBuffer(2, "_handCropOutput", handCropBuffer);
            handCs.Dispatch(2, handCropImageSize / 8, handCropImageSize / 8, 1);

            handLandmarkDetector.ProcessImage(handCropBuffer);

            handLandmarkDetector.OutputBuffer.GetData(scoreCache, 0, 0, 1);
            float score = scoreCache[0].x;
            float handness = scoreCache[0].y;
            bool isRight = handness > 0.5f;
            if(score < 0.5f){
                if(isRight) isNeedRightFallback = true;
                else isNeedLeftFallback = true;
                continue;
            }

            // Key point postprocess
            handCs.SetFloat("_handPostDt", Time.deltaTime);
            handCs.SetBuffer(3, "_handPostInput", handLandmarkDetector.OutputBuffer);
            handCs.SetBuffer(3, "_handPostRegion", handsRegionFromPalm);
            handCs.SetBuffer(3, "_handPostOutput", isRight ? rightHandVertexBuffer : leftHandVertexBuffer);
            handCs.SetBuffer(3, "_handPostDeltaOutput", isRight ? deltaRightHandVertexBuffer : deltaLeftHandVertexBuffer);
            handCs.Dispatch(3, 1, 1, 1);
        }

        if(isNeedRightFallback) HandProcessFromPose(inputTexture, true);
        if(isNeedLeftFallback) HandProcessFromPose(inputTexture, false);
    }

    void HandProcessFromPose(Texture inputTexture, bool isRight){
        // Calculate hand region with pose landmark
        handCs.SetInt("_isRight", isRight?1:0);
        handCs.SetFloat("_bboxDt", Time.deltaTime);
        handCs.SetBuffer(1, "_poseInput", blazePoseDetecter.outputBuffer);
        handCs.SetBuffer(1, "_bboxRegion", isRight ? rightHandRegionFromPose : leftHandRegionFromPose);
        handCs.Dispatch(1, 1, 1, 1);

        var scale = new Vector2(1, 1);
        handCs.SetVector("_spadScale", scale);
        handCs.SetInt("_isVerticalFlip", 0);
        handCs.SetInt("_handRegionIndex", 0);

        // Hand region cropping
        handCs.SetInt("_handCropImageSize", handCropImageSize);
        handCs.SetTexture(2, "_handCropInput", inputTexture);
        handCs.SetBuffer(2, "_handCropRegion", isRight ? rightHandRegionFromPose : leftHandRegionFromPose);
        handCs.SetBuffer(2, "_handCropOutput", handCropBuffer);
        handCs.Dispatch(2, handCropImageSize / 8, handCropImageSize / 8, 1);

        // Hand landmark detection
        handLandmarkDetector.ProcessImage(handCropBuffer);

        // Key point postprocess
        handCs.SetFloat("_handPostDt", Time.deltaTime);
        handCs.SetBuffer(3, "_handPostInput", handLandmarkDetector.OutputBuffer);
        handCs.SetBuffer(3, "_handPostRegion", isRight ? rightHandRegionFromPose : leftHandRegionFromPose);
        handCs.SetBuffer(3, "_handPostOutput", isRight ? rightHandVertexBuffer : leftHandVertexBuffer);
        handCs.SetBuffer(3, "_handPostDeltaOutput", isRight ? deltaRightHandVertexBuffer : deltaLeftHandVertexBuffer);
        handCs.Dispatch(3, 1, 1, 1);
    }
}

}