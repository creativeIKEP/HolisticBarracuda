using UnityEngine;
using Mediapipe.BlazePose;

namespace MediaPipe.Holistic {
    [CreateAssetMenu(fileName = "Holistic", menuName = "ScriptableObjects/Holistic Resource")]
    public class HolisticResource : ScriptableObject
    {
        public BlazePoseResource blazePoseResource;
        public MediaPipe.FaceMesh.ResourceSet faceMeshResource;
        public MediaPipe.BlazePalm.ResourceSet blazePalmResource;
        public MediaPipe.HandLandmark.ResourceSet handLandmarkResource;
        public ComputeShader commonCs;
        public ComputeShader faceCs;
        public ComputeShader handCs;
    }
}