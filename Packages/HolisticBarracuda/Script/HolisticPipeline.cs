using UnityEngine;
using MediaPipe.FaceMesh;


namespace MediaPipe.Holistic {

public class HolisticPipeline : System.IDisposable
{
    public ComputeBuffer faceVertexBuffer => facePipeline.RefinedFaceVertexBuffer;
    public ComputeBuffer leftEyeVertexBuffer;
    public ComputeBuffer rightEyeVertexBuffer;

    FacePipeline facePipeline;
    ComputeShader cs;

    public HolisticPipeline(HolisticResource resource){
        cs = resource.cs;
        facePipeline = new FacePipeline(resource.faceMeshResource);
        leftEyeVertexBuffer = new ComputeBuffer(facePipeline.RawLeftEyeVertexBuffer.count, sizeof(float) * 4);
        rightEyeVertexBuffer = new ComputeBuffer(facePipeline.RawRightEyeVertexBuffer.count, sizeof(float) * 4);
    }

    public void ProcessImage(Texture inputTexture){
        facePipeline.ProcessImage(inputTexture);
        
        // Reconstruct left eye rotation
        cs.SetMatrix("_irisCropMatrix", facePipeline.LeftEyeCropMatrix);
        cs.SetBuffer(0, "_IrisVertices", facePipeline.RawLeftEyeVertexBuffer);
        cs.SetBuffer(0, "_IrisReconVertices", leftEyeVertexBuffer);
        cs.Dispatch(0, facePipeline.RawLeftEyeVertexBuffer.count, 1, 1);

        // Reconstruct right eye rotation
        cs.SetMatrix("_irisCropMatrix", facePipeline.RightEyeCropMatrix);
        cs.SetBuffer(0, "_IrisVertices", facePipeline.RawRightEyeVertexBuffer);
        cs.SetBuffer(0, "_IrisReconVertices", rightEyeVertexBuffer);
        cs.Dispatch(0, facePipeline.RawRightEyeVertexBuffer.count, 1, 1);
    }

    public void Dispose(){
        facePipeline.Dispose();
        leftEyeVertexBuffer.Dispose();
        rightEyeVertexBuffer.Dispose();
    }
}

}