# HolisticBarracuda
![demo](https://user-images.githubusercontent.com/34697515/136178988-9a6c37cb-09a2-43e4-9f05-f8c4908b8665.gif)

`HolisticBarracuda` is the Unity Package that simultaneously estimates 33 pose, 21 per-hand, and 468 facial landmarks with a monocular color camera only.
HolisticBarracuda runs the pipeline simular to [Mediapipe Holistic](https://google.github.io/mediapipe/solutions/holistic) on the [Unity](https://unity.com/).

## Dependencies
HolisticBarracuda uses the following packages:
- [BlazeFaceBarracuda](https://github.com/keijiro/BlazeFaceBarracuda)
- [FaceLandmarkBarracuda](https://github.com/keijiro/FaceLandmarkBarracuda)
- [IrisBarracuda](https://github.com/keijiro/IrisBarracuda)
- [BlazePalmBarracuda](https://github.com/keijiro/BlazePalmBarracuda)
- [HandLandmarkBarracuda](https://github.com/keijiro/HandLandmarkBarracuda)

## Used source codes
Parts of face and hands implementations used many [keijiro](https://github.com/keijiro)'s below source codes. Thanks!
- [FaceMeshBarracuda](https://github.com/keijiro/FaceMeshBarracuda)
- [HandPoseBarracuda](https://github.com/keijiro/HandPoseBarracuda)