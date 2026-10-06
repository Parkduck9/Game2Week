using System.Linq;
using Game2Week.Battle.View.Animation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game2Week.EditorTools.Stages
{
    public static class HeroineV2Authoring
    {
        public static void Capture()
        {
            System.IO.Directory.CreateDirectory("Logs");
            var instance=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Battle/Player_Heroine.prefab"));
            var render=new PreviewRenderUtility();
            try
            {
                render.camera.nearClipPlane=.01f;render.camera.farClipPlane=10;render.lights[0].intensity=1.4f;render.lights[0].transform.rotation=Quaternion.Euler(30,30,0);render.ambientColor=Color.gray;
                foreach(int angle in new[]{0,90,180})
                {
                    render.BeginPreview(new Rect(0,0,800,800),GUIStyle.none);
                    render.camera.transform.position=Quaternion.Euler(0,angle,0)*new Vector3(0,.5f,2.2f);render.camera.transform.LookAt(new Vector3(0,.5f,0));render.camera.fieldOfView=32;
                    var meshes=new System.Collections.Generic.List<Mesh>();
                    foreach(var skin in instance.GetComponentsInChildren<SkinnedMeshRenderer>()){var mesh=new Mesh();skin.BakeMesh(mesh);meshes.Add(mesh);render.DrawMesh(mesh,skin.transform.localToWorldMatrix,skin.sharedMaterial,0);}
                    render.Render(true);SaveCapture(render.EndPreview(),"Logs/heroine_v2_"+angle+".png",800,800);
                    foreach(var mesh in meshes)Object.DestroyImmediate(mesh);
                }
                var repo=new Game2Week.Stages.StageRepository(Game2Week.Stages.StageRepository.DefaultDirectory);
                using(var map=new StagePreview3D())SaveCapture(map.Render(new Rect(0,0,960,600),repo.LoadStage(repo.LoadIndex().stages[0]).Stage,20,45),"Logs/stage_m5_preview.png",960,600);
            }
            finally{render.Cleanup();Object.DestroyImmediate(instance);}
            Debug.Log("v2 정면·측면·후면 및 맵툴 3D 미리보기 캡처 완료");
        }
        static void SaveCapture(Texture image,string path,int width,int height)
        {
            var rt=RenderTexture.GetTemporary(width,height,0);var previous=RenderTexture.active;var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{Graphics.Blit(image,rt);RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();System.IO.File.WriteAllBytes(path,pixels.EncodeToPNG());}
            finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(pixels);}
        }
        public static void Apply()
        {
            const string path="Assets/_Project/Art/Characters/Heroine/heroine_v2.glb";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var clips=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().ToArray();
            if(clips.Length!=10)throw new System.InvalidOperationException("v2 동작 10종 임포트 실패: "+clips.Length);
            const string controllerPath="Assets/_Project/Art/Characters/Heroine/HeroineV2.controller";
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if(!controller)controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine=controller.layers[0].stateMachine;
            foreach(var old in machine.states)machine.RemoveState(old.state);
            foreach(var clip in clips)
            {
                var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=clip.name=="Idle"||clip.name=="Run"||clip.name=="Brace";AnimationUtility.SetAnimationClipSettings(clip,settings);
                var state=machine.AddState(clip.name);state.motion=clip;if(clip.name=="Idle")machine.defaultState=state;
            }
            const string prefabPath="Assets/_Project/Prefabs/Battle/Player_Heroine.prefab";
            var root=PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                foreach(Transform child in root.transform.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
                var model=(GameObject)PrefabUtility.InstantiatePrefab(source,root.transform);model.name="HeroineV2";
                var animator=model.GetComponentInChildren<Animator>();if(!animator)animator=model.AddComponent<Animator>();
                animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                var driver=root.GetComponent<PlayerAnimationDriver>();if(!driver)driver=root.AddComponent<PlayerAnimationDriver>();driver.Configure(animator);
                PrefabUtility.SaveAsPrefabAsset(root,prefabPath);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
            AssetDatabase.SaveAssets();Debug.Log("주인공 v2 스킨·동작 10종·컨트롤러·플레이어 프리팹 연결 완료. 씬 변경 없음.");
        }
    }
}
