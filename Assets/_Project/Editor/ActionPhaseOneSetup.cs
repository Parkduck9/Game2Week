using System;
using System.Collections.Generic;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.View;
using Game2Week.Data;
using Game2Week.EditorTools.Stages;
using Game2Week.Stages;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game2Week.EditorTools
{
    /// <summary>1단계의 에셋/씬/맵 연결. 맵은 반드시 맵툴 모델로 저장한다.</summary>
    public static class ActionPhaseOneSetup
    {
        [MenuItem("Tools/Action Phase 1/Apply Assets and Stage 1")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Play를 중지한 뒤 실행하세요.");
            var settings = Asset<PlayerActionSettings>("Assets/_Project/Data/PlayerActionSettings.asset");
            EditorUtility.SetDirty(settings);
            const string materialPath = "Assets/_Project/Art/Placeholder/M_YellowAttack.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (!material)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = new Color(1f, 0.8f, 0.06f);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(1f, 0.65f, 0.05f));
                AssetDatabase.CreateAsset(material, materialPath);
            }
            const string bulletPath = "Assets/_Project/Prefabs/Battle/Patterns/Bullet_Yellow.prefab";
            var bulletRoot = PrefabUtility.LoadPrefabContents("Assets/_Project/Prefabs/Battle/Patterns/Bullet_Teal.prefab");
            try
            {
                bulletRoot.name = "Bullet_Yellow";
                foreach (var r in bulletRoot.GetComponentsInChildren<Renderer>()) r.sharedMaterial = material;
                Set(bulletRoot.GetComponent<Bullet>(), "parryable", p => p.boolValue = true);
                PrefabUtility.SaveAsPrefabAsset(bulletRoot, bulletPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(bulletRoot); }
            var root = new GameObject("Pattern_YellowTraining");
            try
            {
                var pattern = root.AddComponent<YellowTrainingPattern>();
                Set(pattern, "bulletPrefab", p => p.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPath).GetComponent<Bullet>());
                Set(pattern, "warningMaterial", p => p.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(materialPath));
                PrefabUtility.SaveAsPrefabAsset(root, "Assets/_Project/Prefabs/Battle/Patterns/Pattern_YellowTraining.prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            var data = Asset<AttackPatternData>("Assets/_Project/Data/Patterns/Pattern_YellowTraining.asset");
            Set(data, "displayName", p => p.stringValue = "노랑 직선 · 쳐내기 연습");
            Set(data, "patternPrefab", p => p.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Battle/Patterns/Pattern_YellowTraining.prefab"));
            AssetDatabase.SaveAssets();
            var catalog = ContentCatalogUtility.LoadOrCreate(); ContentCatalogUtility.Refresh(catalog);
            var maps = new StageEditorModel(new StageRepository(StageRepository.DefaultDirectory), catalog);
            maps.Load();
            if (maps.LoadProblems.Count > 0) throw new InvalidOperationException(string.Join("\n", maps.LoadProblems));
            maps.Edit(stage => stage.enemyTurn.patterns = new List<string> { data.name });
            var report = maps.SaveAll();
            if (report.Blocked.Count > 0) throw new InvalidOperationException("1-1 저장 검증 실패");
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Battle.unity");
            var world = UnityEngine.Object.FindAnyObjectByType<BattleWorld>();
            Set(world, "actionSettings", p => p.objectReferenceValue = settings);
            var mainCamera = Camera.main;
            if (mainCamera && !mainCamera.TryGetComponent<AudioListener>(out _)) mainCamera.gameObject.AddComponent<AudioListener>();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("Action Phase 1: 입력/액션 설정/노랑 시험 패턴/1-1/카메라 씬 연결 완료");
        }
        static T Asset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset) return asset;
            asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
        }
        static void Set(UnityEngine.Object target, string property, Action<SerializedProperty> change)
        {
            var serialized = new SerializedObject(target);
            change(serialized.FindProperty(property)); serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
