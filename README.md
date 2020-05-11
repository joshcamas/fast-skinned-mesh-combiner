# fast-skinned-mesh-combiner
A much faster implementation of combining skinned meshes without messing with skinned mesh prefabs

This is a MUCH better implementation of an older script I made: https://github.com/joshcamas/skinned-mesh-combiner

Most skinned mesh combiners require actual SkinnedMeshComponents to base the data off of - this means it's a bit slower, since if you want to create a character with a lot of skinned mesh parts, you'll need to first create each individual skinned mesh renderer and bone transforms, then you'll need to merge them and destroy the old renderers.

This solution skips this issue - simply create a single rig, then give it a bunch of meshe datas - it then spits out a single SkinnedMeshComponent.

## Features ##
- Quite fast!
- Bone Weights (supports 1 frame)
- Optionally can apply UV's as it goes - this can be used if you're generating an atlas beforehand

## Limitations ##
- Single material
- All meshes must have the same bone weights
