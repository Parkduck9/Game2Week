using System;
using System.Collections.Generic;
using System.Linq;
using Game2Week.Battle;
using Game2Week.Data;
using Game2Week.Dialogue;
using Game2Week.EditorTools.Stages;
using Game2Week.Stages;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Dialogue
{
    public static class DialogueSampleAuthoring
    {
        [MenuItem("Tools/Dialogue Editor/샘플 대화와 자비 조건 만들기")]
        public static void Generate()
        {
            var repository = new DialogueRepository(DialogueRepository.DefaultDirectory);
            Create(repository, "dlg_stage_001_intro", "테스트 적", "여기까지 온 거야?", "싸우러 온 것은 아니야.", true);
            Create(repository, "dlg_stage_001_spare", "테스트 적", "이제 마음이 조금 편해졌어.", "서로 이야기를 해 보자.", false);
            Create(repository, "dlg_stage_001_win", "테스트 적", "다음에 다시 만나자.", "잘 있어.", false);
            Create(repository, "dlg_stage_002_intro", "주황 테스트 적", "내 움직임을 따라올 수 있어?", "같이 뛰어 보자.", true);
            Create(repository, "dlg_stage_002_phase2", "주황 테스트 적", "이제 조금 더 빠르게 갈 거야.", "움직임을 잘 볼게.", false);
            var first = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test.asset");
            var second = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test2.asset");
            SetRule(first, new ActSequenceCondition { acts = new List<string> { "응원하기", "말 걸기" } }, new DialogueFlagCondition { flag = "listened" });
            // 처음 선택에서 미뤘더라도 이후 말 걸기 행동으로 이야기를 들을 수 있다.
            var serializedFirst = new SerializedObject(first);
            int talkIndex = first.Acts.ToList().FindIndex(act => act.DisplayName == "말 걸기");
            if (talkIndex >= 0)
            {
                var flags = serializedFirst.FindProperty("acts").GetArrayElementAtIndex(talkIndex).FindPropertyRelative("spareFlags");
                flags.arraySize = 1; flags.GetArrayElementAtIndex(0).stringValue = "listened"; serializedFirst.ApplyModifiedPropertiesWithoutUndo();
            }
            SetRule(second, new BulletActionCondition { action = SpareActionKind.Parry, required = 3 }, new ActSequenceCondition { acts = new List<string> { "같이 뛰기" } });
            var catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>("Assets/_Project/Data/ContentCatalog.asset");
            var stages = new StageEditorModel(new StageRepository(StageRepository.DefaultDirectory), catalog); stages.Load();
            stages.SelectStage(0); stages.Edit(stage => stage.dialogues = new StageDialogues { intro = "dlg_stage_001_intro", spareReady = "dlg_stage_001_spare", victory = "dlg_stage_001_win" });
            stages.SelectStage(1); stages.Edit(stage => stage.dialogues = new StageDialogues { intro = "dlg_stage_002_intro", phase2 = "dlg_stage_002_phase2" });
            var report = stages.SaveAll(); if (report.Blocked.Count > 0) throw new InvalidOperationException(string.Join("\n", report.Blocked));
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            // 게임 문구에 필요한 한글과 마음 단계 기호를 정적 폰트에 포함한다.
            Build.ReleasePreparation.Prepare();
            Debug.Log("10단계 샘플 대화 5개와 자비 조건 저장 완료. 데미지·체력 미변경.");
        }
        static void SetRule(EnemyData enemy, params ISpareCondition[] conditions)
        {
            if (!enemy) throw new InvalidOperationException("샘플 적 데이터가 없습니다.");
            enemy.SpareRule.enabled = true; enemy.SpareRule.noFightTurns = 3; enemy.SpareRule.halveOnFight = false;
            enemy.SpareRule.conditions = conditions.ToList(); EditorUtility.SetDirty(enemy);
        }
        static void Create(DialogueRepository repository, string id, string enemyName, string enemyText, string playerText, bool choices)
        {
            if (repository.List().Contains(id)) return;
            var model = new DialogueEditorModel(repository); model.New(id);
            model.Edit(definition =>
            {
                definition.speakers["enemy"] = new DialogueSpeaker { name = enemyName, anchor = "enemy" };
                definition.nodes = new List<DialogueNode>
                {
                    new() { id = "n1", speaker = "enemy", text = enemyText, camera = "enemy_close", next = "n2" },
                    new() { id = "n2", speaker = "heroine", text = playerText, camera = "two_shot", next = "end" },
                    new() { id = "end", end = true },
                };
                if (choices)
                    definition.nodes[1].choices = new List<DialogueChoice>
                    {
                        new() { text = "이야기를 들어 준다", next = "end", effects = new List<DialogueEffect> { new() { type = "spareCondition", id = "listened", value = true } } },
                        new() { text = "나중에 다시 이야기한다", next = "end", effects = new List<DialogueEffect> { new() { type = "flag", id = "listened", value = false } } },
                    };
            });
            model.Save();
        }
    }
}
