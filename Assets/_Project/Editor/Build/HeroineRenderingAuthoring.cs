using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Build
{
    /// <summary>같은 모델·뼈·재질을 유지하고 파츠 렌더러를 재질별 하위 메시로 합쳐 제출 비용을 줄인다.</summary>
    public static class HeroineRenderingAuthoring
    {
        public static void Merge(GameObject root)
        {
            var animator=root.GetComponentInChildren<Animator>();
            var sources=root.GetComponentsInChildren<SkinnedMeshRenderer>().Where(renderer=>renderer.name!="통합외형").ToArray();
            if(!animator||sources.Length==0)return;
            var bones=sources.SelectMany(renderer=>renderer.bones).Distinct().ToArray();
            var lookup=bones.Select((bone,index)=>(bone,index)).ToDictionary(pair=>pair.bone,pair=>pair.index);
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var colors=new List<Color>();var weights=new List<BoneWeight>();
            var materials=new List<Material>();var triangles=new List<List<int>>();
            foreach(var source in sources)
            {
                var mesh=source.sharedMesh;if(!mesh)continue;
                int offset=vertices.Count;var matrix=animator.transform.worldToLocalMatrix*source.transform.localToWorldMatrix;
                var positions=mesh.vertices;var directions=mesh.normals;var vertexColors=mesh.colors;var boneWeights=mesh.boneWeights;
                for(int i=0;i<positions.Length;i++)
                {
                    vertices.Add(matrix.MultiplyPoint3x4(positions[i]));normals.Add(directions.Length>i?matrix.MultiplyVector(directions[i]).normalized:Vector3.up);
                    colors.Add(vertexColors.Length>i?vertexColors[i]:Color.white);
                    var weight=boneWeights.Length>i?boneWeights[i]:new BoneWeight{weight0=1};
                    int Map(int index)=>index<source.bones.Length?lookup[source.bones[index]]:0;
                    weight.boneIndex0=Map(weight.boneIndex0);weight.boneIndex1=Map(weight.boneIndex1);weight.boneIndex2=Map(weight.boneIndex2);weight.boneIndex3=Map(weight.boneIndex3);weights.Add(weight);
                }
                for(int i=0;i<mesh.subMeshCount;i++)
                {
                    var material=source.sharedMaterials[Mathf.Min(i,source.sharedMaterials.Length-1)];int slot=materials.IndexOf(material);
                    if(slot<0){slot=materials.Count;materials.Add(material);triangles.Add(new List<int>());}
                    triangles[slot].AddRange(mesh.GetTriangles(i).Select(index=>index+offset));
                }
                // 원본은 자세 제작 도구의 눈 위치 검사에 남기고 렌더 제출만 끈다.
                source.enabled=false;
            }
            var combined=new Mesh{name="주인공 통합 렌더 메시"};combined.SetVertices(vertices);combined.SetNormals(normals);combined.SetColors(colors);
            combined.boneWeights=weights.ToArray();combined.bindposes=bones.Select(bone=>bone.worldToLocalMatrix*animator.transform.localToWorldMatrix).ToArray();
            combined.subMeshCount=materials.Count;for(int i=0;i<triangles.Count;i++)combined.SetTriangles(triangles[i],i);combined.RecalculateBounds();
            string path="Assets/_Project/Art/Characters/Heroine/Heroine_RenderCombined.asset";var stored=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(stored){EditorUtility.CopySerialized(combined,stored);Object.DestroyImmediate(combined);EditorUtility.SetDirty(stored);}
            else{stored=combined;AssetDatabase.CreateAsset(stored,path);}
            var existing=animator.transform.Find("통합외형");
            if(!existing){existing=new GameObject("통합외형").transform;existing.SetParent(animator.transform,false);}
            if(!existing.TryGetComponent<SkinnedMeshRenderer>(out var renderer))renderer=existing.gameObject.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh=stored;renderer.sharedMaterials=materials.ToArray();renderer.bones=bones;renderer.rootBone=animator.transform;
            renderer.localBounds=stored.bounds;renderer.updateWhenOffscreen=true;renderer.enabled=true;
            Debug.Log($"주인공 외형 유지: 파츠 {sources.Length}개 → 재질 묶음 {materials.Count}개");
        }
    }
}
