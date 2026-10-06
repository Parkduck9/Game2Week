using System;
using System.Linq;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.Patterns.Trajectories;
using Game2Week.Data;
using Game2Week.Data.Patterns;
using Game2Week.EditorTools.Stages;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Patterns
{
    public static class PatternAssetStore
    {
        public const string Root="Assets/_Project/Data/Patterns/Graph";
        public const string Prefabs="Assets/_Project/Prefabs/Battle/Patterns/Graph";
        public static GraphPatternDefinition[] List()=>AssetDatabase.FindAssets("t:GraphPatternDefinition")
            .Select(id=>AssetDatabase.LoadAssetAtPath<GraphPatternDefinition>(AssetDatabase.GUIDToAssetPath(id))).OrderBy(d=>d.name).ToArray();
        public static GraphPatternDefinition Create(string label,GraphPatternDefinition source=null)
        {
            Folder(Root);Folder(Prefabs);
            var d=source?UnityEngine.Object.Instantiate(source):ScriptableObject.CreateInstance<GraphPatternDefinition>();
            d.hideFlags=HideFlags.None;
            var path=AssetDatabase.GenerateUniqueAssetPath(Root+"/"+SafeName(label)+".asset");
            AssetDatabase.CreateAsset(d,path);Save(d);return d;
        }
        public static AttackPatternData Save(GraphPatternDefinition d)
        {
            var errors=PatternGraphRules.Validate(d);
            if(errors.Count>0)throw new InvalidOperationException(string.Join("\n",errors));
            string path=AssetDatabase.GetAssetPath(d);
            if(!path.StartsWith(Root+"/",StringComparison.Ordinal))throw new InvalidOperationException("Graph 폴더의 에셋만 저장합니다.");
            Folder(Prefabs);
            string stem=System.IO.Path.GetFileNameWithoutExtension(path);
            string prefabPath=Prefabs+"/"+stem+".prefab";
            var root=new GameObject(stem);
            try
            {
                var bullet=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Battle/Patterns/Bullet_Yellow.prefab");
                if(!bullet)throw new InvalidOperationException("공통 노랑 탄 프리팹이 없습니다.");
                root.AddComponent<GraphAttackPattern>().Configure(d,bullet.GetComponent<Bullet>(),
                    AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Placeholder/M_YellowAttack.mat"));
                PrefabUtility.SaveAsPrefabAsset(root,prefabPath);
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
            var dataPath=Root+"/Attack_"+stem+".asset";
            var attack=AssetDatabase.LoadAssetAtPath<AttackPatternData>(dataPath);
            if(!attack){attack=ScriptableObject.CreateInstance<AttackPatternData>();AssetDatabase.CreateAsset(attack,dataPath);}
            var serialized=new SerializedObject(attack);
            serialized.FindProperty("displayName").stringValue=stem;
            serialized.FindProperty("patternPrefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(d);EditorUtility.SetDirty(attack);AssetDatabase.SaveAssets();
            ContentCatalogUtility.Refresh(ContentCatalogUtility.LoadOrCreate());return attack;
        }
        public static void Delete(GraphPatternDefinition d)
        {
            string path=AssetDatabase.GetAssetPath(d);
            if(!path.StartsWith(Root+"/",StringComparison.Ordinal))throw new InvalidOperationException("Graph 폴더의 에셋만 삭제합니다.");
            string stem=System.IO.Path.GetFileNameWithoutExtension(path);
            string attackName="Attack_"+stem;
            var repo=new Game2Week.Stages.StageRepository(Game2Week.Stages.StageRepository.DefaultDirectory);
            foreach(var id in repo.LoadIndex().stages)
            {
                var stage=repo.LoadStage(id).Stage;
                if(stage!=null&&stage.enemyTurn.patterns.Contains(attackName))
                    throw new InvalidOperationException(id+"에서 사용하는 패턴입니다. Stage Editor에서 먼저 연결을 해제하세요.");
            }
            AssetDatabase.DeleteAsset(Prefabs+"/"+stem+".prefab");
            AssetDatabase.DeleteAsset(Root+"/Attack_"+stem+".asset");
            AssetDatabase.DeleteAsset(path);ContentCatalogUtility.Refresh(ContentCatalogUtility.LoadOrCreate());
        }
        public static void Folder(string path)
        {if(AssetDatabase.IsValidFolder(path))return;var parent=System.IO.Path.GetDirectoryName(path).Replace('\\','/');Folder(parent);AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(path));}
        static string SafeName(string name)
        {foreach(char c in System.IO.Path.GetInvalidFileNameChars())name=name.Replace(c,'_');return string.IsNullOrWhiteSpace(name)?"NewPattern":name;}
        // Explicit authoring command, no scene, input, stage layout or Claude assets touched.
        [MenuItem("Tools/Pattern Editor Presets/Create Missing 8 Presets")]
        public static void CreatePresets()
        {
            foreach(TrajectoryKind kind in Enum.GetValues(typeof(TrajectoryKind)))
            {
                string label="Graph_"+kind;
                if(AssetDatabase.LoadAssetAtPath<GraphPatternDefinition>(Root+"/"+label+".asset"))continue;
                var draft=ScriptableObject.CreateInstance<GraphPatternDefinition>();draft.trajectory=kind;
                if(kind==TrajectoryKind.Spiral||kind==TrajectoryKind.FigureEight){draft.amplitude=2;draft.frequency=.2f;}
                try{Create(label,draft);}finally{UnityEngine.Object.DestroyImmediate(draft);}
            }
            Debug.Log("Pattern Editor: 8 presets saved and registered. Existing stage patterns unchanged.");
        }
    }
}
