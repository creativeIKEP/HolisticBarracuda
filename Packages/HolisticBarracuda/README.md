# HolisticBarracuda
![demo](https://user-images.githubusercontent.com/34697515/136178988-9a6c37cb-09a2-43e4-9f05-f8c4908b8665.gif)

**HolisticBarracuda** is the Unity Package that simultaneously estimates 33 pose, 21 per-hand, and 468 facial landmarks with a monocular color camera only.

HolisticBarracuda runs the pipeline simular to [Mediapipe Holistic](https://google.github.io/mediapipe/solutions/holistic) on the [Unity](https://unity.com/).

## Dependencies
HolisticBarracuda uses the following packages:
- [BlazeFaceBarracuda](https://github.com/keijiro/BlazeFaceBarracuda)
- [FaceLandmarkBarracuda](https://github.com/keijiro/FaceLandmarkBarracuda)
- [IrisBarracuda](https://github.com/keijiro/IrisBarracuda)
- [BlazePalmBarracuda](https://github.com/keijiro/BlazePalmBarracuda)
- [HandLandmarkBarracuda](https://github.com/keijiro/HandLandmarkBarracuda)
- [BlazePoseBarracuda](https://github.com/creativeIKEP/BlazePoseBarracuda)

## Used source codes
Parts of face and hands implementations used many [keijiro](https://github.com/keijiro)'s below source codes. Thanks!
- [FaceMeshBarracuda](https://github.com/keijiro/FaceMeshBarracuda)
- [HandPoseBarracuda](https://github.com/keijiro/HandPoseBarracuda)

## Install
HolisticBarracuda can be installed by adding following sections to your manifest file (`Packages/manifest.json`).

To the `scopedRegistries` section:
```
{
    "name": "Keijiro",
    "url": "https://registry.npmjs.com",
    "scopes": [ "jp.keijiro" ]
},
{
  "name": "creativeikep",
  "url": "https://registry.npmjs.com",
  "scopes": [ "jp.ikep" ]
}
```
To the `dependencies` section:
```
"jp.ikep.mediapipe.holistic": "1.0.1"
```
Finally, the manifest file looks like below:
```
{
    "scopedRegistries": [
        {
            "name": "Keijiro",
            "url": "https://registry.npmjs.com",
            "scopes": [ "jp.keijiro" ]
        },
        {
            "name": "creativeikep",
            "url": "https://registry.npmjs.com",
            "scopes": [ "jp.ikep" ]
        }
    ],
    "dependencies": {
        "jp.ikep.mediapipe.holistic": "1.0.1",
        ...
    }
}
```

## Usage Demo
This repository has the demo that inference pose, face and hands landmarks, and visualize landmarks.

Check a Unity [scene](https://github.com/creativeIKEP/HolisticBarracuda/blob/main/Assets/Scenes/Sample.unity), [scripts](https://github.com/creativeIKEP/HolisticBarracuda/tree/main/Assets/Scripts) and [shaders](https://github.com/creativeIKEP/HolisticBarracuda/tree/main/Assets/Shaders) in the ["/Assets"](https://github.com/creativeIKEP/HolisticBarracuda/tree/main/Assets) directory.

## Demo image
Videos for demoe scene (["/Assets/Scenes/Sample.unity"](https://github.com/creativeIKEP/HolisticBarracuda/blob/main/Assets/Scenes/Sample.unity)) was downloaded from [here](https://www.pexels.com/ja-jp/video/7559286/).

## Author
[IKEP](https://ikep.jp)

## LICENSE
Copyright (c) 2021 IKEP

[Apache-2.0](/LICENSE.md)