using System;
using System.Linq;
using Game2Week.Battle.View;
using Game2Week.Data;
using Game2Week.EditorTools.Patterns;
using Game2Week.EditorTools.Stages;
using Game2Week.Stages;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;

namespace Game2Week.EditorTools.Build
{
    public static class GraphicsAuthoring
    {
        const string Root="Assets/_Project/Art/Presentation";
        public const string ThemePath="Assets/_Project/Data/Themes/경기장테마목록.asset";
        const string EnvironmentPath="Assets/_Project/Prefabs/Battle/FX/VisualEnvironment.prefab";
        [MenuItem("Tools/Graphics/11단계 그래픽 다시 구성")]
        public static void Build()
        {
            PatternAssetStore.Folder(Root);PatternAssetStore.Folder("Assets/_Project/Data/Themes");
            AssetDatabase.Refresh();
            var toon=AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/ToonCharacter.shadergraph");
            if(!toon)throw new InvalidOperationException("공용 툰 Shader Graph를 먼저 생성하세요.");
            var glow=Material("탄발광",Shader.Find("Game2Week/탄 발광"),Color.white);
            glow.SetColor("_BaseColor",new Color(1,.8f,.1f));
            var fx=Material("효과잔광",Shader.Find("Game2Week/효과 잔광"),Color.white);
            var props=Material("테마소품",toon,Color.white);
            var sky=Material("하늘",Shader.Find("Game2Week/그라디언트 하늘"),Color.white);
            Environment(sky);Renderer();Bullets(glow,fx);Fx(fx);Characters(toon);
            var themes=Themes(props);
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach(string sceneName in new[]{"Battle","MainMenu"})
                {
                    var scene=EditorSceneManager.OpenScene("Assets/_Project/Scenes/"+sceneName+".unity");
                    if(!Object.FindAnyObjectByType<VisualEnvironment>())
                        PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(EnvironmentPath),scene);
                    foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                        if(light.type==LightType.Directional&&!light.GetComponentInParent<VisualEnvironment>())light.enabled=false;
                    foreach(var spawner in Object.FindObjectsByType<StageSpawner>(FindObjectsSortMode.None))
                    {var serialized=new SerializedObject(spawner);serialized.FindProperty("themeCatalog").objectReferenceValue=AssetDatabase.LoadAssetAtPath<ArenaThemeCatalog>(ThemePath);serialized.ApplyModifiedPropertiesWithoutUndo();}
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                if(previous.Length>0&&previous.All(setup=>!string.IsNullOrEmpty(setup.path)))EditorSceneManager.RestoreSceneManagerSetup(previous);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            }
            var model=new StageEditorModel(new StageRepository(StageRepository.DefaultDirectory),ContentCatalogUtility.LoadOrCreate());model.Load();
            for(int i=0;i<model.Count;i++){model.SelectStage(i);string id=themes[i%themes.Length].id;model.Edit(stage=>stage.theme=id);}
            var report=model.SaveAll();if(report.Blocked.Count>0)throw new InvalidOperationException("맵 테마 저장 검증 실패");
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("11단계 공용 그래픽·세 테마·탄 모양·효과를 구성했습니다.");
        }
        static Material Material(string name,Shader shader,Color color)
        {
            if(!shader)throw new InvalidOperationException("셰이더가 없습니다: "+name);
            string path=Root+"/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!material){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
            material.shader=shader;if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",color);
            EditorUtility.SetDirty(material);return material;
        }
        static void Environment(Material sky)
        {
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Root+"/전투후처리.asset");
            if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Root+"/전투후처리.asset");}
            T Component<T>()where T:VolumeComponent=>profile.TryGet<T>(out var component)?component:profile.Add<T>(true);
            var bloom=Component<Bloom>();bloom.intensity.Override(.18f);bloom.threshold.Override(1.1f);
            var colors=Component<ColorAdjustments>();colors.contrast.Override(8);colors.saturation.Override(6);
            var vignette=Component<Vignette>();vignette.intensity.Override(.12f);vignette.smoothness.Override(.45f);
            foreach(var component in profile.components){if(!AssetDatabase.Contains(component))AssetDatabase.AddObjectToAsset(component,profile);EditorUtility.SetDirty(component);}
            EditorUtility.SetDirty(profile);
            var root=new GameObject("공용 그래픽 환경");
            try
            {
                var environment=root.AddComponent<VisualEnvironment>();var data=new SerializedObject(environment);data.FindProperty("sky").objectReferenceValue=sky;data.ApplyModifiedPropertiesWithoutUndo();
                var volume=root.AddComponent<Volume>();volume.isGlobal=true;volume.priority=10;volume.sharedProfile=profile;
                var light=new GameObject("주 조명");light.transform.SetParent(root.transform,false);light.transform.localRotation=Quaternion.Euler(45,-35,0);
                var lamp=light.AddComponent<Light>();lamp.type=LightType.Directional;lamp.color=new Color(1,.94f,.86f);lamp.intensity=1.1f;lamp.shadows=LightShadows.Soft;
                PrefabUtility.SaveAsPrefabAsset(root,EnvironmentPath);
            }
            finally{Object.DestroyImmediate(root);}
        }
        static void Renderer()
        {
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset");
            var outline=renderer.rendererFeatures.OfType<FullScreenPassRendererFeature>().FirstOrDefault(feature=>feature.name=="공용 외곽선");
            if(!outline){outline=ScriptableObject.CreateInstance<FullScreenPassRendererFeature>();outline.name="공용 외곽선";AssetDatabase.AddObjectToAsset(outline,renderer);renderer.rendererFeatures.Add(outline);}
            outline.injectionPoint=FullScreenPassRendererFeature.InjectionPoint.BeforeRenderingPostProcessing;
            outline.requirements=ScriptableRenderPassInput.Depth;outline.fetchColorBuffer=true;
            outline.passMaterial=Material("외곽선",Shader.Find("Game2Week/화면 외곽선"),Color.white);
            foreach(var feature in renderer.rendererFeatures)
            {
                if(feature.GetType().Name=="ScreenSpaceAmbientOcclusion")
                {var serialized=new SerializedObject(feature);serialized.FindProperty("m_Settings.Intensity").floatValue=.22f;serialized.FindProperty("m_Settings.Downsample").boolValue=true;serialized.ApplyModifiedPropertiesWithoutUndo();}
                EditorUtility.SetDirty(feature);
            }
            renderer.SetDirty();EditorUtility.SetDirty(renderer);
        }
        static Mesh SaveMesh(string name,Mesh mesh)
        {
            string path=Root+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);return old;}
            mesh.name=name;AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static void Bullets(Material glow,Material fx)
        {
            var source=GameObject.CreatePrimitive(PrimitiveType.Sphere);var sphere=Object.Instantiate(source.GetComponent<MeshFilter>().sharedMesh);Object.DestroyImmediate(source);
            sphere=SaveMesh("노랑구",sphere);
            var crystal=new Mesh();crystal.vertices=new[]{Vector3.up*.65f,Vector3.down*.65f,Vector3.right*.4f,Vector3.forward*.4f,Vector3.left*.4f,Vector3.back*.4f};
            crystal.triangles=new[]{0,3,2,0,4,3,0,5,4,0,2,5,1,2,3,1,3,4,1,4,5,1,5,2};crystal.RecalculateNormals();crystal=SaveMesh("빨강결정",crystal);
            var vertices=new Vector3[16*6];var triangles=new int[16*6*6];
            for(int i=0;i<16;i++)for(int j=0;j<6;j++)
            {
                float a=i/16f*Mathf.PI*2,b=j/6f*Mathf.PI*2;float r=.42f+Mathf.Cos(b)*.08f;
                vertices[i*6+j]=new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,Mathf.Sin(b)*.08f);
                int k=(i*6+j)*6,n=(i+1)%16*6+j,m=i*6+(j+1)%6,o=(i+1)%16*6+(j+1)%6;
                triangles[k]=i*6+j;triangles[k+1]=n;triangles[k+2]=m;triangles[k+3]=m;triangles[k+4]=n;triangles[k+5]=o;
            }
            var ring=new Mesh{vertices=vertices,triangles=triangles};ring.RecalculateNormals();ring=SaveMesh("파랑고리",ring);
            foreach(string id in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/_Project/Prefabs/Battle/Patterns"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(id);var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if(!asset.GetComponent<Game2Week.Battle.Patterns.Bullet>())continue;
                var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var visual=root.GetComponentInChildren<MeshFilter>();if(!visual)continue;
                    visual.transform.localRotation=Quaternion.identity;visual.transform.localScale=Vector3.one*.28f;visual.sharedMesh=sphere;
                    visual.GetComponent<MeshRenderer>().sharedMaterial=glow;
                    if(!root.TryGetComponent<BulletAppearance>(out var appearance))appearance=root.AddComponent<BulletAppearance>();
                    if(!root.TryGetComponent<TrailRenderer>(out var trail))trail=root.AddComponent<TrailRenderer>();
                    trail.sharedMaterial=fx;trail.time=.12f;trail.minVertexDistance=.08f;trail.startWidth=.06f;trail.endWidth=0;trail.emitting=false;
                    var serialized=new SerializedObject(appearance);
                    foreach(var pair in new[]{("visual",(Object)visual),("sphere",sphere),("crystal",crystal),("ring",ring),("trail",trail)})serialized.FindProperty(pair.Item1).objectReferenceValue=pair.Item2;
                    serialized.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
        }
        static void Fx(Material material)
        {
            string path="Assets/_Project/Prefabs/Battle/FX/BattleFxRig.prefab";var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                if(!root.TryGetComponent<CombatAccentFxModule>(out var module))module=root.AddComponent<CombatAccentFxModule>();
                var serialized=new SerializedObject(module);serialized.FindProperty("material").objectReferenceValue=material;serialized.ApplyModifiedPropertiesWithoutUndo();
                var actions=root.GetComponentInChildren<ActionFxModule>();
                if(actions){var fields=new SerializedObject(actions);fields.FindProperty("particleMaterial").objectReferenceValue=material;fields.ApplyModifiedPropertiesWithoutUndo();}
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        static void Characters(Shader toon)
        {
            foreach(string path in new[]{"Assets/_Project/Prefabs/Battle/Player_Heroine.prefab","Assets/_Project/Prefabs/Enemies/EnemyView_TestBlob.prefab","Assets/_Project/Prefabs/Enemies/EnemyView_TestBlob_Orange.prefab"})
            {
                var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach(var renderer in root.GetComponentsInChildren<Renderer>())
                    {
                        var materials=renderer.sharedMaterials;
                        for(int i=0;i<materials.Length;i++)
                        {
                            var old=materials[i];if(!old||old.shader==toon)continue;
                            var color=old.HasProperty("_BaseColor")?old.GetColor("_BaseColor"):old.HasProperty("_Color")?old.GetColor("_Color"):Color.white;
                            materials[i]=Material("툰_"+old.name.Replace("/","_").Replace("\\","_"),toon,color);
                        }
                        renderer.sharedMaterials=materials;
                    }
                    if(root.GetComponentInChildren<Animator>())HeroineRenderingAuthoring.Merge(root);
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
        }
        static ArenaTheme[] Themes(Material material)
        {
            var ids=new[]{"default","grove","amber"};var names=new[]{"푸른광장","숲빛광장","노을광장"};
            var colors=new[]{new Color(.16f,.23f,.32f),new Color(.16f,.29f,.23f),new Color(.31f,.2f,.2f)};
            var themes=new ArenaTheme[3];
            for(int i=0;i<3;i++)
            {
                string path="Assets/_Project/Data/Themes/"+names[i]+".asset";var theme=AssetDatabase.LoadAssetAtPath<ArenaTheme>(path);
                if(!theme){theme=ScriptableObject.CreateInstance<ArenaTheme>();AssetDatabase.CreateAsset(theme,path);}
                theme.id=ids[i];theme.floor=colors[i];theme.wall=Color.Lerp(colors[i],Color.white,.35f);theme.accent=i==2?new Color(.85f,.52f,.3f):i==1?new Color(.45f,.8f,.5f):new Color(.4f,.75f,.9f);
                theme.propMaterial=material;theme.checker=i==2;EditorUtility.SetDirty(theme);themes[i]=theme;
            }
            var catalog=AssetDatabase.LoadAssetAtPath<ArenaThemeCatalog>(ThemePath);
            if(!catalog){catalog=ScriptableObject.CreateInstance<ArenaThemeCatalog>();AssetDatabase.CreateAsset(catalog,ThemePath);}
            catalog.themes=themes;EditorUtility.SetDirty(catalog);return themes;
        }
    }
}
