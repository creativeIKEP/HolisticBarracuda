using UnityEngine;

namespace MediaPipe.Holistic {
    [CreateAssetMenu(fileName = "Holistic", menuName = "ScriptableObjects/Holistic Resource")]
    public class HolisticResource : ScriptableObject
    {
        public MediaPipe.FaceMesh.ResourceSet faceMeshResource;
        public ComputeShader cs;
    }
}