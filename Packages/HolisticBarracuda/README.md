# HolisticBarracuda
`full`

![full](https://user-images.githubusercontent.com/34697515/192131544-97d0aedb-bd4b-477c-a367-4c7f22f8f7cb.gif)

`pose_and_face` 

![pose_and_face](https://user-images.githubusercontent.com/34697515/192131548-66a26715-cc9d-4a1c-a391-3ecd0d648f02.gif)

`pose_and_hand`

![pose_and_hand](https://user-images.githubusercontent.com/34697515/192131549-b5929bd0-de56-4938-9cb2-a816987a639b.gif)

`pose_only`

![pose_only](https://user-images.githubusercontent.com/34697515/192131552-6b2948a5-93f2-47b4-bd45-d11bffe5a58c.gif)

`face_only`

![face_only](https://user-images.githubusercontent.com/34697515/192131531-2b46cfb9-d6b8-4668-81a6-93d6e4595b3f.gif)

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
"jp.ikep.mediapipe.holistic": "1.1.0"
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
        "jp.ikep.mediapipe.holistic": "1.1.0",
        ...
    }
}
```

## Usage Demo
This repository has the demo that inference pose, face and hands landmarks, and visualize landmarks.

Check a Unity [scene](https://github.com/creativeIKEP/HolisticBarracuda/blob/main/Assets/Scenes/Sample.unity), [scripts](https://github.com/creativeIKEP/HolisticBarracuda/tree/main/Assets/Scripts) and [shaders](https://github.com/creativeIKEP/HolisticBarracuda/tree/main/Assets/Shaders) in the ["/Assets"](https://github.com/creativeIKEP/HolisticBarracuda/tree/main/Assets) directory.

## Demo image
Videos for demo was downloaded from [pexels](https://www.pexels.com/ja-jp/).
- https://www.pexels.com/ja-jp/video/5089491/
- https://www.pexels.com/ja-jp/video/4492700/
- https://www.pexels.com/ja-jp/video/8627747/
- https://www.pexels.com/ja-jp/video/2795750/
- https://www.pexels.com/ja-jp/video/6985340/

## Author
[IKEP](https://ikep.jp)

## LICENSE
Copyright (c) 2021 IKEP

[Apache-2.0](/LICENSE.md)