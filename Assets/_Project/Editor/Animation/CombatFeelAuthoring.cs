using System;
using Game2Week.Battle;
using Game2Week.Battle.View;
using Game2Week.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game2Week.EditorTools.Animation
{
    public static class CombatFeelAuthoring
    {
        public const string SettingsPath="Assets/_Project/Data/CombatPresentationSettings.asset";
        [MenuItem("Tools/Heroine/타격감과 공동 행동 다시 구성")]
        public static void Build()
        {
            var settings=AssetDatabase.LoadAssetAtPath<CombatPresentationSettings>(SettingsPath);
            if(!settings){settings=ScriptableObject.CreateInstance<CombatPresentationSettings>();AssetDatabase.CreateAsset(settings,SettingsPath);}
            HeroineClipAuthoring.Build();
            foreach(var path in new[]{"Assets/_Project/Prefabs/Enemies/EnemyView_TestBlob.prefab","Assets/_Project/Prefabs/Enemies/EnemyView_TestBlob_Orange.prefab"})
            {
                var root=PrefabUtility.LoadPrefabContents(path);
                try{if(!root.TryGetComponent<EnemyFlight>(out var flight))root.AddComponent<EnemyFlight>();PrefabUtility.SaveAsPrefabAsset(root,path);}
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            var first=AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test.asset");
            var second=AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test2.asset");
            Assign(first,new[]{ActionCue.Cheer,ActionCue.Talk});Assign(second,new[]{ActionCue.Cheer,ActionCue.RunTogether,ActionCue.Talk});AddPlay(first);AddPlay(second);
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene("Assets/_Project/Scenes/Battle.unity",OpenSceneMode.Single);
                foreach(var root in scene.GetRootGameObjects())foreach(var world in root.GetComponentsInChildren<BattleWorld>(true))
                {var serialized=new SerializedObject(world);serialized.FindProperty("presentationSettings").objectReferenceValue=AssetDatabase.LoadAssetAtPath<CombatPresentationSettings>(SettingsPath);serialized.ApplyModifiedPropertiesWithoutUndo();}
                EditorSceneManager.SaveScene(scene);
            }
            finally{if(setup.Length>0&&Array.TrueForAll(setup,item=>!string.IsNullOrEmpty(item.path)))EditorSceneManager.RestoreSceneManagerSetup(setup);else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);}
            AssetDatabase.SaveAssets();Debug.Log("타격감·공동 행동·후퇴 연출 구성 완료");
        }
        static void AddPlay(EnemyData enemy)
        {
            foreach(var act in enemy.Acts)if(act.Motion==ActionCue.Play)return;
            var serialized=new SerializedObject(enemy);var acts=serialized.FindProperty("acts");var entry=acts.GetArrayElementAtIndex(acts.arraySize++);
            entry.FindPropertyRelative("displayName").stringValue="같이 놀기";
            entry.FindPropertyRelative("resultText").stringValue="* 함께 통통 뛰며 놀았다.\n* 조금 더 편해 보인다.";
            entry.FindPropertyRelative("spareProgress").intValue=0;entry.FindPropertyRelative("spareFlags").arraySize=0;entry.FindPropertyRelative("motion").enumValueIndex=(int)ActionCue.Play;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        static void Assign(EnemyData enemy,ActionCue[] cues)
        {
            var serialized=new SerializedObject(enemy);var acts=serialized.FindProperty("acts");
            for(int i=0;i<acts.arraySize&&i<cues.Length;i++)acts.GetArrayElementAtIndex(i).FindPropertyRelative("motion").enumValueIndex=(int)cues[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        public static void BuildFromCommandLine(){try{Build();EditorApplication.Exit(0);}catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}}
    }
}
