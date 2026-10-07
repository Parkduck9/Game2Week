using System;
using System.Linq;
using Game2Week.Battle.Patterns;
using Game2Week.Data;
using Game2Week.Data.Patterns;
using Game2Week.EditorTools.Stages;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Patterns
{
    public static class SequenceAssetStore
    {
        public const string Root = "Assets/_Project/Data/Patterns/Sequences";
        public const string Prefabs = "Assets/_Project/Prefabs/Battle/Patterns/Sequences";
        public static AttackSequence[] List() => AssetDatabase.FindAssets("t:AttackSequence")
            .Select(id => AssetDatabase.LoadAssetAtPath<AttackSequence>(AssetDatabase.GUIDToAssetPath(id)))
            .OrderBy(asset => asset.name).ToArray();
        public static AttackSequence Create()
        {
            PatternAssetStore.Folder(Root);
            var sequence = ScriptableObject.CreateInstance<AttackSequence>();
            AssetDatabase.CreateAsset(sequence, AssetDatabase.GenerateUniqueAssetPath(Root + "/새시퀀스.asset"));
            return sequence;
        }
        public static AttackPatternData Save(AttackSequence sequence)
        {
            var errors = SequenceSchedule.Validate(sequence);
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
            string path = AssetDatabase.GetAssetPath(sequence);
            if (!path.StartsWith(Root + "/", StringComparison.Ordinal)) throw new InvalidOperationException("시퀀스 폴더의 에셋만 저장합니다.");
            PatternAssetStore.Folder(Prefabs);
            string stem = System.IO.Path.GetFileNameWithoutExtension(path);
            string prefabPath = Prefabs + "/" + stem + ".prefab";
            var root = new GameObject(stem);
            try { root.AddComponent<SequencePattern>().Configure(sequence); PrefabUtility.SaveAsPrefabAsset(root, prefabPath); }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            string attackPath = Root + "/Attack_" + stem + ".asset";
            var attack = AssetDatabase.LoadAssetAtPath<AttackPatternData>(attackPath);
            if (!attack) { attack = ScriptableObject.CreateInstance<AttackPatternData>(); AssetDatabase.CreateAsset(attack, attackPath); }
            var data = new SerializedObject(attack);
            data.FindProperty("displayName").stringValue = stem;
            data.FindProperty("patternPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(sequence); AssetDatabase.SaveAssets();
            ContentCatalogUtility.Refresh(ContentCatalogUtility.LoadOrCreate());
            return attack;
        }
        [MenuItem("Tools/Pattern Editor Presets/시퀀스 샘플 생성")]
        public static void GenerateSample()
        {
            PatternAssetStore.Folder(Root);
            var sequence = AssetDatabase.LoadAssetAtPath<AttackSequence>(Root + "/고리_부채저격.asset");
            if (!sequence)
            {
                sequence = ScriptableObject.CreateInstance<AttackSequence>();
                AssetDatabase.CreateAsset(sequence, Root + "/고리_부채저격.asset");
                sequence.entries.Add(new SequenceEntry { pattern = SampleGraph("Sequence_Ring", ShotLayout.Ring, 12, 360), duration = 6 });
                sequence.entries.Add(new SequenceEntry { time = 1.2f, pattern = SampleGraph("Sequence_Fan", ShotLayout.Fan, 3, 22),
                    repeat = true, count = 3, interval = 1.2f, duration = 6 });
            }
            var attack = Save(sequence);
            var enemy = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test2.asset");
            var serialized = new SerializedObject(enemy); var moves = serialized.FindProperty("phaseMoves");
            bool found = false;
            for (int i = 0; i < moves.arraySize; i++) found |= moves.GetArrayElementAtIndex(i).objectReferenceValue == attack;
            if (!found) { int i = moves.arraySize; moves.InsertArrayElementAtIndex(i); moves.GetArrayElementAtIndex(i).objectReferenceValue = attack; }
            serialized.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssets();
            Debug.Log("고리 뒤 부채꼴 저격 세 번 샘플을 주황 적의 체력 단계 기술에 등록했습니다.");
        }
        static AttackPatternData SampleGraph(string label, ShotLayout layout, int count, float spread)
        {
            var graph = AssetDatabase.LoadAssetAtPath<GraphPatternDefinition>(PatternAssetStore.Root + "/" + label + ".asset");
            if (!graph)
            {
                var draft = ScriptableObject.CreateInstance<GraphPatternDefinition>();
                draft.layout = layout; draft.shotCount = count; draft.spreadDegrees = spread;
                draft.interval = AnimationCurve.Constant(0, 30, 30);
                draft.warningSeconds = .4f;
                try { graph = PatternAssetStore.Create(label, draft); }
                finally { UnityEngine.Object.DestroyImmediate(draft); }
            }
            return PatternAssetStore.Save(graph);
        }
    }
}
