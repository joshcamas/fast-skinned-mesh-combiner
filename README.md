# fast-skinned-mesh-combiner
A much faster implementation of combining skinned meshes in Unity without messing with skinned mesh prefabs.

This is a MUCH better implementation of an older script I made: https://github.com/joshcamas/skinned-mesh-combiner

Most skinned mesh combiners require actual SkinnedMeshComponents to base the data off of - this means it's a bit slower, since if you want to create a character with a lot of skinned mesh parts, you'll need to first create each individual skinned mesh renderer and bone transforms, then you'll need to merge them and destroy the old renderers.

This solution skips this issue - simply create a single rig, then give it a bunch of meshe datas - it then spits out a single SkinnedMeshComponent.

### Features
- Quite fast!
- Shape Keys (supports 1 frame)
- Optionally can apply UV's as it goes - this can be used if you're generating an atlas beforehand

### Limitations
- Single material
- All meshes must have the same bone weights
- Still slow due to unity shenanigans - specifically AddBlendShapeFrame is ridiculously slow, costing up to 1ms per call
- All done on a single frame. On my personal version I've modified this to take multiple frames when desired, making it much less destructive to your main thread. If anyone wants this version, please make a issue for it, and I can upload it. :)

### Usage

```c#

//Create Rig (Often from an FBX)
GameObject rig = GameObject.Instantiate(myRig);

//Find the "packed" renderer, which has bone info
SkinnedMeshRenderer rigRenderer = rig.GetComponentInChildren<SkinnedMeshRenderer>();
Transform[] bones = rigRenderer.bones
Transform rootBone = rigRenderer.rootBone;

//Pack! (In this case, by overwriting the already existing renderer found on the rig)
SpellcastStudios.MeshCombiner.CombineFast(rigRenderer,rootBone,material,bones,meshes);

```

### Todo
- Remove odd implementation of rootBone
- Show how Atlas can be used
