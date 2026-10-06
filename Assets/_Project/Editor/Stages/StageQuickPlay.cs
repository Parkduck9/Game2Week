using System;
using System.Linq;
using Game2Week.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game2Week.EditorTools.Stages
{
    [InitializeOnLoad]
    public static class StageQuickPlay
    {
        const string Key="Game2Week.StageQuickPlay";
        [Serializable] sealed class SavedScene{public string path;public bool loaded,active;}
        [Serializable] sealed class SavedSetup{public SavedScene[] scenes;}
        static StageQuickPlay(){EditorApplication.playModeStateChanged+=OnPlayState;}
        public static bool Start(StageEditorModel model)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||model.Current==null)return false;
            if(model.ValidateCurrent().Any(i=>i.Severity==Game2Week.Stages.IssueSeverity.Error))return false;
            if(model.SaveAll().Blocked.Count>0)return false;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return false;
            var setup=new SavedSetup{scenes=EditorSceneManager.GetSceneManagerSetup().Select(s=>new SavedScene{path=s.path,loaded=s.isLoaded,active=s.isActive}).ToArray()};
            SessionState.SetString(Key+"Scenes",JsonUtility.ToJson(setup));SessionState.SetInt(Key,model.CurrentIndex);
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/"+SceneNames.Battle+".unity");EditorApplication.isPlaying=true;return true;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void PrepareStage()
        {
            int index=SessionState.GetInt(Key,-1);if(index<0)return;
            var session=AssetDatabase.LoadAssetAtPath<GameSession>("Assets/_Project/Data/GameSession.asset");
            if(!session.BeginStage(index))Debug.LogError("맵툴 바로 플레이: 스테이지를 불러오지 못했습니다.");
        }
        static void OnPlayState(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredEditMode||SessionState.GetInt(Key,-1)<0)return;
            var saved=JsonUtility.FromJson<SavedSetup>(SessionState.GetString(Key+"Scenes",""));SessionState.EraseInt(Key);SessionState.EraseString(Key+"Scenes");
            if(saved?.scenes==null)return;
            var setup=saved.scenes.Where(s=>!string.IsNullOrEmpty(s.path)).Select(s=>new SceneSetup{path=s.path,isLoaded=s.loaded,isActive=s.active}).ToArray();
            if(setup.Length>0)EditorSceneManager.RestoreSceneManagerSetup(setup);else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }
    }
}
