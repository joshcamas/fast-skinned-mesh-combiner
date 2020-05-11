# fast-skinned-mesh-combiner
A much faster implementation of combining skinned meshes without messing with skinned mesh prefabs

This is a MUCH better implementation of an older script I made: https://github.com/joshcamas/skinned-mesh-combiner

There is a limitation: Only a single material is supported. Of course, this shouldn't matter much, since you probably shouldn't have multiple materials per skinned mesh anyways. Regardless, it's definitely there.

Most skinned mesh combiners require actual SkinnedMeshComponents to base the data off of - this means it's a bit slower, since if you want to create a character with a lot of skinned mesh parts, you'll need to first create each individual skinned mesh renderer and bone transforms, then you'll need to merge them and destroy the old renderers.

This solution skips this issue - simply create a single rig, then give it a bunch of meshe datas - it then spits out a single SkinnedMeshComponent.

There are some downsides - by far the biggest one is the fact that each mesh being merged must have the same exact number of boneweights. So this solution is definitely not for everyone.
