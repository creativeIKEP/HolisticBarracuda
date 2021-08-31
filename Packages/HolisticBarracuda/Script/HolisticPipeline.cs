using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MediaPipe.FaceMesh;


namespace MediaPipe.Holistic {

public class HolisticPipeline : System.IDisposable
{
    public ComputeBuffer faceVertexBuffer => facePipeline.RefinedFaceVertexBuffer;
    public ComputeBuffer leftEyeVertexBuffer => facePipeline.RawLeftEyeVertexBuffer;
    public ComputeBuffer rightEyeVertexBuffer => facePipeline.RawRightEyeVertexBuffer;

    FacePipeline facePipeline;
    public Matrix4x4 leftEyeCropMatrix;
    public Matrix4x4 rightEyeCropMatrix;

    public HolisticPipeline(MediaPipe.FaceMesh.ResourceSet faceMeshResource){
        facePipeline = new FacePipeline(faceMeshResource);
    }

    public void ProcessImage(Texture inputTexture){
        facePipeline.ProcessImage(inputTexture);
        leftEyeCropMatrix = facePipeline.LeftEyeCropMatrix;
        rightEyeCropMatrix = facePipeline.RightEyeCropMatrix;
    }

    public void Dispose(){
        facePipeline.Dispose();
    }
}

}