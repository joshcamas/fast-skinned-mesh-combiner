using System.Collections.Generic;
using UnityEngine;

namespace SpellcastStudios
{
  public class MeshCombiner 
  {
  
    /// <summary>
    /// Combines meshes and creates a new skinned mesh renderer, parenting to baseBone
    /// </summary>
    /// <param name="baseBone"></param>
    /// <param name="material"></param>
    /// <param name="bones"></param>
    /// <param name="meshes"></param>
    /// <param name="rects">Optional UV's to apply to each mesh</param>
    /// <returns></returns>
    public static SkinnedMeshRenderer CombineFast(Transform baseBone, Material material, Transform[] bones, Mesh[] meshes, Rect[] rects = null)
    {
        // Create mesh renderer
        GameObject newGameObject = new GameObject("Avatar", typeof(SkinnedMeshRenderer));
        newGameObject.transform.parent = baseBone.parent;
        newGameObject.transform.localPosition = Vector3.zero;
        newGameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        SkinnedMeshRenderer skinnedMeshRenderer = newGameObject.GetComponent<SkinnedMeshRenderer>();

        CombineFast(skinnedMeshRenderer, baseBone, material, bones, meshes, rects);

        return skinnedMeshRenderer;
    }
    
    /// <summary>
    /// Combines meshes and implements onto an existing skinned mesh renderer + bones
    /// </summary>
    /// <param name="skinnedMeshRenderer"></param>
    /// <param name="baseBone"></param>
    /// <param name="material"></param>
    /// <param name="bones"></param>
    /// <param name="meshes"></param>
    /// <param name="uvs">Optional UV's to apply to each mesh</param>
    public static void CombineFast(SkinnedMeshRenderer skinnedMeshRenderer, Transform baseBone, Material material, Transform[] bones, Mesh[] meshes, Rect[] uvs=null)
    {
        if (meshes.Length == 0)
            return;

        CombineInstance[] combineInstances = new CombineInstance[meshes.Length];

        for(int i =0;i<meshes.Length;i++)
        {
            if (meshes[i] == null)
                continue;

            combineInstances[i] = new CombineInstance();
            combineInstances[i].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.zero),new Vector3(100,100,100));
            combineInstances[i].mesh = meshes[i];
        }

        //Copy bind poses from first mesh in array
        Matrix4x4[] bindPoses = meshes[0].bindposes;

        Mesh combined_new_mesh = new Mesh();
        combined_new_mesh.CombineMeshes(combineInstances,true,false);
        combined_new_mesh.bindposes = bindPoses;

        //Note: Mesh.boneWeights returns a copy of bone weights (this is undocumented)
        BoneWeight[] newboneweights = combined_new_mesh.boneWeights;
        Vector2[] newUvs = combined_new_mesh.uv;

        //Blendshape dictionary contains list of matching blendshape indices per mesh. 
        //Index is offset by 1, so 0 = no shape exists for that mesh
        Dictionary<string, int[]> blendshapes = new Dictionary<string, int[]>();

        //Realign boneweights, apply uv's, and map blendshapes 
        int offset = 0;
        for (int i=0;i<meshes.Length;i++)
        {
            for(int k=0;k<meshes[i].vertexCount;k++)
            {
                if(i > 0)
                {
                    newboneweights[offset + k].boneIndex0 -= bones.Length * i;
                    newboneweights[offset + k].boneIndex1 -= bones.Length * i;
                    newboneweights[offset + k].boneIndex2 -= bones.Length * i;
                    newboneweights[offset + k].boneIndex3 -= bones.Length * i;
                }

                if(uvs != null)
                    newUvs[offset + k] = new Vector2(newUvs[offset + k].x * uvs[i].width + uvs[i].x, newUvs[offset + k].y * uvs[i].height + uvs[i].y);
            }

            offset += meshes[i].vertexCount;

            for(int k=0;k<meshes[i].blendShapeCount;k++)
            {
                string key = meshes[i].GetBlendShapeName(k);

                if(!blendshapes.ContainsKey(key))
                    blendshapes[key] = new int[meshes.Length];

                blendshapes[key][i] = k + 1;
            }
        }

        Vector3[] deltaVertices = null;
        Vector3[] deltaTangents = null;
        Vector3[] deltaNormals = null;

        if (blendshapes.Count > 0)
        {
            deltaVertices = new Vector3[combined_new_mesh.vertexCount];
            deltaTangents = new Vector3[combined_new_mesh.vertexCount];
            deltaNormals = new Vector3[combined_new_mesh.vertexCount];
        }

        //We assume all blendshapes only have a single frame, aka 0 (empty) to 1 (full). 
        //So we just copy the last frame in each blendshape to a weight of 1 
        foreach (KeyValuePair<string,int[]> shape in blendshapes)
        {
            offset = 0;

            for (int i = 0; i < meshes.Length; i++)
            {
                int vcount = meshes[i].vertexCount;

                //No blendshape for this mesh
                if (shape.Value[i] == 0)
                {
                    //TODO: Research whether it's better to create a new array initially, or manually clear them as needed
                    System.Array.Clear(deltaVertices, offset, vcount);
                    System.Array.Clear(deltaTangents, offset, vcount);
                    System.Array.Clear(deltaNormals, offset, vcount);

                    offset += vcount;
                    continue;
                }

                //Since GetBlendShapeFrameVertices requires matching sizes of arrays (stupid), we gotta create these every time -_-
                Vector3[] tempDeltaVertices = new Vector3[vcount];
                Vector3[] tempDeltaTangents = new Vector3[vcount];
                Vector3[] tempDeltaNormals = new Vector3[vcount];

                int frame = (meshes[i].GetBlendShapeFrameCount(shape.Value[i] - 1) - 1);

                meshes[i].GetBlendShapeFrameVertices(shape.Value[i] - 1, frame, tempDeltaVertices, tempDeltaNormals, tempDeltaTangents);

                System.Array.Copy(tempDeltaVertices, 0, deltaVertices, offset, vcount);
                System.Array.Copy(tempDeltaNormals, 0, deltaNormals, offset, vcount);
                System.Array.Copy(tempDeltaTangents, 0, deltaTangents, offset, vcount);

                offset += vcount;
            }

            //Apply
            combined_new_mesh.AddBlendShapeFrame(shape.Key, 1, deltaVertices, deltaNormals, deltaTangents);
        }

        if (uvs != null)
            combined_new_mesh.uv = newUvs;

        combined_new_mesh.boneWeights = newboneweights;

        combined_new_mesh.RecalculateBounds();

        skinnedMeshRenderer.sharedMesh = combined_new_mesh;
        skinnedMeshRenderer.sharedMaterial = material;
        skinnedMeshRenderer.bones = bones;
        skinnedMeshRenderer.rootBone = baseBone;
    }
  }
}
