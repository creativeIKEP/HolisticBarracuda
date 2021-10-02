using UnityEngine;
using Mediapipe.BlazePose;
using MediaPipe.FaceMesh;
using MediaPipe.BlazePalm;
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
    PalmDetector palmDetector;
    HandLandmarkDetector handLandmarkDetector;
    ComputeShader cs;
    RenderTexture letterBoxTexture;
    ComputeBuffer leftHandRegion;
    public RenderTexture leftHandCropTexture;
    ComputeBuffer rightHandRegion;
    public RenderTexture rightHandCropTexture;

    ComputeBuffer handsRegion;
    ComputeBuffer handCropBuffer;


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

        palmDetector = new PalmDetector(resource.blazePalmResource);
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

        handsRegion = new ComputeBuffer(2, sizeof(float) * 24);
        handCropBuffer = new ComputeBuffer(handCropImageSize * handCropImageSize * 3, sizeof(float));
    }

    public void ProcessImage(Texture inputTexture, BlazePoseModel blazePoseModel = BlazePoseModel.full){
        blazePoseDetecter.ProcessImage(inputTexture, blazePoseModel);
        FaceProcess(inputTexture);
        HandProcess(inputTexture);
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

    void HandProcess(Texture inputTexture){
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

        palmDetector.ProcessImage(letterBoxTexture);

        // Hand region bounding box update
        cs.SetFloat("_bbox_dt", Time.deltaTime);
        cs.SetBuffer(6, "_bbox_count", palmDetector.CountBuffer);
        cs.SetBuffer(6, "_bbox_palm", palmDetector.DetectionBuffer);
        cs.SetBuffer(6, "_bbox_region", handsRegion);
        cs.Dispatch(6, 1, 1, 1);

        int[] countReadCache = new int[1];
        palmDetector.CountBuffer.GetData(countReadCache, 0, 0, 1);
        var count = countReadCache[0];
        count = (int)Mathf.Min(count, 2);

        bool isNeedLeftFallback = (count == 0);
        bool isNeedRightFallback = (count == 0);
        var scoreCache = new Vector4[1];

        for(int i=0; i<count; i++){
            // Hand region cropping
            cs.SetInt("_handsCropImageSize", handCropImageSize);
            cs.SetTexture(7, "_handsCropInput", inputTexture);
            cs.SetBuffer(7, "_handsCropRegion", handsRegion);
            cs.SetInt("_handsIndex", i);
            cs.SetBuffer(7, "_handsCropOutput", handCropBuffer);
            cs.Dispatch(7, handCropImageSize / 8, handCropImageSize / 8, 1);

            handLandmarkDetector.ProcessImage(handCropBuffer);

            // Key point postprocess
            cs.SetFloat("_handsPostDt", Time.deltaTime);
            cs.SetBuffer(8, "_handsPostInput", handLandmarkDetector.OutputBuffer);
            cs.SetBuffer(8, "_handsPostRegion", handsRegion);
            cs.SetBuffer(8, "_handsPostLeftOutput", leftHandVertexBuffer);
            cs.SetBuffer(8, "_handsPostRightOutput", rightHandVertexBuffer);
            cs.Dispatch(8, 1, 1, 1);

            handLandmarkDetector.OutputBuffer.GetData(scoreCache, 0, 0, 1);
            float score = scoreCache[0].x;
            float handness = scoreCache[0].y;
            bool isRight = handness > 0.5f;
            if(isRight && score < 0.5f){
                isNeedRightFallback = true;
            }
            if(!isRight && score < 0.5f){
                isNeedLeftFallback = true;
            }
        }

        if(isNeedRightFallback) HandProcessFromPose(inputTexture, true);
        if(isNeedLeftFallback) HandProcessFromPose(inputTexture, false);
    }

    void HandProcessFromPose(Texture inputTexture, bool isRight){
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

        palmDetector.Dispose();
        handLandmarkDetector.Dispose();
        leftHandRegion.Dispose();
        leftHandCropTexture.Release();
        leftHandVertexBuffer.Dispose();
        rightHandRegion.Dispose();
        rightHandCropTexture.Release();
        rightHandVertexBuffer.Dispose();

        handsRegion.Dispose();
        handCropBuffer.Dispose();
    }
}

}