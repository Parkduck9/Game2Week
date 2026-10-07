using System;
using Game2Week.Battle.View;
using Game2Week.Dialogue;
using Game2Week.EditorTools.Dialogue;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Animation
{
    public static class AnimationPhaseAuthoring
    {
        [MenuItem("Tools/Heroine/12단계 동작과 대화 자세 다시 구성")]
        public static void Build()
        {
            Game2Week.EditorTools.Build.GraphicsAuthoring.RepairPalette();
            HeroineClipAuthoring.Build();
            const string path="Assets/_Project/Prefabs/Battle/FX/BattleFxRig.prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            try {if(!root.TryGetComponent<EnemyMotionFxModule>(out var module))root.AddComponent<EnemyMotionFxModule>();PrefabUtility.SaveAsPrefabAsset(root,path);}
            finally{PrefabUtility.UnloadPrefabContents(root);}
            var model=new DialogueEditorModel(new DialogueRepository(DialogueRepository.DefaultDirectory));
            string[] files={"dlg_stage_001_intro","dlg_stage_001_spare","dlg_stage_001_win","dlg_stage_002_intro","dlg_stage_002_phase2"};
            string[] poses={"nod","spare","victory","talk","surprise"};
            for(int i=0;i<files.Length;i++)
            {
                model.Load(files[i]);string pose=poses[i];
                model.Edit(definition=>{foreach(var node in definition.nodes){if(node.end)continue;node.pose=node.speaker=="enemy"?"talk":pose;if(node.speaker!="enemy")node.camera="player_close";}});model.Save();
            }
            AssetDatabase.Refresh();AssetDatabase.SaveAssets();Debug.Log("12단계 동작과 대화 자세 구성 완료");
        }
        public static void BuildFromCommandLine()
        {try{Build();EditorApplication.Exit(0);}catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}}
    }
}
