### [1.1.0] - 2022-09-25
- Update BlazePose version
- Fixed an issue that estimation was not performed correctly when the color space was Liner.
- Automatically load `HolisticResource` asset data. The constructor arguments are not required.
- Add new methods (`GetPoseLandmark`, `GetPoseWorldLandmark`, `GetFaceLandmark`, `GetLeftEyeLandmark`, `GetRightEyeLandmark`, `GetLeftHandLandmark` and `GetRightHandLandmark`) for accessing data with CPU (C#).
- Add detection score variables (`faceDetectionScore`, `leftHandDetectionScore` and `rightHandDetectionScore`)
- Improve the stability of hand estimation.

### [1.0.1] - 2021-10-06
This is the first release of `creativeIKEP/HolisticBarracuda`(`jp.ikep.mediapipe.holistic`).