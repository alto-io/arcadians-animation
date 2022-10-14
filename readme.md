# Animated Arcadians

## Demo

https://arcadians-animation.herokuapp.com/

## Unity Project

Unity project to demonstrate the animated arcadians.

Open the SampleScene to test the demo. 

Open the UIDebug.cs shows how to use the Arcadian class

## Using in your Unity project

### 1. Import the unitypackage
Found in the [releases page](https://github.com/alto-io/arcadians-animation/releases)
This contains the Unity/Assets/Arcadians directory. Import it to a separate project so you can use the animated arcadians

### 2. Add the Arcadian prefab to the scene

### 3. With another script link the Arcadian prefab and call its functions

```
using UnityEngine;
using OPGames.Arcadians;

public class Test : MonoBehaviour
{
	public Arcadian arcadian;

	private void Start()
	{
		arcadian.Load(1234, null); 
	}
}
```

## How it works

The Arcadian prefab is the main thing you need to use. Inside this are the male and female mesh renderers of Arcadians.

Calling Arcadian.Load(tokenId) will do the following steps:

1. Get the metadata from api.arcadians.io/{tokenId}
1. The resulting json data will be parsed to get the gender and what parts are used for that particular Arcadian.
1. If the arcadian is male, the female mesh renderer will be hidden and vice versa
1. Each part will then be loaded from Resources folder and will be assigned as a texture to that specific part

## Blender File

We used 3D mesh because we needed to deform an animate the parts. The mesh and animations are authored in Blender.

The Blender directory contains the Blend file used to export the FBX to Unity.

The Blend file can be exported to GLTF as well and imported to other open source game frameworks that support GLTF

## Importing to other game engines or frameworks

Most 3D capable game engines or frameworks will be able to import and use the animated Arcadian mesh. You also need the individual parts under `Unity/Assets/Arcadians/Resources/Parts` and use the correct texture to use based on the metadata

## Future development

Pets are not yet implemented here and in Arcadeum Arena. We will get to this in the future

## External Dependencies

- [UniTask v2](https://github.com/Cysharp/UniTask)
- [ChainSafe](https://github.com/ChainSafe/web3.unity)
- [Json.net](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.0/manual/index.html)
