# HolisticBarracuda Usage Documentation
## Dependencies
HolisticBarracuda uses the following packages:
- [BlazeFaceBarracuda](https://github.com/keijiro/BlazeFaceBarracuda)
- [FaceLandmarkBarracuda](https://github.com/keijiro/FaceLandmarkBarracuda)
- [IrisBarracuda](https://github.com/keijiro/IrisBarracuda)
- [BlazePalmBarracuda](https://github.com/keijiro/BlazePalmBarracuda)
- [HandLandmarkBarracuda](https://github.com/keijiro/HandLandmarkBarracuda)
- [BlazePoseBarracuda](https://github.com/creativeIKEP/BlazePoseBarracuda)

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