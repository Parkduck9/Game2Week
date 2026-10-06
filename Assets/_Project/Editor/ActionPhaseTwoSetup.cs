using System;
using System.Collections.Generic;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.View;
using Game2Week.Core;
using Game2Week.Data;
using Game2Week.EditorTools.Stages;
using Game2Week.Flow;
using Game2Week.Stages;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game2Week.EditorTools
{
    public static class ActionPhaseTwoSetup
    {
        [MenuItem("Tools/Action Phase 2/Apply Assets and Stages 2-3")]
        public static void Apply()
        {
            ActionPhaseOneSetup.Apply();
            const string sinePath = "Assets/_Project/Prefabs/Battle/Patterns/Bullet_YellowSine.prefab";
            var root = PrefabUtility.LoadPrefabContents("Assets/_Project/Prefabs/Battle/Patterns/Bullet_Yellow.prefab");
            try
            {
                root.name = "Bullet_YellowSine";
                UnityEngine.Object.DestroyImmediate(root.GetComponent<Bullet>());
                var sine = root.AddComponent<SineBullet>(); Set(sine,"parryable",p=>p.boolValue=true);
                PrefabUtility.SaveAsPrefabAsset(root,sinePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            MakePattern("Pattern_YellowSine","노랑 사인파 · 랜덤 조준",sinePath,false);
            MakePattern("Pattern_YellowSide","노랑 정면/측면 교대",sinePath,true);
            var catalog = ContentCatalogUtility.LoadOrCreate(); ContentCatalogUtility.Refresh(catalog);
            var maps = new StageEditorModel(new StageRepository(StageRepository.DefaultDirectory),catalog);
            maps.Load();
            if (maps.LoadProblems.Count>0) throw new InvalidOperationException(string.Join("\n",maps.LoadProblems));
            maps.SelectStage(1); maps.Edit(stage=>stage.enemyTurn.patterns=new List<string>{"Pattern_YellowSine"});
            maps.SelectStage(2); maps.Edit(stage=>stage.enemyTurn.patterns=new List<string>{"Pattern_YellowSide"});
            if (maps.SaveAll().Blocked.Count>0) throw new InvalidOperationException("2단계 맵 검증 실패");
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Battle.unity");
            var world = UnityEngine.Object.FindAnyObjectByType<BattleWorld>();
            var camera = UnityEngine.Object.FindAnyObjectByType<BattleCameraDirector>();
            Set(camera,"shoulderOffset",p=>p.floatValue=.70f);
            var pause = UnityEngine.Object.FindAnyObjectByType<PauseMenu>();
            var uiRoot = (GameObject)new SerializedObject(pause).FindProperty("battleUiRoot").objectReferenceValue;
            var existing = uiRoot.transform.Find("ActionFeedback");
            if (existing) UnityEngine.Object.DestroyImmediate(existing.gameObject);
            var overlay = new GameObject("ActionFeedback",typeof(RectTransform)); overlay.transform.SetParent(uiRoot.transform,false);
            var rect = (RectTransform)overlay.transform;
            rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero;
            var feedback = world.GetComponent<ThreatFeedbackView>(); if (!feedback) feedback=world.gameObject.AddComponent<ThreatFeedbackView>();
            var indicators = new TMP_Text[3];
            for(int i=0;i<3;i++) indicators[i]=Text(overlay.transform,$"Danger_{i}",new Vector2(.5f,.5f),new Vector2(160f,48f));
            var serialized = new SerializedObject(feedback); var array = serialized.FindProperty("indicators"); array.arraySize=3;
            for(int i=0;i<3;i++) array.GetArrayElementAtIndex(i).objectReferenceValue=indicators[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Set(world,"threatFeedback",p=>p.objectReferenceValue=feedback);
            var statusGo = new GameObject("ActionStatus",typeof(ActionStatusView)); statusGo.transform.SetParent(overlay.transform,false);
            var status = statusGo.GetComponent<ActionStatusView>();
            Set(status,"world",p=>p.objectReferenceValue=world);
            Set(status,"cameraDirector",p=>p.objectReferenceValue=camera);
            Set(status,"input",p=>p.objectReferenceValue=AssetDatabase.LoadAssetAtPath<InputReader>("Assets/_Project/Input/InputReader.asset"));
            var statusText = Text(overlay.transform,"Status",new Vector2(.5f,.14f),new Vector2(950f,44f));
            statusText.color=new Color(.82f,.88f,.98f);
            Set(status,"label",p=>p.objectReferenceValue=statusText);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("Action Phase 2: 사인파/랜덤/측면 패턴, 경고음/화면 밖 표시, 1-2~1-3 연결 완료");
        }
        static TMP_Text Text(Transform parent,string name,Vector2 anchor,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform)); go.transform.SetParent(parent,false);
            var text=go.AddComponent<TextMeshProUGUI>(); text.font=TMP_Settings.defaultFontAsset;
            text.fontSize=24f; text.color=new Color(1f,.82f,.15f); text.alignment=TextAlignmentOptions.Center; text.raycastTarget=false;
            text.rectTransform.anchorMin=text.rectTransform.anchorMax=anchor; text.rectTransform.sizeDelta=size; return text;
        }
        static void MakePattern(string name,string label,string bulletPath,bool side)
        {
            var root=new GameObject(name);
            try
            {
                var pattern=root.AddComponent<YellowTrainingPattern>();
                Set(pattern,"bulletPrefab",p=>p.objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(bulletPath).GetComponent<Bullet>());
                Set(pattern,"warningMaterial",p=>p.objectReferenceValue=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Placeholder/M_YellowAttack.mat"));
                Set(pattern,"aimRandomRadius",p=>p.floatValue=.25f); Set(pattern,"alternateSideOrigins",p=>p.boolValue=side);
                Set(pattern,"warningWaveAmplitude",p=>p.floatValue=.38f);
                Set(pattern,"warningDuration",p=>p.floatValue=side?1.05f:.85f);
                Set(pattern,"interval",p=>p.floatValue=side?1.65f:1.5f);
                PrefabUtility.SaveAsPrefabAsset(root,$"Assets/_Project/Prefabs/Battle/Patterns/{name}.prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            string assetPath=$"Assets/_Project/Data/Patterns/{name}.asset";
            var data=AssetDatabase.LoadAssetAtPath<AttackPatternData>(assetPath);
            if(!data){data=ScriptableObject.CreateInstance<AttackPatternData>();AssetDatabase.CreateAsset(data,assetPath);}
            Set(data,"displayName",p=>p.stringValue=label);
            Set(data,"patternPrefab",p=>p.objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Prefabs/Battle/Patterns/{name}.prefab"));
            AssetDatabase.SaveAssets();
        }
        static void Set(UnityEngine.Object target,string property,Action<SerializedProperty> change)
        {
            var serialized=new SerializedObject(target);change(serialized.FindProperty(property));serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(target);
        }
    }
}
