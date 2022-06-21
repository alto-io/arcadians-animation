# Animated Arcadians

## Demo

https://arcadians-animation.herokuapp.com/

## Unity Project

Unity project to demonstrate the animated arcadians.

Open the SampleScene to test the demo. 

Open the UIDebug.cs shows how to use the Arcadian class

## Using in your Unity project

### Import the unitypackage
Found in the [releases page](https://github.com/alto-io/arcadians-animation/releases)
This contains the Unity/Assets/Arcadians directory. Import it to a separate project so you can use the animated arcadians

### Add the Arcadian prefab to the scene

### With another script link the Arcadian prefab and call its functions

```
public class Test : MonoBehaviour
{
	public Arcadian arcadian;

	private void Start()
	{
		arcadian.Load(1234); // pass the token id of the arcadian
	}

	private vodi AnimWalk()
	{
		arcadian.SetTrigger("Walk");
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

